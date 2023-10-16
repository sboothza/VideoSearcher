from threading import Lock

from sb_db_common import Session
from sb_pubsub_kafka import PubSub
from sb_threadpool import JobQueue, Job, JobCompletionStatus
from yt_dlp import YoutubeDL

from utils import task_download, Video, log_verbose


class UrlConfig:
    def __init__(
        self,
        connection_string: str = "",
        yt_dlp: YoutubeDL = None,
        temp_path: str = "",
        output_format: str = "",
        download_path: str = "",
        translate: bool = False,
        pubsub: PubSub = None,
        tools_path: str = "",
        session: Session = None,
        verbose: bool = False,
    ):
        self.connection_string = connection_string
        self.yt_dlp = yt_dlp
        self.temp_path = temp_path
        self.output_format = output_format
        self.download_path = download_path
        self.translate = translate
        self.pubsub = pubsub
        self.tools_path = tools_path
        self.session = (session,)
        self.verbose = verbose


class UrlJob(Job):
    def __init__(self, id: int = 0, url: str = "", config: UrlConfig = None):
        super().__init__(url, None)
        self.id = id
        self.url = url
        self.config = config

    def execute(self):
        try:
            item: Video = task_download(
                self.config.yt_dlp,
                self.url,
                self.config.temp_path,
                self.config.output_format,
                self.config.download_path,
                self.config.verbose,
            )
            item.id = self.id
            if self.config.translate:
                item.translate()

            msg = item.get_message()
            log_verbose(
                f"publishing download completed message for [{msg.url}]",
                self.config.verbose,
            )
            self.config.pubsub.publish(msg)

            self.result = JobCompletionStatus.SUCCESS
        except Exception as ex:
            # print(ex)
            self.result = JobCompletionStatus.FAIL_RETRY
            raise


class UrlJobQueue(JobQueue):
    session: Session

    exists_query = (
        "SELECT count(*) FROM sqlite_schema WHERE type='table' and name = 'url_queue';"
    )

    create_query = (
        "create table url_queue(id INTEGER PRIMARY KEY AUTOINCREMENT, url text, in_progress integer, retries "
        "integer, errors text);"
    )

    reset_query = "update url_queue set in_progress = 0;"
    queue_insert_query = "insert into url_queue(url, in_progress, retries, errors) values (:url, 0, 0, '');"
    queue_delete_query = "delete from url_queue where id = :id"
    queue_set_in_progress_query = (
        "update url_queue set in_progress = :in_progress where id = :id"
    )

    queue_get_next_item_query = "select id, url from url_queue where in_progress = 0 and retries < 6 order by id limit 1;"

    queue_count_query = (
        "select count(*) from url_queue where in_progress = 0 and retries < 6;"
    )

    already_in_queue_query = "select count(*) from url_queue where url = :url;"

    def __init__(self, config: UrlConfig, session: Session):
        super().__init__()
        self.config = config
        self.lock = Lock()
        self.session = session

        sql = UrlJobQueue.exists_query
        result = session.fetch_scalar(sql)
        if result == 0:
            sql = UrlJobQueue.create_query
            session.execute(sql)
            session.commit()

        sql = UrlJobQueue.reset_query
        session.execute(sql)
        session.commit()

    def get_job(self) -> Job | None:
        try:
            self.lock.acquire()
            sql = UrlJobQueue.queue_get_next_item_query
            row = self.session.fetch_one(sql)
            if row is not None:
                id = row[0]
                url = row[1]
                sql = UrlJobQueue.queue_set_in_progress_query
                params = {"id": id, "in_progress": 1}
                self.session.execute(sql, params)
                self.session.commit()
                return UrlJob(id, url, self.config)
            else:
                return None

        except Exception as ex:
            print(ex)
            return None
        finally:
            self.lock.release()

    def queue_job(self, obj: Job):
        job: UrlJob = obj
        sql = UrlJobQueue.already_in_queue_query
        params = {"url": job.url}
        result = self.session.fetch_scalar(sql, params)
        if result == 0:
            sql = UrlJobQueue.queue_insert_query
            self.session.execute(sql, params)
            self.session.commit()

    def commit_job(self, obj: Job):
        job: UrlJob = obj
        sql = UrlJobQueue.queue_delete_query
        params = {"id": job.id}
        self.session.execute(sql, params)
        self.session.commit()

    def rollback_job(self, obj: Job):
        job: UrlJob = obj
        sql = UrlJobQueue.queue_set_in_progress_query
        params = {"id": job.id, "in_progress": 0}
        self.session.execute(sql, params)
        self.session.commit()

    def count(self) -> int:
        try:
            self.lock.acquire()
            sql = UrlJobQueue.queue_count_query
            count = self.session.fetch_scalar(sql)
            return count
        finally:
            self.lock.release()

    def clear(self):
        pass

    def close(self):
        try:
            self.lock.acquire()
            self.session.close()
        finally:
            self.lock.release()
