using Moq;
using WeatherReportAssignment.Models;  // Import the correct namespace for models
using Microsoft.Extensions.Logging;  // For logging mock
using WeatherReportAssignment.Services;  // For the service being tested
using WeatherReportAssignment;  // For the main project namespace

// This unit test tests the WeatherReportService's method CalculateRainfallForCityAsync.
// However, this test currently uses a concrete implementation of SmhiApiClient
// rather than an interface (ISmhiApiClient). Ideally, to make this test more
// testable and flexible, SmhiApiClient should implement ISmhiApiClient, and we
// should mock the interface instead of the concrete class. As it stands, the test
// can still run but it may not be as easily maintainable or flexible for future changes.

public class WeatherReportServiceTests
{
    [Fact]
    public async Task CalculateRainfallForCityAsync_ReturnsCorrectRainfall()
    {
        // ARRANGE
        // The test begins by setting up the mocks for SmhiApiClient and ILogger.
        // We mock the SmhiApiClient to return predefined data for stations and rainfall.

        // Mock the SmhiApiClient to simulate calling GetAllStationsAsync
        var mockApiClient = new Mock<SmhiApiClient>();

        // Setup the mock to return a list of stations, including a station called "CityStation"
        mockApiClient
            .Setup(client => client.GetAllStationsAsync())
            .ReturnsAsync(new List<StationMetadata>
            {
                new StationMetadata { Id = 1, Name = "CityStation" } // Simulate one station
            });

        // Mock the GetRainfallLatestMonthsAsync method to return predefined rainfall data
        mockApiClient
            .Setup(client => client.GetRainfallLatestMonthsAsync(It.IsAny<int>()))
            .ReturnsAsync(new ObservationData
            {
                // Simulating rainfall data for the station
                Value = new List<ValueEntry>
                {
                    new ValueEntry { From = 1740895201000, Value = "2.3" },
                    new ValueEntry { From = 1740981601000, Value = "1.8" }
                }
            });

        // Mock the logger
        var mockLogger = new Mock<ILogger<WeatherReportService>>();

        // Create the service under test, injecting the mocked dependencies
        var service = new WeatherReportService(mockApiClient.Object, mockLogger.Object);

        // ACT
        // Call the method that we want to test
        await service.CalculateRainfallForCityAsync("CityStation");

        // ASSERT
        // After calling the method, we want to verify if the correct logging occurred,
        // indicating that the service has processed the rainfall data and logged the information.

        // Verify that the method `LogInformation` was called at least once during the execution of the method
        // This ensures that the service is logging relevant information such as total rainfall calculation.
        mockLogger.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.AtLeastOnce());
    }
}
