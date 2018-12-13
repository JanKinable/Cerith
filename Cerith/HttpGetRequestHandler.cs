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
            if (!operation.EndsWith('/')) operation += "/";

            var routes = _cerithConfiguration.Collections
                .Select(x => RouteComparer.Equals(x, operation, "GET"))
                .Where(x => x.Result).ToArray();
            if (routes.Length  != 1) return false;

            var route = routes[0];

            string filter;
            var isById = false;
            if (RouteComparer.TryGetId(route.Route, operation, out string idName, out string idValue))
            {
                filter = $"{{'{idName}':'{idValue}'}}";
                isById = true;
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

            var db = _client.GetDatabase(route.Collection.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(route.Collection.Name);

            var req = collection.FindAsync(filter);
            string result;
            if (isById)
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