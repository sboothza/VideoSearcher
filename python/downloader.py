import argparse
import os
import random
import time

import requests
import yt_dlp
from rich.live import Live
from rich.table import Table
from sb_db_common import SessionFactory, Session
from sb_pubsub_kafka import HardSerializer, PubSub
from sb_threadpool import SimpleThreadpool

from kbhit import KBHit
from messages import QueueVideo
from translate import save_translations
from url_job_queue import UrlJobQueue, UrlConfig, UrlJob
from utils import (
    get_fullname,
    log_verbose,
    download_progress,
    set_outstanding_urls,
    get_outstanding_urls,
    global_progress,
)

pubsub: PubSub
connection_string = "sqlite://url_queue.db"
api_url = ""
session: Session
job_queue: UrlJobQueue


def build_yt_dlp(config: UrlConfig) -> yt_dlp.YoutubeDL:
    ydl_opts = {
        "outtmpl": get_fullname(os.path.join(config.temp_path, config.output_format)),
        "restrictfilenames": True,
        "nooverwrites": True,
        "noplaylist": True,
        "quiet": True,
        "skip_unavailable_fragments": True,
        "noprogress": True,
        "ignore_config": True,
    }

    if config.tools_path != "not_set":
        ydl_opts["ffmpeg_location"] = config.tools_path

    if config.verbose:
        ydl_opts["quiet"] = False
        ydl_opts["noprogress"] = False
        ydl_opts["verbose"] = False
        ydl_opts["no_warnings"] = False
        ydl_opts["ignoreerrors"] = False
    else:
        ydl_opts["progress_hooks"] = [download_progress]
        ydl_opts["no_warnings"] = True
        ydl_opts["ignoreerrors"] = False

    ydl: yt_dlp.YoutubeDL = yt_dlp.YoutubeDL(ydl_opts)
    return ydl


def handle_queue_video(pubsub, obj, context):
    try:
        msg: QueueVideo = obj
        config: UrlConfig = context
        log_verbose(f"received request for {msg.url}", config.verbose)

        # check if it already exists
        url = api_url.replace("%url%", msg.url)
        response = requests.get(url)
        if response.status_code == 404:
            log_verbose(f"queuing request for {msg.url}", config.verbose)
            job_queue.queue_job(UrlJob(url=msg.url))
        else:
            log_verbose(
                f"url {msg.url} has already been downloaded - skipping", config.verbose
            )
    except Exception as ex:
        print(ex)


def handle_everything(pubsub, obj, context):
    pass


def generate_table():
    # print("generate")
    status_table = Table()
    status_table.add_column(f"Title ({get_outstanding_urls()} remaining)")
    status_table.add_column("%")
    status_table.add_column("ETA")
    status_table.add_column("Speed")
    status_table.add_column("Size")
    status_table.add_column("Extractor")
    for prog in global_progress.items():
        status_table.add_row(
            prog[1].title,
            prog[1].completed,
            prog[1].eta,
            prog[1].speed,
            prog[1].size,
            prog[1].extractor,
        )
    return status_table


def main():
    global pubsub
    global api_url
    global session
    global job_queue

    parser = argparse.ArgumentParser(description="Download worker")
    parser.add_argument(
        "--topic",
        help="topic",
        dest="topic",
        default="video-downloader",
        required=False,
    )
    parser.add_argument(
        "--group",
        help="group",
        dest="group",
        default="video-downloader",
        required=False,
    )
    parser.add_argument(
        "--servers",
        help="servers",
        dest="servers",
        default="localhost:9092",
        required=False,
    )
    parser.add_argument(
        "--temp-path",
        help="temp path",
        dest="temp_path",
        default="c:\\temp\\downloader",
        required=False,
    )
    parser.add_argument(
        "--output-format",
        help="output format",
        dest="output_format",
        default="%(uploader)s/%(extractor)s_%(title).35s_%(id)s_%(height)s.%(ext)s",
        required=False,
    )
    parser.add_argument(
        "--download-path",
        help="download path",
        dest="download_path",
        default="not_set",
        required=False,
    )
    parser.add_argument(
        "--translation-file",
        help="Translation file",
        dest="translation_file",
        type=str,
        default="not_set",
        required=False,
    )
    parser.add_argument(
        "--tools-path",
        help="tools path",
        dest="tools_path",
        default="~",
        required=False,
    )
    parser.add_argument(
        "--num-threads",
        help="Number of threads",
        dest="threads",
        type=int,
        default=1,
        required=False,
    )
    parser.add_argument(
        "--api-url",
        help="DB server api url",
        dest="api",
        type=str,
        default="http://localhost:5000/url?url=%url%",
        required=False,
    )
    parser.add_argument(
        "--verbose",
        help="Verbose",
        dest="verbose",
        action="store_true",
        default=False,
        required=False,
    )

    args = parser.parse_args()
    serializer = HardSerializer()

    if args.download_path == "not_set":
        print("download path is required")
        parser.print_help()
        exit(0)

    api_url = args.api

    # table exists
    session = SessionFactory.connect(connection_string)

    config = UrlConfig(
        connection_string=connection_string,
        temp_path=get_fullname(args.temp_path),
        output_format=args.output_format,
        download_path=get_fullname(args.download_path),
        translate=True if args.translation_file != "not_set" else False,
        tools_path=get_fullname(args.tools_path),
        session=session,
        verbose=args.verbose,
    )

    pubsub = PubSub(serializer, args.topic, args.group, args.servers, context=config)
    pubsub.bind(QueueVideo, handle_queue_video)
    pubsub.bind_everything(handle_everything)

    config.pubsub = pubsub

    if not os.path.exists(config.temp_path):
        os.makedirs(config.temp_path)
    if not os.path.exists(config.download_path):
        os.makedirs(config.download_path)

    config.yt_dlp = build_yt_dlp(config)

    pubsub.start()

    thread_count = args.threads
    if config.translate:
        translation_file = get_fullname(args.translation_file)
    else:
        translation_file = None

    job_queue = UrlJobQueue(config, session)

    pool = SimpleThreadpool(job_queue, "Downloader", thread_count, verbose=True)
    terminate = False
    kb = KBHit()
    counter: int = 20
    with Live(generate_table(), refresh_per_second=4) as live:
        while not terminate:
            pool.loop()
            time.sleep(0.5)
            counter -= 1
            if counter == 0:
                counter = 20
                save_translations(translation_file)
                set_outstanding_urls(job_queue.count())

            if not config.verbose:
                live.update(generate_table())

            if kb.kbhit():
                print("shutting down due to interrupt")
                terminate = True

    job_queue.close()
    pool.shutdown()
    print("shutdown complete")

    print("done")
    pubsub.shutdown()
    pubsub.join(1000)
    session.close()


if __name__ == "__main__":

    # from rich.live import Live
    # from rich.table import Table
    #
    # def generate_table() -> Table:
    #     """Make a new table."""
    #     table = Table()
    #     table.add_column("ID")
    #     table.add_column("Value")
    #     table.add_column("Status")
    #
    #     for row in range(random.randint(2, 6)):
    #         value = random.random() * 100
    #         table.add_row(
    #             f"{row}",
    #             f"{value:3.2f}",
    #             "[red]ERROR" if value < 50 else "[green]SUCCESS",
    #         )
    #     return table

    # with Live(generate_table(), refresh_per_second=4) as live:
    #     for _ in range(40):
    #         time.sleep(0.4)
    #         live.update(generate_table())
    main()
