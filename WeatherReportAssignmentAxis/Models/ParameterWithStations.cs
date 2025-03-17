using System.Collections.Generic;

namespace WeatherReportAssignmentAxis.Models
{
    public class ParameterWithStations
    {
        public List<StationMetadata> Station { get; set; } = new List<StationMetadata>();
    }
}