using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cerith
{
    public class HttpPostRequestHandler : IHttpRequestHandler
    {
        private readonly CerithConfiguration _cerithConfiguration;
        private readonly MongoClient _client;
        public HttpPostRequestHandler(IOptions<CerithConfiguration> options,
            MongoClient mongoClient)
        {
            _cerithConfiguration = options.Value;
            _client = mongoClient;
        }

        public async Task<bool> HandleRequest(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path.EndsWith('/')) path = path.Substring(0, path.Length - 2);

            var cerithMaps = _cerithConfiguration.Collections.Where(x =>
            {
                var route = x.GetCerithRoute();
                return path.Equals(route.Path, StringComparison.OrdinalIgnoreCase);
            }).ToArray();

            if (!cerithMaps.Any())
                return false;

            if (cerithMaps.Length > 1)
            {
                await context.Response.Error(HttpStatusCode.BadRequest, $"The url does not point to a unique handler. Found candidates: {string.Join(",", cerithMaps.Select(x => x.Route))}");
                return true;
            }

            var cerithMap = cerithMaps.First();

            string json = "";
            using (var reader = new StreamReader(context.Request.Body))
            {
                json = reader.ReadToEnd();
            }

            if (cerithMap.AccessType != CollectionAccessType.Admin)
            {
                await context.Response.Error(HttpStatusCode.Unauthorized, $"You need admin permissions to add a document to the {cerithMap.Name} collection.");
                return true;
            }

            var db = _client.GetDatabase(cerithMap.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(cerithMap.Name);

            var doc = BsonDocument.Parse(json);

            context.Response.ContentType = "application/json";
            try
            {
                await collection.InsertOneAsync(doc);
            }
            catch (MongoWriteException mwx)
            {
                await context.Response.Error(HttpStatusCode.Conflict, mwx.WriteError.Message);
                return true;
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