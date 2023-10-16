import datetime
import os
import re
import shutil
from pathlib import Path
from typing import Union

import yt_dlp


from messages import VideoDownloaded, QueueVideo
from translate import is_english, translate_string

global_progress = {}
outstanding_urls: int = 0


def get_outstanding_urls():
    return outstanding_urls


def set_outstanding_urls(value: int):
    global outstanding_urls
    outstanding_urls = value


def log_verbose(value: str, verbose: bool):
    if verbose:
        print(value)


class DownloadProgress:
    def __init__(
        self,
        url: str = "",
        completed: str = "",
        eta: str = "",
        speed: str = "",
        size: str = "",
        title: str = "",
        extractor: str = "",
    ):
        self.url = url
        self.completed = completed
        self.eta = eta
        self.speed = speed
        self.size = size
        self.title = title
        self.extractor = extractor


class Video(object):
    id: int
    url: str
    filename: str
    video_id: str
    uploader: str
    title: str
    width: int
    height: int
    duration: int
    size: int
    description: str
    tags: str
    download_date: datetime.datetime
    errors: str
    retries: int

    def __init__(
        self,
        id: int = 0,
        url: str = "",
        filename: str = "",
        video_id: str = "",
        uploader: str = "",
        title: str = "",
        width: int = 0,
        height: int = 0,
        duration: int = 0,
        size: int = 0,
        description: str = "",
        tags: str = "",
        download_date: datetime.datetime = datetime.datetime.now(),
        errors: str = "",
        retries: int = 0,
    ):
        self.id = id
        self.url = url
        self.filename = filename
        self.video_id = video_id
        self.uploader = uploader
        self.title = title
        self.width = width
        self.height = height
        self.duration = duration
        self.size = size
        self.description = description
        self.tags = tags
        self.download_date = download_date
        self.errors = errors
        self.retries = retries

    @staticmethod
    def get_from_message(msg: QueueVideo):
        video = Video(url=msg.url)
        return video

    def get_message(self):
        video_downloaded = VideoDownloaded(
            url=self.url,
            filename=self.filename,
            video_id=self.video_id,
            uploader=self.uploader,
            title=self.title,
            width=self.width,
            height=self.height,
            duration=self.duration,
            size=self.size,
            description=self.description,
            tags=self.tags,
            download_date=self.download_date,
        )
        return video_downloaded

    def translate(self, bypass: bool = False) -> bool:
        translated = False
        if not is_english(self.title, bypass):
            self.title = translate_string(self.title, bypass)
            translated = True
        if not is_english(self.description):
            self.description = translate_string(self.description, bypass)
            translated = True
        if not is_english(self.tags):
            self.tags = translate_string(self.tags, bypass)
            translated = True

        return translated


def get_basename(path: str) -> str:
    p = Path(path)
    p.resolve()
    return p.name


def get_root(path: str) -> str:
    p = Path(path)
    p.resolve()
    return str(p.parent)


def get_fullname(path: str) -> str:
    p = Path(path)
    p.resolve()
    return str(p.expanduser())


def is_null_or_empty(value: str) -> bool:
    return value is None or value == ""


def safe_get(
    obj: object, prop: str, default: Union[str, int, list] = ""
) -> Union[str, int, list]:
    if obj is None:
        return default
    if type(obj) is not dict:
        d = obj.__dict__
    else:
        d = obj
    if prop in d:
        value = d[prop]
        return default if value is None or value == "" else value
    else:
        return default


def parse_es_connection_string(connection_string: str):
    match = re.match(r"(\w+):\/\/(\w+):([^@]+)@(\w+):(\d+)\/(\w+)", connection_string)
    # estls://elastic:vr*+5yWT-tL*124sA1_t@localhost:9200/video      - uses host https
    # es://elastic:vr*+5yWT-tL*124sA1_t@localhost:9200/video         - uses host http
    if match:
        scheme = match.group(1)
        user = match.group(2)
        password = match.group(3)
        hostname = match.group(4)
        port = match.group(5)
        index = match.group(6)
        if scheme == "es":
            hostname = f"http://{hostname}:{port}"
        elif scheme == "estls":
            hostname = f"https://{hostname}:{port}"
        else:
            raise Exception("Unknown scheme")

        return user, password, hostname, index
    else:
        raise Exception("Invalid connection string")


def download_progress(progress):
    try:
        url = progress["info_dict"]["webpage_url"]
        completed = progress["_percent_str"]
        eta = str(safe_get(progress, "_eta_str"))
        speed = str(safe_get(progress, "_speed_str"))
        size = str(safe_get(progress, "_total_bytes_estimate_str"))
        title = progress["info_dict"]["title"]
        extractor = progress["info_dict"]["extractor"]
        global_progress[url] = DownloadProgress(
            url, completed, eta, speed, size, title, extractor
        )
    except Exception as ex:
        pass


def task_download(
    ydl: yt_dlp.YoutubeDL,
    url: str,
    temp_path: str,
    output_format: str,
    download_path: str,
    verbose: bool,
) -> Video:
    msg = Video(url=url)
    try:
        if not verbose:
            global_progress[msg.url] = DownloadProgress(msg.url)

        log_verbose(f"Getting info for {url}", verbose)

        progress = {}
        progress["info_dict"] = {}
        progress["info_dict"]["webpage_url"] = msg.url
        progress["_percent_str"] = "0%"
        progress["_eta_str"] = "0"
        progress["_speed_str"] = "0"
        progress["_total_bytes_estimate_str"] = "0"
        progress["info_dict"]["title"] = msg.url
        progress["info_dict"]["extractor"] = "unknown"
        download_progress(progress)

        info = ydl.extract_info(url, False)
        filename: str = ydl.prepare_filename(
            info, outtmpl=os.path.join(temp_path, output_format)
        )
        basename: str = get_basename(filename)

        msg.filename = basename
        msg.video_id = safe_get(info, "id")
        msg.uploader = safe_get(info, "uploader", "")
        msg.title = safe_get(info, "title")
        msg.width = safe_get(info, "width", 0)
        msg.height = safe_get(info, "height", 0)
        msg.duration = safe_get(info, "duration", 0)
        msg.size = safe_get(info, "filesize_approx", 0)
        msg.description = safe_get(info, "description")
        if msg.description == "":
            msg.description = "N/A"
        tags = safe_get(info, "tags", ["N/A"])
        msg.tags = " ".join(tags)

        final_filename: str = ydl.prepare_filename(
            info, outtmpl=os.path.join(download_path, output_format)
        )
        folder = get_root(filename)
        if not os.path.exists(folder):
            os.makedirs(folder)

        if not os.path.exists(final_filename):
            log_verbose(f"downloading {msg.video_id}", verbose)
            ydl.download([url])

            log_verbose(f"finished downloading {msg.video_id}", verbose)

            folder = get_root(final_filename)
            if not os.path.exists(folder):
                os.makedirs(folder)

            shutil.copyfile(filename, final_filename)
            os.remove(filename)

        st = os.stat(final_filename)
        msg.size = st.st_size
        msg.download_date = datetime.datetime.now()

        if not verbose:
            del global_progress[msg.url]

    except Exception as ex:
        err_msg = str(ex)
        if "unavailable pending review" in err_msg:
            print(ex)
            can_recover = False
        elif "This video has been disabled" in err_msg:
            print(ex)
            can_recover = False
        elif "urlopen error" in err_msg:
            print(ex)
            can_recover = False
        else:
            print(ex)
            can_recover = True

        print(f"Error: {str(ex)}")

        if can_recover:
            msg.retries += 1
            if msg.errors is None:
                msg.errors = ""
            msg.errors = msg.errors + f"\nERROR:{str(ex)[:100]}"
            if msg.retries > 5:
                msg.complete = 1
            else:
                msg.complete = 0
        else:
            msg.retries = 6
            msg.complete = 1

    return msg
