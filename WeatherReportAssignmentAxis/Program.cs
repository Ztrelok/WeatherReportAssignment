using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using WeatherReportAssignmentAxis.Services;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup DI and Logging
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation($"Program started at: {DateTime.Now}");

        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var smhiClient = new SmhiApiClient(httpClientFactory.CreateClient());

        // Run each task
        await CalculateAverageTemperatureAsync(smhiClient, logger);
        await CalculateRainfallInLundAsync(smhiClient, logger);
        await DisplayTemperaturesWithCancellationAsync(smhiClient, logger);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);

            // Suppress noisy HTTP logs (only show warnings or errors)
            builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        });
    }


    // Task 1: Average Temperature in Sweden (last hour)
    private static async Task CalculateAverageTemperatureAsync(SmhiApiClient smhiClient, ILogger logger)
    {
        logger.LogInformation("Calculating average temperature in Sweden (last hour)...");

        var stations = await smhiClient.GetAllStationsAsync();

        var temperatureTasks = stations.Select(async station =>
        {
            var data = await smhiClient.GetTemperatureLatestHourAsync(station.Id);
            var temp = data?.Value?.LastOrDefault()?.GetNumericValue();
            return temp;
        });

        var temperatures = await Task.WhenAll(temperatureTasks);
        var validTemperatures = temperatures.Where(t => t.HasValue).Select(t => t.Value).ToList();

        if (validTemperatures.Any())
        {
            var average = validTemperatures.Average();
            var formatted = average.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');
            Console.WriteLine($"The average temperature for Sweden in Sweden for the last hours was {formatted} degrees");
            logger.LogInformation($"Average temperature calculated: {average:F1} °C");
        }
        else
        {
            Console.WriteLine("No valid temperature data available.");
            logger.LogWarning("No temperature data found for averaging.");
        }
    }

    // Task 2: Total Rainfall in Lund (last months)
    private static async Task CalculateRainfallInLundAsync(SmhiApiClient smhiClient, ILogger logger)
    {
        logger.LogInformation("Calculating total rainfall in Lund (last months)...");

        var stations = await smhiClient.GetAllStationsAsync();

        var lundStation = stations.FirstOrDefault(s => s.Name.Equals("Lund", StringComparison.OrdinalIgnoreCase))
            ?? stations.FirstOrDefault(s => s.Name.Contains("Lund", StringComparison.OrdinalIgnoreCase));

        if (lundStation == null)
        {
            Console.WriteLine("No station found for Lund.");
            logger.LogWarning("No station found matching 'Lund'.");
            return;
        }

        Console.WriteLine($"Lund Station ID: {lundStation.Id}, Name: {lundStation.Name}");

        var data = await smhiClient.GetRainfallLatestMonthsAsync(lundStation.Id);

        if (data?.Value == null || !data.Value.Any())
        {
            Console.WriteLine("No rainfall data available for Lund.");
            logger.LogWarning("No rainfall data retrieved for Lund.");
            return;
        }

        var validValues = data.Value
            .Where(v => v.From > 0)
            .ToList();

        if (!validValues.Any())
        {
            Console.WriteLine("No valid rainfall measurements available for Lund.");
            logger.LogWarning("No valid rainfall measurements retrieved.");
            return;
        }

        var validDates = validValues.Select(v => v.From).ToList();
        var earliestDate = validDates.Min();
        var latestDate = validDates.Max();
        var fromDate = DateTimeOffset.FromUnixTimeMilliseconds(earliestDate).Date.ToString("yyyy-MM-dd");
        var toDate = DateTimeOffset.FromUnixTimeMilliseconds(latestDate).Date.ToString("yyyy-MM-dd");

        var rainfallByMonth = validValues
            .GroupBy(v => DateTimeOffset.FromUnixTimeMilliseconds(v.From).ToString("yyyy-MM"))
            .Select(group => new
            {
                Month = group.Key,
                TotalRainfall = group.Sum(v => v.GetNumericValue() ?? 0)
            }).ToList();

        foreach (var month in rainfallByMonth)
        {
            var formattedMonthly = month.TotalRainfall.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');
            Console.WriteLine($"Rainfall in {month.Month}: {formattedMonthly} mm");
        }

        var totalRainfall = validValues.Sum(v => v.GetNumericValue() ?? 0);
        var formattedRainfall = totalRainfall.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ',');

        Console.WriteLine($"\nBetween {fromDate} and {toDate} the total rainfall in Lund was {formattedRainfall} millimeters");
        logger.LogInformation($"Total rainfall in Lund calculated: {formattedRainfall} mm");
    }

    // Task 3: Display temperatures with 100ms delay and cancellation
    private static async Task DisplayTemperaturesWithCancellationAsync(SmhiApiClient smhiClient, ILogger logger)
    {
        logger.LogInformation("Starting temperature display with cancellation support...");

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _ = Task.Run(() =>
        {
            Console.WriteLine("Press any key to cancel...\n");
            Console.ReadKey(true);
            cts.Cancel();
        });

        var stations = await smhiClient.GetAllStationsAsync();

        foreach (var station in stations)
        {
            if (token.IsCancellationRequested)
            {
                Console.WriteLine("\nOperation cancelled by user.");
                logger.LogInformation("Temperature display cancelled.");
                break;
            }

            var data = await smhiClient.GetTemperatureLatestHourAsync(station.Id);
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

            try
            {
                await Task.Delay(100, token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("\nOperation cancelled during delay.");
                logger.LogInformation("Temperature display cancelled during delay.");
                break;
            }
        }

        if (!token.IsCancellationRequested)
        {
            Console.WriteLine("\nTemperature display completed.");
            logger.LogInformation("Temperature display finished.");
        }
    }

}
