namespace SafePath.Classes
{
    public class SecurityElement
    {
        public double Lat { get; set; }
        public double Long { get; set; }
        public Types Type { get; set; }
        public int? Radiance { get; set; }
        public double SecurityRate { get; set; }

        public enum Types
        {
            PublicLightning = 1,
            Semaphore = 2,
            ComercialArea,
            PoliceStation,
            Hospital
        }
    }
}
