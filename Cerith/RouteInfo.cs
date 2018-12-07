using System;
using System.Collections.Generic;

namespace Cerith
{
    public class RouteInfo
    {
        private readonly int _maxIdx;

        public static RouteInfo Create(Collection source, string target, string method)
        {
            return new RouteInfo(source, target, method);
        }

        private RouteInfo(Collection source, string target, string method)
        {
            Collection = source;
            var sourceRoute = source.Route;

            //src should always contain an id
            if (sourceRoute.IndexOf('{') == -1 && method != "POST")
            {
                //extend with id
                sourceRoute += $"{{{source.IdName}:{source.IdType}}}";
            }

            var src = sourceRoute.Split("/", StringSplitOptions.RemoveEmptyEntries);
            var trg = target.Split("/", StringSplitOptions.RemoveEmptyEntries);
            _maxIdx = Math.Max(src.Length, trg.Length);
            SourceSegmentCount = src.Length;
            TargetSegmentCount = trg.Length;

            for (var i = 0; i < _maxIdx; i++)
            {
                var srcItem = src.Length > i ? src[i] : string.Empty;
                var trgItem = trg.Length > i ? trg[i] : string.Empty;
                if (srcItem != trgItem && srcItem.StartsWith("{") && !string.IsNullOrEmpty(trgItem))
                {
                    if (!(source.IdType.Equals("guid",StringComparison.OrdinalIgnoreCase) && Guid.TryParse(trgItem, out Guid resGuid)))
                    {
                        continue;
                    }

                    if (!(source.IdType.Equals("int", StringComparison.OrdinalIgnoreCase) && int.TryParse(trgItem, out int resId)))
                    {
                        continue;
                    }

                    HasIdentifier = true;
                    IdentifierKeyValue = new KeyValuePair<string, string>(srcItem.Substring(1, srcItem.Length -2).Split(':')[0], trgItem);
                    NrOfEqualSegments++;
                }

                if (srcItem == trgItem)
                {
                    NrOfEqualSegments++;
                }
            }
        }

        public Collection Collection { get; }
        public int SourceSegmentCount { get; set; }
        public int TargetSegmentCount { get; set; }
        public double NrOfEqualSegments { get; set; }

        public bool HasIdentifier { get; set; }
        public KeyValuePair<string, string> IdentifierKeyValue { get; set; }
        public double Probability
        {
            get
            {
                if (IsById)
                {
                    NrOfEqualSegments += 1;
                }

                return NrOfEqualSegments / _maxIdx;
            }
        }

        public bool IsById => HasIdentifier && !string.IsNullOrEmpty(IdentifierKeyValue.Value);
    }
}