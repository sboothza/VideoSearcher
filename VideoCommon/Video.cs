using System.Data;
using System.Data.Common;

using DatabaseProcessor;

namespace VideoSearcher
{
    public class Video
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Uploader { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Duration { get; set; }
        public int Complete { get; set; }
        public string Filename { get; set; }
        public int Retries { get; set; }
        public string Errors { get; set; }
        public int InProgress { get; set; }
        public int Translated { get; set; }
        public int Deleted { get; set; }
        public long Size { get; set; }
        public DateTime InsertDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }

        public Video()
        {

        }

        public void Populate(DbDataReader reader)
        {
            Id = reader.GetInt("id");
            Url = reader.GetStr("url");
            VideoId = reader.GetStr("video_id");
            Title = reader.GetStr("title");
            Uploader = reader.GetStr("uploader");
            Width = reader.GetInt("width");
            Height = reader.GetInt("height");
            Duration = reader.GetInt("duration");
            Complete = reader.GetInt("complete");
            Filename = reader.GetStr("filename");
            Retries = reader.GetInt("retries");
            Errors = reader.GetStr("errors");
            InProgress = reader.GetInt("in_progress");
            Translated = reader.GetInt("translated");
            Deleted = reader.GetInt("deleted");
            Size = reader.GetLong("size");
            InsertDate = reader.GetDT("insertdate");
            DownloadDate = reader.GetDT("downloaddate");
            Description = reader.GetStr("description");
            Tags = reader.GetStr("tags");
        }
    }

    public class VideoList : List<Video>
    {
        public VideoList() { }
    }
}
