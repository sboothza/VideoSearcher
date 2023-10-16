import argparse

from sb_pubsub_kafka import HardSerializer, PubSub

from python.messages import (
    QueueVideo,
    VideoDownloaded,
    VideoManualAdd,
    VideoError,
    VideoDeleted,
)


def handle_everything(pubsub, obj):
    pass


def main():
    parser = argparse.ArgumentParser(description="cli for downloader")
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
        default="downloader-cli",
        required=False,
    )
    parser.add_argument(
        "--servers",
        help="servers",
        dest="servers",
        default="localhost:9092",
        required=False,
    )

    args = parser.parse_args()

    serializer = HardSerializer()

    pubsub = PubSub(serializer, args.topic, args.group, args.servers)
    pubsub.bind_everything(handle_everything)

    pubsub.create_topic(args.topic, 1)

    done = False
    print("cli")
    while not done:
        print("> ", end="")
        line = input()
        words = line.split(" ")
        if words[0] == "quit":
            done = True
        elif words[0] == "help":
            print("Help")
            print("queue - queue sample video")
            print("downloaded - trigger video downloaded")
            print("manual - trigger manual add")
            print("error - trigger error")
            print("delete - trigger delete")
            print("quit - exit")
        elif words[0] == "queue":
            req = QueueVideo(url=words[1])
            pubsub.publish(req)
        elif words[0] == "downloaded":
            req = VideoDownloaded(
                url="https://xhamster.com/videos/hot-teen-ass-in-panties-873041",
                video_id="873041",
                title="Hot teen ass in panties",
                uploader="everestcash",
                width=426,
                height=240,
                duration=360,
                filename="XHamster_Hot_teen_ass_in_panties_873041_240.mp4",
                size=14447665,
                description="There are many great upskirt views in the Emily 18 video. There are shots of her in boots as well and she wears colorful panties that look damn good.",
                tags="There are many great upskirt views in the Emily 18 video. There are shots of her in boots as well and she wears colorful panties that look damn good.",
            )
            pubsub.publish(req)
        elif words[0] == "manual":
            req = VideoManualAdd(
                filename="XHamster_Hot_teen_ass_in_panties_873041_240.mp4",
                uploader="everestcash",
                width=426,
                height=240,
                duration=360,
                size=14447665,
            )
            pubsub.publish(req)
        elif words[0] == "error":
            req = VideoError(
                url="https://xhamster.com/videos/hot-teen-ass-in-panties-873041",
                video_id="873041",
                title="Hot teen ass in panties",
                uploader="everestcash",
                width=426,
                height=240,
                duration=360,
                size=14447665,
                description="There are many great upskirt views in the Emily 18 video. There are shots of her in boots as well and she wears colorful panties that look damn good.",
                tags="There are many great upskirt views in the Emily 18 video. There are shots of her in boots as well and she wears colorful panties that look damn good.",
                errors="big error",
                retries=5,
            )
            pubsub.publish(req)
        elif words[0] == "delete":
            req = VideoDeleted(
                url="https://xhamster.com/videos/hot-teen-ass-in-panties-873041",
            )
            pubsub.publish(req)
        else:
            print("unknown command")

    print("done")
    pubsub.shutdown()
    pubsub.join(1000)


if __name__ == "__main__":
    main()
