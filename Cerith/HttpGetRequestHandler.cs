using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cerith
{
    public class HttpGetRequestHandler : IHttpRequestHandler
    {
        private readonly CerithConfiguration _cerithConfiguration;
        private readonly MongoClient _client;
        public HttpGetRequestHandler(IOptions<CerithConfiguration> options,
            MongoClient mongoClient)
        {
            _cerithConfiguration = options.Value;
            _client = mongoClient;
        }

        public async Task<bool> HandleRequest(HttpContext context)
        {
            var operation = context.Request.Path.Value;
            var cerithRoute = _cerithConfiguration.Collections.Select(x => RouteInfo.Create(x, operation, "GET"))
                .OrderByDescending(x => x.Probability)
                .FirstOrDefault();

            if (cerithRoute == null || Math.Abs(cerithRoute.NrOfEqualSegments - 1) <= 0) 
                return false;

            string filter;
            if (cerithRoute.IsById)
            {
                filter = $"{{'{cerithRoute.IdentifierKeyValue.Key}':'{cerithRoute.IdentifierKeyValue.Value}'}}";
            }
            else if (context.Request.Query.ContainsKey("filter"))
            {
                filter = context.Request.Query["filter"].ToString();
            }
            else
            {
                var args = context.Request.Query.Select(kv => $"'{kv.Key}':'{kv.Value}'").ToArray();
                filter = "{" + string.Join(",", args) + " }";
            }

            var db = _client.GetDatabase(cerithRoute.Collection.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(cerithRoute.Collection.Name);

            var req = collection.FindAsync(filter);
            string result;
            if (cerithRoute.IsById)
            {
                var doc = await req.Result.FirstAsync();
                if (doc == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return true;
                }
                result = doc.ToJson();
            }
            else
            {
                var docs = new List<string>();
                await collection.FindSync(filter).ForEachAsync(document => docs.Add(document.ToJson()));
                result = $"[{string.Join(",", docs)}]";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(result);
            return true;

        }
    }
}