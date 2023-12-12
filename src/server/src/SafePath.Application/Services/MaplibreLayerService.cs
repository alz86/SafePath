using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using SafePath.DTOs;
using SafePath.Entities.FastStorage;

namespace SafePath.Services
{
    public interface IMaplibreLayerService
    {
        Task<GeoJsonFeatureCollection> GetMaplibreLayer(string[] storageKeys);

        Task<GeoJsonFeatureCollection> GenerateMaplibreLayer(IEnumerable<MapElement> elements, string[] storageKeys);
    }

    public class MaplibreLayerService : IMaplibreLayerService
    {
        private readonly IStorageProviderService storageProviderService;

        public MaplibreLayerService(IStorageProviderService storageProviderService)
        {
            this.storageProviderService = storageProviderService;
        }

        public async Task<GeoJsonFeatureCollection> GetMaplibreLayer(string[] storageKeys)
        {
            using var stream = storageProviderService.OpenRead(storageKeys);
            return (await JsonSerializer.DeserializeAsync<GeoJsonFeatureCollection>(stream))!;
        }

        public async Task<GeoJsonFeatureCollection> GenerateMaplibreLayer(IEnumerable<MapElement> elements, string[] storageKeys)
        {
            var geoJson = new GeoJsonFeatureCollection();

            foreach (var element in elements)
            {
                var feature = new GeoJsonFeature
                {
                    Geometry = new GeoJsonPoint
                    {
                        Coordinates = [element.Lng, element.Lat]
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "OSMNodeId", element.OSMNodeId },
                        { "type", element.Type.ToString() }
                    }
                };

                geoJson.Features.Add(feature);
            }

            using (var stream = storageProviderService.OpenWrite(storageKeys))
            {
                await JsonSerializer.SerializeAsync(stream, geoJson);
            }

            return geoJson;
        }
    }
}