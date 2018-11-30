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
            var reqPath = context.Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var path = $"/{string.Join("/", reqPath.SkipLast(1))}";
            var id = reqPath.Last();

            var cerithMap = _cerithConfiguration.Collections.FirstOrDefault(x =>
            {
                var route = x.GetCerithRoute();
                return route.Path == path;
            });

            if (cerithMap == null)
                return false;

            string json = "";
            using (var reader = new StreamReader(context.Request.Body))
            {
                json = reader.ReadToEnd();
            }

            if (cerithMap.AccessType != CollectionAccessType.Admin)
            {
                await context.Response.Error(HttpStatusCode.Unauthorized, $"You need admin permissions to update a document in the {cerithMap.Name} collection.");
                return true;
            }

            var db = _client.GetDatabase(cerithMap.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(cerithMap.Name);
            
            var filter = Builders<BsonDocument>.Filter.Eq(s => s[cerithMap.IdName], id);
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