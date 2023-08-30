using SafePath;
using Volo.Abp;

Console.WriteLine("Initializing App");

// 1: Create the ABP application container
using var application = await AbpApplicationFactory.CreateAsync<OSMParserModule>();


// 2: Initialize/start the ABP Framework (and all the modules)
await application.InitializeAsync();

Console.WriteLine("App Initialized");

var path = args.Length > 0 ? args[0] : null;

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

var parser = new OSMFileParser(path);
await parser.Parse();

// 3: Stop the ABP Framework (and all the modules)
await application.ShutdownAsync();
