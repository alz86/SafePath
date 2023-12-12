
namespace SafePath.DTOs
{
    public class MapSecurityElementDto
    {
        public long OSMNodeId { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public SecurityElementTypesDto Type { get; set; }

    }

    public enum SecurityElementTypesDto
    {
        //OSM items
        StreetLamp = 1,
        CCTV,
        BusStation,
        RailwayStation,
        PoliceStation,
        Hospital,
        Semaphore,
        BusStop,
        GovernmentBuilding,
        EducationCenter,
        HealthCenter,
        Leisure,
        Amenity,

        //Crime reports
        CrimeReport_Severity_1,
        CrimeReport_Severity_2,
        CrimeReport_Severity_3,
        CrimeReport_Severity_4,
        CrimeReport_Severity_5,

        //test dat
        Test_5_Points = 100
    }
}
