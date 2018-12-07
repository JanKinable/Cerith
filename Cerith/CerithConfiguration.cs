using System.Collections.Generic;

namespace Cerith
{
    public class CerithConfiguration
    {
        public string MongoConnectionString { get; set; }
        public List<Collection> Collections { get; set; }
    }

    public class Collection
    {
        private CerithRoute _route;
        private string _routeVal;
        public string Name { get; set; }
        public string DatabaseName { get; set; }

        public string IdName { get; set; } = "_id";
        public string IdType { get; set; } = "Guid";

        public string Route
        {
            get
            {
                if (string.IsNullOrEmpty(_routeVal)) GetCerithRoute();
                return _routeVal;
            }
            set => _routeVal = value;
        }

        public CerithRoute GetCerithRoute()
        {
            if (_route == null)
            {
                if (string.IsNullOrEmpty(_routeVal))
                {
                    _routeVal = $"/api/{Name}/";
                }
                _route = CerithRoute.Parse(_routeVal, IdName, IdType);
                IdType = _route.IdType;
            }
            return _route;
        }
       
        public CollectionAccessType AccessType { get; set; }
    }
}