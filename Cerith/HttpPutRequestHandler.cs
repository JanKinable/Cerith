using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Cerith
{
    public class HttpPutRequestHandler : IHttpRequestHandler
    {
        private readonly CerithConfiguration _cerithConfiguration;
        private readonly MongoClient _client;
        public HttpPutRequestHandler(IOptions<CerithConfiguration> options,
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
                .Select(x => RouteComparer.Equals(x, operation, "PUT"))
                .Where(x => x.Result).ToArray();
            if (routes.Length != 1) return false;
            var route = routes[0];

            string json = "";
            using (var reader = new StreamReader(context.Request.Body))
            {
                json = reader.ReadToEnd();
            }

            var db = _client.GetDatabase(route.Collection.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(route.Collection.Name);

            if (!RouteComparer.TryGetId(route.Route, operation, out string idName, out string idValue))
            {
                return false;
            }

            var filter = Builders<BsonDocument>.Filter.Eq(s => s[idName], idValue);
            var doc = BsonDocument.Parse(json);

            context.Response.ContentType = "application/json";
            try
            {
                var res = await collection.ReplaceOneAsync(
                    filter: filter,
                    replacement: doc);

                if (!res.IsAcknowledged)
                {
                    await context.Response.Error(HttpStatusCode.Conflict, "Document not found");
                    return true;
                }
            }
            catch (Exception e)
            {
                await context.Response.Error(HttpStatusCode.InternalServerError, e.ToString());
                return true;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Accepted;
            await context.Response.WriteAsync(json);
            return true;
        }
    }
}