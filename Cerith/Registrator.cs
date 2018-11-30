using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Cerith
{
    public static class Registrator
    {
        public static void AddCerith(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CerithConfiguration>(configuration);

            var config = configuration.Get<CerithConfiguration>();
           
            if (config.MongoConnectionString != null)
            {
                //for now only use a connectionstring
                if (string.IsNullOrEmpty(config.MongoConnectionString))
                {
                    services.AddSingleton(provider => new MongoClient(new MongoClientSettings
                    {
                        Server = new MongoServerAddress("localhost", 27017),
                        ServerSelectionTimeout = TimeSpan.FromSeconds(5)
                    }));
                }
                else
                {
                    services.AddSingleton(provider =>
                        new MongoClient(config.MongoConnectionString));
                }
            }

            //register the other services (/handlers)
            services.AddTransient<HttpGetRequestHandler>();
            services.AddTransient<HttpPutRequestHandler>();
            services.AddTransient<HttpPostRequestHandler>();
        }

        public static IApplicationBuilder UseCerith(this IApplicationBuilder builder)
        {
            //check the config
            var config = builder.ApplicationServices.GetService<IOptions<CerithConfiguration>>();
            if (config?.Value == null)
            {
                throw new ApplicationException("Missing CerithConfiguration or AddCerith not being called.");
            }
           

            return builder.UseMiddleware<CerithMiddleware>(builder.ApplicationServices);
        }
    }
}
