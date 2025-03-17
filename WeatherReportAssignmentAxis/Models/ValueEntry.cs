using System.Globalization;

namespace WeatherReportAssignmentAxis.Models
{
    public class ValueEntry
    {
        public long From { get; set; }
        public long To { get; set; }
        public string Value { get; set; }
        public string Quality { get; set; }

        public double? GetNumericValue()
        {
            if (double.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;
            return null;
        }
    }
}
