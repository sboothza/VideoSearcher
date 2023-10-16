import argparse

import requests
from sb_pubsub_kafka import HardSerializer, Pub

from python.messages import QueueVideo


def url_exists(url: str, api_url: str):
    url = api_url.replace("%url%", url)
    response = requests.get(url)
    if response.status_code == 404:
        return False
    else:
        return True


def import_list(filename: str, pub: Pub, api_url: str) -> None:
    urls: list[str]
    with open(filename, "r", encoding="utf-8") as f:
        urls = f.readlines()
        f.close()

    total: int = 0
    ok: int = 0
    bad: int = 0

    for url in urls:
        clean_url = url.strip()
        if clean_url.startswith("#") or clean_url == "":
            pass
        else:
            if not url_exists(clean_url, api_url):
                msg = QueueVideo(clean_url)
                pub.publish(msg)
                print(f"added url {clean_url}")
                ok = ok + 1
                total = total + 1
            else:
                print(f"already added {clean_url} -- skipping")
                bad = bad + 1
                total = total + 1

    print("FINISHED IMPORTING")
    print(f"Successfully imported {ok} of {total} - {bad} already imported")


def main():
    global pub
    parser = argparse.ArgumentParser(
        description="Download videos from streaming source"
    )
    parser.add_argument(
        "--import-file",
        help="list of urls",
        dest="import_file",
        default="not_set",
        required=False,
    )
    parser.add_argument(
        "--topic",
        help="topic",
        dest="topic",
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
        "--api-url",
        help="DB server api url",
        dest="api",
        type=str,
        default="http://localhost:5000/url?url=%url%",
        required=False,
    )

    args = parser.parse_args()
    serializer = HardSerializer()

    if args.import_file == "not_set":
        print("import file is required")
        parser.print_help()
        exit(0)

    with Pub(serializer, args.topic, args.servers) as pub:
        pub.create_topic(1)
        import_list(args.import_file, pub, args.api)

    print("done")


if __name__ == "__main__":
    main()
