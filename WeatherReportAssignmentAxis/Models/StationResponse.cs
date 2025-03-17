namespace WeatherReportAssignmentAxis.Models
{
    /// <summary>
    /// API response wrapper containing list of stations.
    /// </summary>
    public class StationResponse
    {
        /// <summary>
        /// List of station metadata. May be null if no stations are returned.
        /// </summary>
        public List<StationMetadata>? Stations { get; set; }
    }
}