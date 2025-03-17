using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using WeatherReportAssignmentAxis;
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
        var reportService = new WeatherReportService(smhiClient, serviceProvider.GetRequiredService<ILogger<WeatherReportService>>());

        // Run tasks
        try
        {
            await reportService.CalculateAverageTemperatureAsync();
            await reportService.CalculateRainfallForCityAsync("Lund"); // You can change this to any city
            await reportService.DisplayTemperaturesWithCancellationAsync();
        }
        catch (Exception ex)
        {
            logger.LogError($"Unhandled exception: {ex.Message}");
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        });
    }
}
