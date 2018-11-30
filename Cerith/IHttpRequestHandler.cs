using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Cerith
{
    public interface IHttpRequestHandler
    {
        Task<bool> HandleRequest(HttpContext context);
    }
}