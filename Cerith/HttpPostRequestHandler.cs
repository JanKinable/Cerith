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

            var cerithRoute = _cerithConfiguration.Collections.Select(x => RouteInfo.Create(x, path, "POST"))
                .OrderByDescending(x => x.Probability)
                .FirstOrDefault();

            if (cerithRoute == null)
                return false;

            var json = "";
            using (var reader = new StreamReader(context.Request.Body))
            {
                json = reader.ReadToEnd();
            }

            if (cerithRoute.Collection.AccessType != CollectionAccessType.Admin)
            {
                await context.Response.Error(HttpStatusCode.Unauthorized, $"You need admin permissions to add a document to the {cerithRoute.Collection.Name} collection.");
                return true;
            }

            var db = _client.GetDatabase(cerithRoute.Collection.DatabaseName);
            var collection = db.GetCollection<BsonDocument>(cerithRoute.Collection.Name);

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