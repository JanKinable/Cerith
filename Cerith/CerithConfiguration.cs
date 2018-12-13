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
        public string Name { get; set; }
        public string DatabaseName { get; set; }
        public CerithRoutes Routes { get; set; }
    }

    public class CerithRoutes
    {
        public string GetList { get; set; }
        public string GetById { get; set; }
        public string Create { get; set; }
        public string Update { get; set; }
    }
}