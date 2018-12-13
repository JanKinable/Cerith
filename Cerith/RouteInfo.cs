using System;
using System.Linq;

namespace Cerith
{
    public static class RouteComparer
    {
        public static RouteCompareResult Equals(Collection candidate, string operation, string methode)
        {
            if (!operation.EndsWith("/")) operation += "/";

            switch (methode)
            {
                case "GET" when candidate.Routes.GetList == operation:
                    return new RouteCompareResult
                    {
                        Result = true,
                        Route = candidate.Routes.GetList,
                        Collection = candidate
                    };
                case "GET":
                    return new RouteCompareResult
                    {
                        Result = CheckById(candidate.Routes.GetById, operation),
                        Route = candidate.Routes.GetById,
                        Collection = candidate
                    };
                case "PUT":
                    return new RouteCompareResult
                    {
                        Result = CheckById(candidate.Routes.Update, operation),
                        Route = candidate.Routes.Update,
                        Collection = candidate
                    };
                case "POST" when candidate.Routes.Create == operation:
                    return new RouteCompareResult
                    {
                        Result = true,
                        Route = candidate.Routes.Create,
                        Collection = candidate
                    };
                default:
                    return new RouteCompareResult { Result = false };
            }
        }

        private static bool CheckById(string route, string operation)
        {
            var candidateParts = route.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
            var operationParts = operation.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (candidateParts.Count != operationParts.Length) return false;

            for (var i = 0; i < operationParts.Length; i++)
            {
                if (candidateParts[i].StartsWith("{"))
                {
                    var idParts = candidateParts[i].Split(":");
                    //check on type only
                    var typePart = idParts[1].Substring(0, idParts[1].Length - 1);
                    if (typePart.Equals("guid", StringComparison.Ordinal))
                    {
                        if (Guid.TryParse(operationParts[i], out var resGuid))
                        {
                            continue;
                        }

                        return false;
                    }

                    if (typePart.Equals("int", StringComparison.Ordinal))
                    {
                        if (int.TryParse(operationParts[i], out var resInt))
                        {
                            continue;
                        }

                        return false;
                    }
                    
                }
                else if (!candidateParts[i].Equals(operationParts[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryGetId(string route, string operation, out string idName, out string idValue)
        {
            idName = string.Empty;
            idValue = string.Empty;
            var splittedRoute = route.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();
            var idx = splittedRoute
                .Select((s, i) => new { Index = i, Value= s })
                .Where(t => t.Value.StartsWith("{"))
                .Select(t => t.Index)
                .FirstOrDefault();
            if (idx == 0)
            {
                return false;
            }

            var operationParts = operation.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (operationParts.Length - 1 < idx)
            {
                return false;
            }

            idName = splittedRoute[idx].Split(":")[0].Substring(1);
            idValue = operationParts[idx];
            return true;
        }

        public class RouteCompareResult
        {
            public string Route { get; set; }
            public bool Result { get; set; }
            public Collection Collection { get; set; }
        }
    }
}