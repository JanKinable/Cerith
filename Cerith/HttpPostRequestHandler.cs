﻿using System;
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
        public HttpPostRequestHandler(IOptionsMonitor<CerithConfiguration> options,
            MongoClient mongoClient)
        {
            _cerithConfiguration = options.CurrentValue;
            _client = mongoClient;
        }

        public async Task<bool> HandleRequest(HttpContext context)
        {
            var operation = context.Request.Path.Value;
            if (!operation.EndsWith('/')) operation += "/";

            var routes = _cerithConfiguration.Collections
                .Select(x => RouteComparer.Equals(x, operation, "POST"))
                .Where(x => x.Result).ToArray();
            if (routes.Length != 1) return false;
            var route = routes[0];

            var json = "";
            using (var reader = new StreamReader(context.Request.Body))
            {
                json = reader.ReadToEnd();
            }

            var db = _client.GetDatabase(route.Collection.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(route.Collection.Name);

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