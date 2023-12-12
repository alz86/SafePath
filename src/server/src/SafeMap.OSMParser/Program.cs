using Microsoft.Extensions.DependencyInjection;
using SafePath.Entities;
using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using SafePath.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

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
//OSMDataParsingService(IRepository<Area, Guid> areaRepository, IStorageProviderService storageProviderService, IAreaSetupProgressService areaSetupProgressService, ISafetyScoreCalculator safetyScoreCalculator, IFastStorageRepositoryBase<MapElement> mapElementRepository, IFastStorageRepositoryBase<SafetyScoreElement> safetyScoreElementRepository)
var areaRepository = application.Services.GetRequiredService<IRepository<Area, Guid>>();
var storageProviderService = application.Services.GetRequiredService<IStorageProviderService>();
var areaSetupProgressService = application.Services.GetRequiredService<IAreaSetupProgressService>();
var safetyScoreCalculator = application.Services.GetRequiredService<ISafetyScoreCalculator>();
var mapElementRepository = application.Services.GetRequiredService<IMapElementRepository>();
var safetyScoreElementRepository = application.Services.GetRequiredService<ISafetyScoreElementRepository>();
var maplibreLayerService = application.Services.GetRequiredService<IMaplibreLayerService>();

var s = new OSMDataParsingService(areaRepository, storageProviderService, areaSetupProgressService, safetyScoreCalculator, mapElementRepository, safetyScoreElementRepository, maplibreLayerService);
await s.Parse(Guid.NewGuid(), new[] { "..", "..", "..", "Data" });

// ABP tier down
await application.ShutdownAsync();
