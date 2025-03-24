using System.Collections.Concurrent;
using System.Globalization;

using Microsoft.Extensions.Logging;

using WeatherReportAssignment.Services;

namespace WeatherReportAssignment
{
    /// <summary>
    /// Handles reporting logic for weather observations using SMHI API.
    /// Encapsulates tasks for temperature averaging, rainfall calculation, and live temperature display with cancellation.
    /// </summary>
    public class WeatherReportService
    {
        private readonly SmhiApiClient _smhiClient;
        private readonly ILogger<WeatherReportService> _logger;

        public WeatherReportService(SmhiApiClient smhiClient, ILogger<WeatherReportService> logger)
        {
            _smhiClient = smhiClient;
            _logger = logger;
        }

        /// <summary>
        /// Task 1: Calculates the average temperature across all Swedish stations for the last hour.
        /// Uses ConcurrentDictionary for thread-safe, scalable parallel data collection.
        /// </summary>
        public async Task CalculateAverageTemperatureAsync()
        {
            _logger.LogInformation("Calculating average temperature in Sweden (last hour)...");

            try
            {
                var stations = await _smhiClient.GetAllStationsAsync();
                var tempResults = new ConcurrentDictionary<string, double>();
                int skippedCount = 0;

                // Fetch temperatures in parallel per station
                var temperatureTasks = stations.Select(async station =>
                {
                    try
                    {
                        var data = await _smhiClient.GetTemperatureLatestHourAsync(station.Id);
                        var temp = data?.Value?.LastOrDefault()?.GetNumericValue();

                        if (temp.HasValue)
                        {
                            var stationName = station.Name ?? $"Station_{station.Id}";
                            tempResults.TryAdd(stationName, temp.Value);
                        }
                        else
                        {
                            Interlocked.Increment(ref skippedCount); // Track skipped stations (null temp)
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Expected for stations with no temp data
                        _logger.LogDebug($"Temperature data not found for {station.Name} (404).");
                        Interlocked.Increment(ref skippedCount);
                    }
                    catch (HttpRequestException ex)
                    {
                        // Unexpected HTTP error
                        _logger.LogWarning($"Failed to get temperature for {station.Name}: {ex.Message}");
                        Interlocked.Increment(ref skippedCount);
                    }
                });

                await Task.WhenAll(temperatureTasks);

                if (tempResults.Any())
                {
                    var avg = tempResults.Values.Average();
                    var formatted = avg.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');
                    Console.WriteLine($"The average temperature for Sweden in Sweden for the last hours was {formatted} degrees");
                    _logger.LogInformation($"Average temp: {avg:F1} °C");
                }
                else
                {
                    Console.WriteLine("No valid temperature data available.");
                    _logger.LogWarning("No temperature data for averaging.");
                }

                Console.WriteLine($"{skippedCount} stations had no recent temperature data (404 or null).");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to fetch temperature data: {ex.Message}");
                _logger.LogError($"Temperature fetch failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Task 2: Calculates the total rainfall for the last months in a specified city.
        /// Allows flexible city input for reusable analysis.
        /// </summary>
        public async Task CalculateRainfallForCityAsync(string cityName)
        {
            _logger.LogInformation($"Calculating total rainfall in {cityName} (last months)...");

            try
            {
                var stations = await _smhiClient.GetAllStationsAsync();

                // Find station by exact or partial name match, safely handling null names
                var station = stations.FirstOrDefault(s => s.Name?.Equals(cityName, StringComparison.OrdinalIgnoreCase) == true)
                    ?? stations.FirstOrDefault(s => s.Name?.Contains(cityName, StringComparison.OrdinalIgnoreCase) == true);

                if (station == null)
                {
                    Console.WriteLine($"No station found for {cityName}.");
                    _logger.LogWarning($"No station found matching '{cityName}'.");
                    return;
                }

                Console.WriteLine($"{cityName} Station ID: {station.Id}, Name: {station.Name}");

                var data = await _smhiClient.GetRainfallLatestMonthsAsync(station.Id);

                if (data?.Value == null || !data.Value.Any())
                {
                    Console.WriteLine($"No rainfall data available for {cityName}.");
                    _logger.LogWarning($"No rainfall data retrieved for {cityName}.");
                    return;
                }

                var validValues = data.Value.Where(v => v.From > 0).ToList();
                if (!validValues.Any())
                {
                    Console.WriteLine($"No valid rainfall measurements available for {cityName}.");
                    _logger.LogWarning($"No valid rainfall measurements retrieved.");
                    return;
                }

                // Extract date range
                var fromDate = DateTimeOffset.FromUnixTimeMilliseconds(validValues.Min(v => v.From)).Date.ToString("yyyy-MM-dd");
                var toDate = DateTimeOffset.FromUnixTimeMilliseconds(validValues.Max(v => v.From)).Date.ToString("yyyy-MM-dd");

                // Group rainfall per month
                var rainfallByMonth = validValues.GroupBy(v => DateTimeOffset.FromUnixTimeMilliseconds(v.From).ToString("yyyy-MM"))
                    .Select(g => new { Month = g.Key, TotalRainfall = g.Sum(v => v.GetNumericValue() ?? 0) })
                    .ToList();

                foreach (var month in rainfallByMonth)
                {
                    var formattedMonthly = month.TotalRainfall.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');
                    Console.WriteLine($"Rainfall in {month.Month}: {formattedMonthly} mm");
                }

                var totalRainfall = validValues.Sum(v => v.GetNumericValue() ?? 0);
                var formattedTotal = totalRainfall.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');

                Console.WriteLine($"\nBetween {fromDate} and {toDate} the total rainfall in {cityName} was {formattedTotal} millimeters");
                _logger.LogInformation($"Total rainfall in {cityName}: {formattedTotal} mm");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to fetch rainfall data for {cityName}: {ex.Message}");
                _logger.LogError($"Rainfall fetch failed for {cityName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Task 3: Displays live temperature for each station with a 100ms delay.
        /// Supports user cancellation via key press and CancellationToken.
        /// </summary>
        public async Task DisplayTemperaturesWithCancellationAsync()
        {
            _logger.LogInformation("Starting temperature display with cancellation support...");
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Start cancellation listener
            _ = Task.Run(() =>
            {
                Console.WriteLine("Press any key to cancel...\n");
                Console.ReadKey(true);
                cts.Cancel();
            });

            try
            {
                var stations = await _smhiClient.GetAllStationsAsync();

                foreach (var station in stations)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("\nOperation cancelled by user.");
                        _logger.LogInformation("Temperature display cancelled.");
                        break;
                    }

                    try
                    {
                        var data = await _smhiClient.GetTemperatureLatestHourAsync(station.Id);
                        var temp = data?.Value?.LastOrDefault()?.GetNumericValue();

                        if (temp.HasValue)
                        {
                            var formattedTemp = temp.Value.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');
                            Console.WriteLine($"{station.Name}: {formattedTemp}");
                        }
                        else
                        {
                            Console.WriteLine($"{station.Name}:");
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogDebug($"Temperature data not found for {station.Name} (404).");
                        Console.WriteLine($"{station.Name}:");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"{station.Name}: Failed to fetch data");
                        _logger.LogWarning($"Failed to fetch temperature for {station.Name}: {ex.Message}");
                    }

                    try
                    {
                        await Task.Delay(100, token);
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("\nOperation cancelled during delay.");
                        _logger.LogInformation("Temperature display cancelled during delay.");
                        break;
                    }
                }

                if (!token.IsCancellationRequested)
                {
                    Console.WriteLine("\nTemperature display completed.");
                    _logger.LogInformation("Temperature display finished.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during temperature display: {ex.Message}");
                _logger.LogError($"Temperature display failed: {ex.Message}");
            }
        }
    }
}
