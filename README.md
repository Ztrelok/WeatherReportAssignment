WeatherReportAssignmentAxis

Overview

Console application in C# that uses SMHI’s open API to:

    Calculate average temperature in Sweden (last hour)
    Calculate total rainfall in a specified city (e.g., Lund, last months)
    Output temperature per weather station with cancellation support

How to Run

    Open WeatherReportAssignmentAxis.sln in Visual Studio 2022+
    Set WeatherReportAssignmentAxis as the startup project
    Press Ctrl + F5 to run — results will appear in the console
    Task 3: Press any key during station output to cancel

Run Unit Tests

    Open Test Explorer (Test > Test Explorer)
    Run all tests from WeatherReportAssignmentAxis.Tests
    Or via CLI:

    dotnet test WeatherReportAssignmentAxis.Tests

Dependencies

    .NET 8.0 SDK (https://dotnet.microsoft.com/en-us/download)
    SMHI Open Data API (https://opendata.smhi.se/apidocs/metobs)

Features

    HttpClientFactory for efficient API calls
    Structured Logging (console + warning suppression for noisy logs)
    ConcurrentDictionary for thread-safe parallel processing
    CancellationToken support with clean interrupt handling
    Fully null-safe models for robust deserialization

Configuration

To change the city for rainfall analysis (Task 2):
Edit in Program.cs:

await weatherService.CalculateRainfallForCityAsync("Stockholm");

Example Output

The average temperature for Sweden in Sweden for the last hours was 4,7 degrees

Rainfall in 2024-11: 29,9 mm
Rainfall in 2024-12: 64,8 mm
Rainfall in 2025-01: 59,2 mm
Rainfall in 2025-02: 16,5 mm
Rainfall in 2025-03: 7,6 mm

Between 2024-11-06 and 2025-03-16 the total rainfall in Lund was 178,0 millimeters

Press any key to cancel...

Göteborg A: 12,4
Hagshult Mo: 12,1
Hallands Väderö A: 11,8
Halmstad:
Hamra A: 9,3
Hanö A: 9,9
...