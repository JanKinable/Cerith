using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cerith
{
    public static class CerithError
    {
        private class ErrorMessage
        {
            public string Message { get; set; }
        }

        public static async Task Error(this HttpResponse response, HttpStatusCode status, string message)
        {
            response.ContentType = "application/json";
            response.StatusCode = (int)status;
            var msg = new ErrorMessage {Message = message};
            var json = JsonConvert.SerializeObject(msg);
            await response.WriteAsync(json);
        }
    }
}