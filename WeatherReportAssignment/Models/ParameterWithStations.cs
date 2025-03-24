namespace WeatherReportAssignment.Models
{
    /// <summary>
    /// Represents parameter data including associated stations (e.g., temperature stations).
    /// </summary>
    public class ParameterWithStations
    {
        public List<StationMetadata> Station { get; set; } = new(); // Initialized to prevent nulls
    }
}
