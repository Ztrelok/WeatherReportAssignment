namespace WeatherReportAssignmentAxis.Models
{
    /// <summary>
    /// Represents metadata for a weather station.
    /// </summary>
    public class StationMetadata
    {
        public int Id { get; set; }

        // Nullable string for station name (API may omit)
        public string? Name { get; set; }

        public double? Height { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}