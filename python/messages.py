import datetime
import uuid

from sb_pubsub_kafka import EnvelopeMapper, Envelope


class QueueVideo(Envelope):
    Identifier = "QueueVideo"
    url: str

    def __init__(self, url: str = "", correlation_id: uuid.UUID = uuid.uuid4()):
        super().__init__(correlation_id)
        self.url = url

    def __str__(self):
        return f"Identifier:{self.Identifier} url:{self.url} corid:{self.CorrelationId}"


EnvelopeMapper.register(QueueVideo.Identifier, QueueVideo)


class VideoDownloaded(Envelope):
    Identifier = "VideoDownloaded"
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

    def __init__(
        self,
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
        correlation_id: uuid.UUID = uuid.uuid4(),
    ):
        super().__init__(correlation_id)
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

    def __str__(self):
        return f"Identifier:{self.Identifier} url:{self.url} corid:{self.CorrelationId}"


EnvelopeMapper.register(VideoDownloaded.Identifier, VideoDownloaded)


class VideoManualAdd(Envelope):
    Identifier = "VideoManualAdd"
    filename: str
    uploader: str
    width: int
    height: int
    duration: int
    size: int

    def __init__(
        self,
        filename: str = "",
        uploader: str = "",
        width: int = 0,
        height: int = 0,
        duration: int = 0,
        size: int = 0,
        correlation_id: uuid.UUID = uuid.uuid4(),
    ):
        super().__init__(correlation_id)
        self.filename = filename
        self.uploader = uploader
        self.width = width
        self.height = height
        self.duration = duration
        self.size = size

    def __str__(self):
        return f"Identifier:{self.Identifier} filename:{self.filename} corid:{self.CorrelationId}"


EnvelopeMapper.register(VideoManualAdd.Identifier, VideoManualAdd)


class VideoError(Envelope):
    Identifier = "VideoError"
    url: str
    video_id: str
    uploader: str
    title: str
    width: int
    height: int
    duration: int
    size: int
    description: str
    tags: str
    errors: str
    retries: int

    def __init__(
        self,
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
        errors: str = "",
        retries: int = 0,
        correlation_id: uuid.UUID = uuid.uuid4(),
    ):
        super().__init__(correlation_id)
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
        self.errors = errors
        self.retries = retries

    def __str__(self):
        return f"Identifier:{self.Identifier} url:{self.url} corid:{self.CorrelationId}"


EnvelopeMapper.register(VideoError.Identifier, VideoError)


class VideoDeleted(Envelope):
    Identifier = "VideoDeleted"
    url: str

    def __init__(
        self,
        url: str = "",
        correlation_id: uuid.UUID = uuid.uuid4(),
    ):
        super().__init__(correlation_id)
        self.url = url

    def __str__(self):
        return f"Identifier:{self.Identifier} url:{self.url} corid:{self.CorrelationId}"


EnvelopeMapper.register(VideoDeleted.Identifier, VideoDeleted)


class GeneralRestMessage:
    success: bool
    code: int
    message: str

    def __init__(self, success: bool, code: int, message: str):
        self.success = success
        self.code = code
        self.message = message
