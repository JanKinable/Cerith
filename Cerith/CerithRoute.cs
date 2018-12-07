using System;
using Microsoft.AspNetCore.Http;

namespace Cerith
{
    public class CerithRoute
    {
        public static CerithRoute Parse(string value, string idName = "_id", string idType = "guid")
        {
            var route = new CerithRoute();
            var path = value;
            var idxStart = value.IndexOf('{');
            if (idxStart == -1)
            {
                route.IdName = idName;
                route.IdType = idType;
            }
            else
            {
                var idxEnd = value.IndexOf('}');
                var id = value.Substring(idxStart + 1, idxEnd - 1 - idxStart);
                var idSplit = id.Split(':');
                route.IdName = idSplit[0];
                route.IdType = idSplit.Length > 1 ? idSplit[1] : idType;
                path = value.Substring(0, idxStart - 2); //2= /{
            }

            var pathParts = path.ToLower().Split('/', StringSplitOptions.RemoveEmptyEntries);
            route.Path = $"/{string.Join("/", pathParts)}";
            return route;
        }

        public PathString Path{ get; set; }
        public string IdName { get; set; }
        public string IdType { get; set; }
    }
}