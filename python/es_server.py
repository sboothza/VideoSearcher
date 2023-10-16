import argparse
import datetime

from elasticsearch import Elasticsearch
from sb_pubsub_kafka import HardSerializer, PubSub

from python.messages import (
    VideoDownloaded,
    VideoDeleted,
    VideoManualAdd,
)
from python.utils import parse_es_connection_string

serializer: HardSerializer = HardSerializer()
connection_string = ""
es: Elasticsearch
index: str


def handle_video_downloaded(pubsub, obj):
    try:
        msg: VideoDownloaded = obj
        doc = {
            "url": msg.url,
            "video_id": msg.video_id,
            "title": msg.title,
            "uploader": msg.uploader,
            "width": msg.width,
            "height": msg.height,
            "duration": msg.duration,
            "filename": msg.filename,
            "size": msg.size,
            "insertdate": datetime.datetime.now(),
            "downloaddate": msg.download_date,
            "description": msg.description,
            "tags": " ".join(msg.tags),
        }

        es.index(index=index, id=msg.url, document=doc)
        print(f"video inserted")
    except Exception as ex:
        print(ex)


def handle_video_deleted(pubsub, obj):
    msg: VideoDeleted = obj
    es.delete(index=index, id=msg.url)
    print(f"video deleted [{msg.url}]")


def handle_video_manual_added(pubsub, obj):
    try:
        msg: VideoManualAdd = obj
        doc = {
            "url": msg.filename,
            "video_id": "",
            "title": msg.filename,
            "uploader": msg.uploader,
            "width": msg.width,
            "height": msg.height,
            "duration": msg.duration,
            "filename": msg.filename,
            "size": msg.size,
            "insertdate": datetime.datetime.now(),
            "downloaddate": datetime.datetime.now(),
            "description": msg.filename,
            "tags": " ",
        }

        es.index(index=index, id=msg.filename, document=doc)

        print(f"video manually inserted - {msg.filename}")
    except Exception as ex:
        print(ex)


def handle_everything(pubsub, obj):
    pass


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="ES Server for downloader")
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
        default="es-server",
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
        "--connection-string",
        help="connection string",
        dest="connection_string",
        default="not_set",
        required=False,
    )
    args = parser.parse_args()

    if args.connection_string == "not_set":
        print("connection string is required")
        parser.print_help()
        exit(0)

    (user, password, hostname, index) = parse_es_connection_string(
        args.connection_string
    )

    es = Elasticsearch(
        hosts=hostname,
        basic_auth=(user, password),
        verify_certs=False,
    )

    pubsub = PubSub(serializer, args.topic, args.group, args.servers)
    pubsub.bind(VideoDownloaded, handle_video_downloaded)
    pubsub.bind(VideoDeleted, handle_video_deleted)
    pubsub.bind(VideoManualAdd, handle_video_manual_added)
    pubsub.bind_everything(handle_everything)
    pubsub.start()
    print("Press enter to close...")
    input()
    print("done")
    pubsub.shutdown()
    pubsub.join(1000)
