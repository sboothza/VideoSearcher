import argparse
import datetime

from flask import Flask, request, make_response
from sb_db_common import SessionFactory
from sb_pubsub_kafka import HardSerializer, PubSub

from messages import (
    GeneralRestMessage,
    VideoDownloaded,
    VideoDeleted,
    VideoManualAdd,
    VideoError,
)

app = Flask(__name__)

serializer: HardSerializer = HardSerializer()
connection_string = ""
# "mysql://root:or9asm1c@localhost/video"


@app.route("/")
def index():
    msg = GeneralRestMessage(True, 1, "Welcome to the database server")
    json = serializer.serialize(msg)
    return json


@app.route("/url", methods=["GET"])
def url():
    url_to_check = request.args.get("url")
    msg: GeneralRestMessage
    if url_to_check is None:
        msg = GeneralRestMessage(False, 500, "url is required")
    else:
        with SessionFactory.connect(connection_string) as session:
            sql = "select count(*) from video_catalog where url = %(url)s;"
            if session.fetch_scalar(sql, {"url": url_to_check}) == 1:
                msg = GeneralRestMessage(True, 200, f"url found [{url_to_check}]")
            else:
                msg = GeneralRestMessage(False, 404, f"url not found [{url_to_check}]")

    json = serializer.serialize(msg)
    return make_response(json, msg.code)


def handle_video_downloaded(pubsub, obj, context):
    try:
        msg: VideoDownloaded = obj
        with SessionFactory.connect(connection_string) as session:
            sql = (
                "insert into video_catalog (url, video_id, title, uploader, width, height, duration, complete, filename, "
                "retries, errors, in_progress, translated, deleted, size, insertdate, downloaddate, description, tags, "
                "override) "
                "values (%(url)s, %(video_id)s, %(title)s, %(uploader)s, %(width)s, %(height)s, %(duration)s, "
                "%(complete)s, %(filename)s, %(retries)s, %(errors)s, %(in_progress)s, %(translated)s, %(deleted)s, "
                "%(size)s, %(insertdate)s, %(downloaddate)s, %(description)s, %(tags)s, %(override)s) returning id;"
            )
            params = {
                "url": msg.url,
                "video_id": msg.video_id,
                "title": msg.title,
                "uploader": msg.uploader,
                "width": msg.width,
                "height": msg.height,
                "duration": msg.duration,
                "complete": 1,
                "filename": msg.filename,
                "retries": 0,
                "errors": "",
                "in_progress": 0,
                "translated": 0,
                "deleted": 0,
                "size": msg.size,
                "insertdate": datetime.datetime.now(),
                "downloaddate": msg.download_date,
                "description": msg.description,
                "tags": " ".join(msg.tags),
                "override": 0,
            }

            id = session.execute_lastrowid(sql, params)
            print(f"video inserted - {id}")
    except Exception as ex:
        if "Duplicate entry" in str(ex) or "duplicate key" in str(ex):
            pass
        else:
            print(ex)


def handle_video_error(pubsub, obj, context):
    msg: VideoError = obj
    with SessionFactory.connect(connection_string) as session:
        sql = (
            "insert into video_error (url, video_id, title, uploader, width, height, duration, retries, errors, "
            "size, insertdate, description, tags) "
            "values (%(url)s, %(video_id)s, %(title)s, %(uploader)s, %(width)s, %(height)s, %(duration)s, "
            "%(retries)s, %(errors)s, %(size)s, %(insertdate)s, %(description)s, %(tags)s) RETURNING id;"
        )
        params = {
            "url": msg.url,
            "video_id": msg.video_id,
            "title": msg.title,
            "uploader": msg.uploader,
            "width": msg.width,
            "height": msg.height,
            "duration": msg.duration,
            "retries": msg.retries,
            "errors": msg.errors,
            "size": msg.size,
            "insertdate": datetime.datetime.now(),
            "description": msg.description,
            "tags": " ".join(msg.tags),
        }

        id = session.execute_lastrowid(sql, params)
        print(f"error video inserted - {id}")


def handle_video_deleted(pubsub, obj, context):
    msg: VideoDeleted = obj
    with SessionFactory.connect(connection_string) as session:
        sql = "delete from video_catalog where url = %(url)s;"
        params = {"url": msg.url}
        session.execute(sql, params)
        print(f"video deleted [{msg.url}]")


def handle_video_manual_added(pubsub, obj, context):
    msg: VideoManualAdd = obj
    with SessionFactory.connect(connection_string) as session:
        sql = (
            "insert into video_catalog (url, video_id, title, uploader, width, height, duration, complete, "
            "filename, retries, errors, in_progress, translated, deleted, size, insertdate, downloaddate, "
            "description, tags, override) "
            "values (%(url)s, %(video_id)s, %(title)s, %(uploader)s, %(width)s, %(height)s, %(duration)s, "
            "%(complete)s, %(filename)s, %(retries)s, %(errors)s, %(in_progress)s, %(translated)s, %(deleted)s, "
            "%(size)s, %(insertdate)s, %(downloaddate)s, %(description)s, %(tags)s, %(override)s);"
        )
        params = {
            "url": msg.filename,
            "video_id": 0,
            "title": msg.filename,
            "uploader": msg.uploader,
            "width": msg.width,
            "height": msg.height,
            "duration": msg.duration,
            "complete": 1,
            "filename": msg.filename,
            "retries": 0,
            "errors": "",
            "in_progress": 0,
            "translated": 0,
            "deleted": 0,
            "size": msg.size,
            "insertdate": datetime.datetime.now(),
            "downloaddate": datetime.datetime.now(),
            "description": msg.filename,
            "tags": "",
            "override": 0,
        }

        id = session.execute_lastrowid(sql, params)
        print(f"video manually inserted - {id}")


def handle_everything(pubsub, obj, context):
    pass


if __name__ == "__main__":

    # json = (
    #     "{"
    #     '"CorrelationId": "e2cb1904-2fe2-4343-80f7-92deb7b66e0e",'
    #     '"Identifier": "VideoDownloaded",'
    #     '"description": "N/A",'
    #     '"download_date": "2023-10-12 15:49:22.634236",'
    #     '"duration": 612,'
    #     '"filename": "PornHub_Teen_Zoey_Kush_Sucks_Fucks_And_Swal_896994681_480.mp4",'
    #     '"height": 480,'
    #     '"size": 45994905,'
    #     '"tags": "small tits zoey kush skinny teens petite teen swallow blowjob newsensations.com panties tiny tits shaved pussy brunette socks tight",'
    #     '"title": "Teen Zoey Kush Sucks Fucks And Swallows",'
    #     '"uploader": "NewSensations",'
    #     '"url": "https://www.pornhub.com/view_video.php?viewkey=896994681",'
    #     '"video_id": "896994681",'
    #     '"width": 624'
    #     "}"
    # )
    #
    # obj = serializer.de_serialize(json, VideoDownloaded)
    # print(obj)
    # new_json = serializer.serialize(obj)
    # print(new_json)

    parser = argparse.ArgumentParser(description="DB Server for downloader")
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
        default="db-server",
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

    connection_string = args.connection_string
    pubsub = PubSub(serializer, args.topic, args.group, args.servers, offset="earliest")
    pubsub.bind(VideoDownloaded, handle_video_downloaded)
    pubsub.bind(VideoDeleted, handle_video_deleted)
    pubsub.bind(VideoManualAdd, handle_video_manual_added)
    pubsub.bind(VideoError, handle_video_error)
    pubsub.bind_everything(handle_everything)
    pubsub.start()
    app.run(host="0.0.0.0")
