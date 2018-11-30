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
        public string Name { get; set; }
        public string DatabaseName { get; set; }

        public string IdName { get; set; } = "_id";

        public string Route { get; set; }

        public CerithRoute GetCerithRoute()
        {
            if (_route == null)
            {
                if (string.IsNullOrEmpty(Route))
                {
                    Route = $"/api/{Name}/";
                }
                _route = CerithRoute.Parse(Route, IdName);
            }
            return _route;
        }
       
        public CollectionAccessType AccessType { get; set; }
    }
}