using System.Text.Json;

using WeatherReportAssignment.Models;

namespace WeatherReportAssignment.Services
{
    /// <summary>
    /// Handles interaction with the SMHI (Swedish Meteorological and Hydrological Institute) open data API.
    /// Provides access to weather observations such as temperature and rainfall.
    /// </summary>
    public class SmhiApiClient
    {
        private readonly HttpClient _httpClient;

        // Base URL for the SMHI open data API (latest version)
        private const string BaseUrl = "https://opendata-download-metobs.smhi.se/api/version/latest/";

        public SmhiApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        /// <summary>
        /// Retrieves metadata for all weather stations that report temperature (parameter 1).
        /// </summary>
        /// <returns>List of weather stations with temperature data capabilities.</returns>
        public async Task<List<StationMetadata>> GetAllStationsAsync()
        {
            const string url = "parameter/1.json"; // Temperature parameter ID is 1
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode(); // Throws exception if not successful (e.g., 404, 500)

            var contentStream = await response.Content.ReadAsStreamAsync();
            var parameterData = await JsonSerializer.DeserializeAsync<ParameterWithStations>(
                contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return parameterData?.Station ?? new List<StationMetadata>();
        }

        /// <summary>
        /// Retrieves the latest temperature observation (past hour) for a given weather station.
        /// </summary>
        /// <param name="stationId">The SMHI station ID.</param>
        /// <returns>Observation data containing temperature readings.</returns>
        public async Task<ObservationData?> GetTemperatureLatestHourAsync(int stationId)
        {
            string url = $"parameter/1/station/{stationId}/period/latest-hour/data.json"; // Parameter 1: Temperature
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var contentStream = await response.Content.ReadAsStreamAsync();
            var observation = await JsonSerializer.DeserializeAsync<ObservationData>(
                contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return observation;
        }

        /// <summary>
        /// Retrieves the latest rainfall data (last months) for a given weather station.
        /// </summary>
        /// <param name="stationId">The SMHI station ID.</param>
        /// <returns>Observation data containing rainfall measurements.</returns>
        public async Task<ObservationData?> GetRainfallLatestMonthsAsync(int stationId)
        {
            string url = $"parameter/5/station/{stationId}/period/latest-months/data.json"; // Parameter 5: Rainfall
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var rawContent = await response.Content.ReadAsStringAsync();
            var observation = JsonSerializer.Deserialize<ObservationData>(
                rawContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return observation;
        }
    }
}
