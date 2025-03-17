# WeatherReportAssignmentAxis

## Overview
Console application in C# that uses SMHI’s open API to:
1. Calculate average temperature in Sweden (last hour)
2. Calculate total rainfall in Lund (last months)
3. Output temperatures per weather station with cancellation support

## How to Run
1. Open `WeatherReportAssignmentAxis.sln` in Visual Studio 2022+
2. Set **WeatherReportAssignmentAxis** as the startup project
3. Press `Ctrl+F5` to run — results will appear in the console
4. Task 3: Press any key during station output to cancel

## Run Unit Tests
1. Open **Test Explorer** (Test > Test Explorer)
2. Run all tests from **WeatherReportAssignmentAxis.Tests**
3. Or via CLI: `dotnet test WeatherReportAssignmentAxis.Tests`

## Dependencies
- .NET 7 (or adapt for .NET 6 if needed)
- SMHI Open Data API
