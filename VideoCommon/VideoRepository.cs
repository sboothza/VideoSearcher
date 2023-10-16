using System.Data.Common;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace VideoSearcher
{
    public class VideoRepository
    {
        private readonly string _connectionString;

        public VideoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<string> GetFields()
        {
            try
            {
                var conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                List<string> data = new();
                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = "select * from video_catalog limit 1;";
                    command.CommandType = CommandType.Text;
                    command.Prepare();
                    using (var reader = command.ExecuteReader())
                    {
                        var columns = reader.GetColumnSchema();
                        foreach (var column in columns)
                        {
                            data.Add(column.ColumnName);
                        }
                    }
                }

                return data;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public List<Video> GetSearch(string criteria1, string criteria2, string criteria3, string uploader,
            string orderBy, bool ascending, int limit, int minimumSize, string durationDir, string duration)
        {
            var asc = ascending ? "" : "DESC";
            var query = $"select *, crit1+crit2+crit3 as match_count from (select *, ";

            var crit1Str = string.IsNullOrEmpty(criteria1)
                ? "0"
                : $"case when tags like '%{criteria1}%' or description like '%{criteria1}%' or title like '%{criteria1}%' then 1 else 0 end";

            var crit1Where = string.IsNullOrEmpty(criteria1)
                ? "1 = 0 "
                : $"tags like '%{criteria1}%' or description like '%{criteria1}%' or title like '%{criteria1}%' ";

            var crit2Str = string.IsNullOrEmpty(criteria2)
                ? "0"
                : $"case when tags like '%{criteria2}%' or description like '%{criteria2}%' or title like '%{criteria2}%' then 1 else 0 end";

            var crit2Where = string.IsNullOrEmpty(criteria2)
                ? "1 = 0 "
                : $"tags like '%{criteria2}%' or description like '%{criteria2}%' or title like '%{criteria2}%' ";

            var crit3Str = string.IsNullOrEmpty(criteria3)
                ? "0"
                : $"case when tags like '%{criteria3}%' or description like '%{criteria3}%' or title like '%{criteria3}%' then 1 else 0 end";

            var crit3Where = string.IsNullOrEmpty(criteria3)
                ? "1 = 0 "
                : $"tags like '%{criteria3}%' or description like '%{criteria3}%' or title like '%{criteria3}%' ";

            // special case of no criteria at all - find everything
            if (string.IsNullOrEmpty(criteria1) && string.IsNullOrEmpty(criteria2) && string.IsNullOrEmpty(criteria3))
                crit3Where = "1=1 ";

            var uploaderWhere = string.IsNullOrEmpty(uploader) ? "" : $"and uploader like '%{uploader}%' ";

            var durationLimit = duration switch
            {
                "60 min" => (60 * 60).ToString(),
                "45 min" => (45 * 60).ToString(),
                "30 min" => (30 * 60).ToString(),
                "15 min" => (15 * 60).ToString(),
                "5 min" => (5 * 60).ToString(),
                _ => ""
            };

            var durationWhere = durationDir switch
            {
                "N/A" => "",
                "<=" => $" and (duration <= {durationLimit}) ",
                ">=" => $" and (duration >= {durationLimit}) ",
                _ => ""
            };

            query += $"{crit1Str} as crit1, {crit2Str} as crit2, {crit3Str} as crit3 " +
                     "from video_catalog " +
                     $"where (({crit1Where}) or ({crit2Where}) or ({crit3Where})) " +
                     $"and height >= {minimumSize} and deleted = 0 and complete = 1 and filename <> '' {uploaderWhere} {durationWhere} " +
                     ") as f " +
                     $"order by match_count desc, {orderBy} {asc} " +
                     $"limit {limit};";

            Console.WriteLine(query);

            return Get(query);
        }

        //public List<Video> GetSearchAnd(string criteria1, string criteria2, string criteria3)
        //{
        //    var query = $"select * from video_catalog where (title like '%{criteria}%' or uploader like '%{criteria}%' or description like '%{criteria}%' or tags like '%{criteria}%' limit 10;";
        //    return Get(query);
        //}

        public List<Video> Get(string query)
        {
            try
            {
                var conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                List<Video> data = new List<Video>();
                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.Prepare();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var video = new Video();
                            video.Populate(reader);
                            if (video.Filename is not null && video.Filename != "")
                                data.Add(video);
                        }
                    }
                }

                return data;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        private void Execute(string query)
        {
            try
            {
                var conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();


                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public void Delete(string filename, string rootFolder)
        {
            var videos = Get($"select * from video_catalog where deleted = 0 and filename = '{filename}';");
            foreach (var video in videos)
            {
                var actualFilename = Path.Combine(Path.Combine(rootFolder, $"{video.NormalizedUploader}"),
                    $"{video.Filename}");
                File.Delete(actualFilename);
                Execute($"update video_catalog set deleted = 1 where filename = '{video.Filename}'");
            }
        }
    }
}