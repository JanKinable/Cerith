using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Cerith
{
    public class CerithMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _services;

        public CerithMiddleware(RequestDelegate next, IServiceProvider services)
        {
            _next = next;
            _services = services;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            IHttpRequestHandler handler;
            switch (httpContext.Request.Method)
            {
                case "GET":
                    handler = _services.GetService<HttpGetRequestHandler>();
                    break;
                case "PUT":
                    handler = _services.GetService<HttpPutRequestHandler>();
                    break;
                case "POST":
                    handler = _services.GetService<HttpPostRequestHandler>();
                    break;
                default:
                    handler = null;
                    break;
            }

            if (handler == null || ! await handler.HandleRequest(httpContext))
            {
                await _next.Invoke(httpContext);
            }
        }
    }
}
