using SafePath.Services;
using Volo.Abp;

Console.WriteLine("Initializing App");

// ABP application container
using var application = await AbpApplicationFactory.CreateAsync<OSMParserModule>();

// ABP Framework init 
await application.InitializeAsync();

Console.WriteLine("App Initialized");

//app workload
var path = args.FirstOrDefault();

//test path
//path = "C:\\Code\\SafePath\\abp\\SafePath\\src\\server\\src\\SafeMap.OSMParser\\berlin-latest.osm.pbf";

if (string.IsNullOrWhiteSpace(path))
{
    Console.WriteLine("Please, enter the full path to the .osm.pbf file");
    path = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(path))
    {
        Console.WriteLine("Path not provided. Exiting");
        return;
    }
}

//var parser = application.ServiceProvider.GetService<IOSMDataParsingService>();
await new OSMDataParsingService().Parse(path);

// ABP tier down
await application.ShutdownAsync();
