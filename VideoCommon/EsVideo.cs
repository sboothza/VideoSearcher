using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using VideoSearcher;
using ZstdSharp.Unsafe;

namespace VideoCommon
{
    public interface IdObject
    {
        long id { get; set; }
    }

    public class EsVideo : IdObject
    {
        public long id { get; set; }
        public string url { get; set; }
        public string video_id { get; set; }
        public string title { get; set; }
        public string uploader { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int duration { get; set; }
        public string filename { get; set; }
        public long size { get; set; }
        public DateTime insertdate { get; set; }
        public DateTime downloaddate { get; set; }
        public string description { get; set; }
        public string tags { get; set; }
    }

    public class EsVideoRepository
    {
        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public EsVideoRepository(string host, string username, string password, string index)
        {
            var esSettings = new ElasticsearchClientSettings(new Uri(host))
                .ServerCertificateValidationCallback((o, certificate, arg3, arg4) => true)
                .Authentication(new BasicAuthentication(username, password));

            _client = new ElasticsearchClient(esSettings);
            _indexName = index;
        }

        public List<string> GetFields()
        {
            return new List<string>()
            {
                "id",
                "url",
                "video_id",
                "title",
                "uploader",
                "width",
                "height",
                "duration",
                "filename",
                "size",
                "insertdate",
                "downloaddate",
                "description",
                "tags"
            };
        }

        public Video Get(long id)
        {
            var response = _client.Get<EsVideo>(id, idx => idx.Index(_indexName));
            if (response.Found)
            {
                var result = response.Source;
                result.id = id;
                return new Video(result);
            }

            return null;
        }

        public List<Video> Search(string query, int size, string orderBy, bool asc)
        {
            // var resp = _client.Search<EsVideo>(s => s
            //     .Index(_indexName)
            //     .From(0)
            //     .Size(size)
            //     .Query(q => q
            //         .MultiMatch(m => m
            //             .Query(query)
            //         ))
            //     .Sort(s=>s.Field(f => f.size))
            // );
            var ascDesc = asc ? "asc" : "desc";
            var queryString =
                "{ \"query\": { \"multi_match\": { \"query\": \"braces facial\", \"fields\": [ \"tags\", \"uploader\", \"description\" ], \"operator\": \"and\" } }, \"size\": 20, \"from\": 0, \"sort\": [ { \"duration\": { \"order\": \"desc\" } } ]}";

            var resp = _client.Search<EsVideo>(s => s
                .Index((_indexName))
                .Query(q => q.RawJson(queryString))
            );
            var result = new List<Video>();

            if (resp.IsSuccess())
            {
                var docs = resp.Documents.ToList();
                var ids = resp.Hits.Select(d => long.Parse(d.Id)).ToList();
                for (var i = 0; i < resp.Hits.Count; i++)
                {
                    var v = new Video(docs[i])
                    {
                        Id = ids[i]
                    };
                    result.Add(v);
                }
            }

            return result;
        }
    }
}