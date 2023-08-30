using Itinero;
using Itinero.IO.Osm;
using OsmSharp.Streams;
using OsmSharp;
using Itinero.Profiles;
using Vehicle = Itinero.Osm.Vehicles.Vehicle;
using System.Text.Json;
using Node = OsmSharp.Node;
using SafeMap.OSMParser;

public class OSMFileParser
{
    private readonly string filePath;
    private readonly ItineroFilesNamingProvider itineroFilesNamingProvider;
    public OSMFileParser(string filePath)
    {
        this.filePath = filePath;
        itineroFilesNamingProvider = new ItineroFilesNamingProvider(filePath);
    }

    public async Task Parse()
    {
        if (!File.Exists(filePath))
            throw new Exception($"File not found at path: {filePath}");

        var routerDb = GetRouterDbFromOSM();

        //task to save routerDb to disk and read from there onwards
        var saveTask = Task.Run(() => SaveRouterDbCache(routerDb));

        //task to iterate through nodes and find elements to include
        //in the safety algorithm
        var findTask = Task.Run(FindSecurityElements);

        //completes both tasks
        await Task.WhenAll(new[] { saveTask, findTask });

        //now that we have the elements in OSM that will include,
        //we need to map them all to its associated Edge and vertex
        //values in routerDb.
        var elements = findTask.Result;
        MapElementsToItinero(routerDb, elements);

        //we save the result for future uses
        //TODO: save using protobuf.
        using var stream = File.OpenWrite(itineroFilesNamingProvider.SafetyScoreParametersFileName);
        await JsonSerializer.SerializeAsync(stream, elements);

        //TODO: calculate safety score
    }

    /// <summary>
    /// Maps the supplied security elements to a Node/vertex in
    /// Itinero route DB.
    /// </summary>
    private void MapElementsToItinero(RouterDb routerDb, List<MapSecurityElement> elements)
    {
        var pedestrian = routerDb.GetSupportedProfile("pedestrian");
        var profiles = new IProfileInstance[] { pedestrian };
        var router = new Router(routerDb);

        Parallel.ForEach(elements, element =>
        {
            var point = router.TryResolve(profiles, (float)element.Lat, (float)element.Lng, 50);
            if (point.IsError)
            {
                element.ItineroMappingError = point.ErrorMessage;
            }
            else
            {
                element.EdgeId = point.Value.EdgeId;
                element.VertexId = point.Value.VertexId(routerDb);
            }
        });

    }

    private async void SaveRouterDbCache(RouterDb routerDb)
    {
        //information parsed in saved, to avoid the need of parsing it again
        using var outputStream = File.OpenWrite(itineroFilesNamingProvider.ItineroRouteFileName);
        routerDb.Serialize(outputStream);
    }

    private RouterDb GetRouterDbFromOSM()
    {
        var routerDb = new RouterDb();

        //router db file parsing
        using var stream = File.OpenRead(filePath);
        routerDb.LoadOsmData(stream, new[] { Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian });

        return routerDb;
    }

    /// <summary>
    /// Iterates through OpenStreetMap file looking
    /// for elements to be included in the safety score
    /// algorythm
    /// </summary>
    private List<MapSecurityElement> FindSecurityElements()
    {
        using var stream = File.OpenRead(filePath);
        var source = new PBFOsmStreamSource(stream);

        var elements = new List<MapSecurityElement>(1000);
        // Process each OSM object.
        foreach (var osmGeo in source)
        {
            if (osmGeo.Type != OsmGeoType.Node) continue;

            var node = (Node)osmGeo;
            var elementType = CheckForTestData(node);

            //if there wasn't any test data, and there are tags in the
            //node, we look for security elements
            if (elementType == null && node?.Tags?.Any() == true)
            {
                elementType = GetElementType(node);
            }

            if (elementType == null) continue;

            var element = new MapSecurityElement
            {
                Type = elementType.Value,
                Lat = node!.Latitude!.Value,
                Lng = node.Longitude!.Value,
                OSMNodeId = node.Id!.Value
            };
            elements.Add(element);
        }

        return elements;
    }

    private static SecurityElementTypes? CheckForTestData(Node node)
    {
        //for testing purposes, we set an square on Berlin where the score is 5
        //points for every node. thus, it would be a "no-go" zone, and to access
        //any point arount it you should always taking a less direct path
        if (node.Longitude >= 13.417867109181504 && node.Longitude <= 13.426879331471543 &&
            node.Latitude >= 52.50702709960917 && node.Latitude <= 52.51120626576865)
        {
            return SecurityElementTypes.Test_5_Points;
        }
        return null;
    }

    /// <summary>
    /// Checks if the supplied element represent one of
    /// the elements to be included in the safety algorythm
    /// </summary>
    private static SecurityElementTypes? GetElementType(Node node)
    {
        if (node.Tags.Contains("highway", "street_lamp")) return SecurityElementTypes.StreetLamp;
        else if (node.Tags.Contains("man_made", "surveillance")) return SecurityElementTypes.CCTV;
        else if (node.Tags.Contains("amenity", "bus_station")) return SecurityElementTypes.BusStation;
        else if (node.Tags.Contains("railway", "station")) return SecurityElementTypes.RailwayStation;
        else if (node.Tags.Contains("amenity", "police")) return SecurityElementTypes.PoliceStation;

        return null;
    }

    public class MapSecurityElement
    {
        public long OSMNodeId { get; set; }
        public double Lat { get; set; }

        public double Lng { get; set; }

        public SecurityElementTypes Type { get; set; }

        public string? ItineroMappingError { get; set; }
        public uint EdgeId { get; set; }

        public uint VertexId { get; set; }
    }

    public enum SecurityElementTypes
    {
        StreetLamp = 1,
        CCTV,
        BusStation,
        RailwayStation,
        PoliceStation,

        Test_5_Points = 100
    }
}
