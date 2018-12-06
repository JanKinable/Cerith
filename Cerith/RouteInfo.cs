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
                sourceRoute += $"{{{source.IdName}}}";
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
                if (srcItem != trgItem && srcItem.StartsWith("{"))
                {
                    HasIdentifier = true;
                    IdentifierKeyValue = new KeyValuePair<string, string>(srcItem.Substring(1, srcItem.Length -2), trgItem);
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
                if (HasIdentifier)
                {
                    NrOfEqualSegments += 1;
                }

                return NrOfEqualSegments / _maxIdx;
            }
        }

        public bool IsById => HasIdentifier && !string.IsNullOrEmpty(IdentifierKeyValue.Value);
    }
}