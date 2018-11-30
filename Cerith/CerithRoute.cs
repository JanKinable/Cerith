using System;
using Microsoft.AspNetCore.Http;

namespace Cerith
{
    public class CerithRoute
    {
        public static CerithRoute Parse(string value, string idName = "_id")
        {
            var route = new CerithRoute();
            var path = value;
            var idxStart = value.IndexOf('{');
            if (idxStart == -1)
            {
                route.IdName = idName;
            }
            else
            {
                var idxEnd = value.IndexOf('}');
                route.IdName = value.Substring(idxStart + 1, idxEnd - 1 - idxStart);
                path = value.Substring(0, idxStart - 2); //2= /{
            }

            var pathParts = path.ToLower().Split('/', StringSplitOptions.RemoveEmptyEntries);
            route.Path = $"/{string.Join("/", pathParts)}";
            return route;
        }

        public PathString Path{ get; set; }
        public string IdName { get; set; }

    }
}