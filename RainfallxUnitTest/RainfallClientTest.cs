using System.Net;
using Moq;
using Moq.Protected;
using WeatherReportAssignment.Services;
using System.Text;

public class RainfallClientTest
{
    [Fact]
    public async Task GetRainfallLatestMonthsAsync_ReturnsCorrectTotal()
    {
        // Arrange: fake JSON from SMHI
        var jsonResponse = @"{
            ""value"": [
                { ""from"": 1740895201000, ""to"": 1740981600000, ""value"": ""1.2"", ""quality"": ""G"" },
                { ""from"": 1740981601000, ""to"": 1741068000000, ""value"": ""2.3"", ""quality"": ""G"" }
            ]
        }";

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        var client = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://opendata-download-metobs.smhi.se/api/version/latest/")
        };

        var smhiClient = new SmhiApiClient(client);

        // Act
        var data = await smhiClient.GetRainfallLatestMonthsAsync(53430);

        // Assert
        Assert.NotNull(data);
        Assert.Equal(2, data.Value.Count);
        var total = data.Value.Sum(v => v.GetNumericValue() ?? 0);
        Assert.Equal(3.5, total);
    }
}
