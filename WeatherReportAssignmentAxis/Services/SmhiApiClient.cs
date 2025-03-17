using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using WeatherReportAssignmentAxis.Models;

namespace WeatherReportAssignmentAxis.Services
{
    public class SmhiApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://opendata-download-metobs.smhi.se/api/version/latest/";

        public SmhiApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        /// <summary>
        /// Retrieves all weather stations that report temperature (parameter 1).
        /// </summary>
        public async Task<List<StationMetadata>> GetAllStationsAsync()
        {
            string url = "parameter/1.json";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch parameter metadata: {response.StatusCode}");

            var contentStream = await response.Content.ReadAsStreamAsync();
            var parameterData = await JsonSerializer.DeserializeAsync<ParameterWithStations>(
                contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return parameterData?.Station ?? new List<StationMetadata>();
        }

        /// <summary>
        /// Retrieves latest-hour temperature data for a station.
        /// </summary>
        public async Task<ObservationData?> GetTemperatureLatestHourAsync(int stationId)
        {
            string url = $"parameter/1/station/{stationId}/period/latest-hour/data.json";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var contentStream = await response.Content.ReadAsStreamAsync();
            var observation = await JsonSerializer.DeserializeAsync<ObservationData>(
                contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return observation;
        }

        /// <summary>
        /// Retrieves latest-months rainfall data for a station.
        /// </summary>
        public async Task<ObservationData?> GetRainfallLatestMonthsAsync(int stationId)
        {
            string url = $"parameter/5/station/{stationId}/period/latest-months/data.json";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Rainfall API call failed: {response.StatusCode}");
                return null;
            }

            var rawContent = await response.Content.ReadAsStringAsync();
            var observation = JsonSerializer.Deserialize<ObservationData>(
                rawContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return observation;
        }
    }
}
