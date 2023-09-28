using System.Data.Common;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace VideoSearcher
{
    public class VideoRepository
    {
        private readonly string _connectionString;
        public VideoRepository(IConfiguration config)
        {
            _connectionString = config.GetSection("Settings").GetSection("ConnectionString").Value;
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

        public List<Video> GetSearch(string criteria1, string criteria2, string criteria3, string orderBy, bool ascending, int limit, int minimumSize)
        {
            var asc = ascending ? "" : "DESC";
            var query = "select *, crit1+crit2+crit3 as match_count from " +
            "(select *, " +
            $"case when tags like '%{criteria1}%' then 1 else 0 end as crit1, " +
            $"case when tags like '%{criteria2}%' then 1 else 0 end as crit2, " +
            $"case when tags like '%{criteria3}%' then 1 else 0 end as crit3 " +
            "from video_catalog " +
            $"where (tags like '%{criteria1}%' or tags like '%{criteria2}%' or tags like '%{criteria3}%') " +
            $"and height >= {minimumSize} and deleted = 0 and complete = 1 and filename <> '' " +
            ") as f " +
            $"order by match_count desc, {orderBy} {asc} " +
            $"limit {limit};";
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
    }
}
