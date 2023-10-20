using System.Data.Common;
using DatabaseProcessor;
using VideoCommon;

namespace VideoSearcher
{
	public class Video
	{
		public long Id { get; set; }
		public string Url { get; set; }
		public string VideoId { get; set; }
		public string Title { get; set; }
		public string Uploader { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Duration { get; set; }
		public string Filename { get; set; }
		public long Size { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime DownloadDate { get; set; }
		public string Description { get; set; }
		public string Tags { get; set; }

		public string NormalizedUploader => $"{(Uploader == "" ? "NA" : Uploader).Replace(" ", "_")}";

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
			Filename = reader.GetStr("filename");
			Size = reader.GetLong("size");
			InsertDate = reader.GetDT("insertdate");
			DownloadDate = reader.GetDT("downloaddate");
			Description = reader.GetStr("description");
			Tags = reader.GetStr("tags");
		}

        public Video(EsVideo video)
        {
            Id = video.id;
            Url = video.url;
            VideoId = video.video_id;
            Title = video.title;
            Uploader = video.uploader;
            Width = video.width;
            Height = video.height;
            Duration = video.duration;
            Filename = video.filename;
            Size = video.size;
            InsertDate = video.insertdate;
            DownloadDate = video.downloaddate;
            Description = video.description;
            Tags = video.tags;
        }
	}
}