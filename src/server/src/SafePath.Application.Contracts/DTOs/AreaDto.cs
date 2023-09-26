using System;

namespace SafePath.DTOs
{
    public class AreaDto
    {
        public string DisplayName { get;set; }

        public Guid Id { get; set; } 

        public double InitialLatitude { get; set; }

        public double InitialLongitude { get; set; }
    }
}
