using System.Globalization;

namespace WeatherReportAssignmentAxis.Models
{
    /// <summary>
    /// Represents a single data point for temperature or rainfall.
    /// </summary>
    public class ValueEntry
    {
        public long From { get; set; }
        public long To { get; set; }

        // Nullable value and quality — safe for incomplete data
        public string? Value { get; set; }
        public string? Quality { get; set; }

        /// <summary>
        /// Parses the string value into a numeric (double), or null if parsing fails.
        /// </summary>
        public double? GetNumericValue()
        {
            if (!string.IsNullOrEmpty(Value) &&
                double.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;

            return null;
        }
    }
}