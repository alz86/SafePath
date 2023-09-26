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
        StreetLamp = 1,
        CCTV,
        BusStation,
        RailwayStation,
        PoliceStation,
        Hospital,
        Semaphore,

        Test_5_Points = 100
    }
}
