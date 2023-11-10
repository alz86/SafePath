using System.Collections.Generic;

namespace SafePath.Entities
{
    public class FeatureCollection
    {
        public List<Feature> Features { get; set; }
        public string Type { get; set; }
    }

    public class Feature
    {
        public Geometry Geometry { get; set; }
        public Properties? Properties { get; set; }
        public string Type { get; set; }
    }

    public class Geometry
    {
        public List<double> Coordinates { get; set; }
        public string Type { get; set; }
    }

    public class Properties
    {
        public ulong OSMNodeId { get; set; }
        public string Type { get; set; }
    }
}