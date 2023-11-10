using SafePath.Entities.FastStorage;
using System.Collections.Generic;
using System.Linq;

namespace SafePath.Services
{
    public interface ISafetyScoreCalculator
    {
        float Calculate(IEnumerable<MapElement> elements);
    }

    public class SimpleSafetyScoreCalculator : ISafetyScoreCalculator
    {
        public float Calculate(IEnumerable<MapElement> elements)
        {
            if (elements?.Any() != true) return 0;

            var score = 0f;
            foreach (var element in elements)
            {
                var elementScore = GetSecurityScoreByType(element.Type);
                //TODO: complete radiance
                /*
                if (element.Radiance.HasValue)
                {
                    foreach (var otherElement in elements)
                    {
                        var distance = CalculateDistance(
                            element.Lat, element.Long,
                            otherElement.Lat, otherElement.Long
                        );
                        if (distance <= element.Radiance.Value)
                        {
                            otherElement.SecurityRate += element.SecurityRate;
                        }
                    }
                }
                */
                if (elementScore > score)
                {
                    score = elementScore;
                }
            }
            return score;
        }

        private float GetSecurityScoreByType(SecurityElementTypes type)
        {
            float rate = 0;
            switch (type)
            {
                case SecurityElementTypes.PoliceStation:
                    rate = 2;
                    break;
                case SecurityElementTypes.BusStation:
                case SecurityElementTypes.Hospital:
                case SecurityElementTypes.RailwayStation:
                    rate = 1.5f;
                    break;
                case SecurityElementTypes.GovernmentBuilding:
                    rate = 1.4f;
                    break;
                case SecurityElementTypes.CCTV:
                    rate = 1.25f;
                    break;
                case SecurityElementTypes.Leisure:
                case SecurityElementTypes.Amenity:
                case SecurityElementTypes.EducationCenter:
                case SecurityElementTypes.HealthCenter:
                    rate = 1.2f;
                    break;
                case SecurityElementTypes.StreetLamp:
                    rate = 1.1f;
                    break;
                case SecurityElementTypes.Semaphore:
                    rate = 1.05f;
                    break;
                case SecurityElementTypes.Test_5_Points:
                    rate = 5;
                    break;
            }

            //TODO: add context variation
            return rate;
        }
    }

}
