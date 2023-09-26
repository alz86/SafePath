using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SafePath.DTOs
{
    public class GeoJsonFeatureCollection
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "FeatureCollection";

        [JsonPropertyName("features")]
        public List<GeoJsonFeature> Features { get; set; } = new List<GeoJsonFeature>();
    }

    public class GeoJsonFeature
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "Feature";

        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; }

        [JsonPropertyName("geometry")]
        public GeoJsonPoint Geometry { get; set; }
    }

    public class GeoJsonPoint
    {
        [JsonPropertyName("type")]
        public string Type { get; } = "Point";

        [JsonPropertyName("coordinates")]
        public double[] Coordinates { get; set; }
    }
}
