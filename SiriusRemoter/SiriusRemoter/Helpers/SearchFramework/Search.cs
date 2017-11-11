namespace SiriusRemoter.Helpers.SearchFramework
{
    using Elasticsearch.Net;
    using Nest;
    using Newtonsoft.Json.Linq;
    using SiriusRemoter.Models;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class Search
    {
        const string IndexName = "searcher";
        const string typeName = "text";

        private int count = 0;
        private readonly ElasticClient client;

        public Search(string hostname)
        {
            client = Connect(hostname);
            if (client != null && !client.IndexExists(IndexName).Exists)
            {
                var response = CreateIndex(IndexName);
            }
        }

        public ElasticClient Client => this.client;

        public Task<ElasticsearchResponse<byte[]>> Index(NavigationItem item)
        {
            var jsonObject = new JObject();
            jsonObject.Add("name", item.Name);
            return Index(jsonObject.ToString());
        }

        public async Task<ElasticsearchResponse<byte[]>> Index(string json)
        {
            return await client.LowLevel.IndexAsync<byte[]>(IndexName, typeName, count++.ToString(), json);
        }

        public void Clear()
        {
            var response = DeleteIndex(IndexName);
        }

        private bool IsUrlReachable(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 100;
            request.Method = "HEAD"; // As per Lasse's comment
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        private ElasticClient Connect(string url)
        {
            if (IsUrlReachable(url))
            {
                var settings = new ConnectionSettings(new Uri(url));
                return new ElasticClient(settings);
            }
            return null;
        }

        private ICreateIndexResponse CreateIndex(string indexName)
        {
            return client.CreateIndex(indexName);
        }

        private IDeleteIndexResponse DeleteIndex(string indexName)
        {
            return client.DeleteIndex(indexName);
        }
    }
}
