namespace WeatherReportAssignment.Models
{
    /// <summary>
    /// Represents weather observation data for temperature or rainfall.
    /// </summary>
    public class ObservationData
    {
        /// <summary>
        /// Observation values. May be null if no data exists for the time period.
        /// </summary>
        public List<ValueEntry>? Value { get; set; }
    }
}