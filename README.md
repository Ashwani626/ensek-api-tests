# ENSEK API Automation Tests (C#)

This project automates tests for ENSEK candidate API endpoints using:
- NUnit
- RestSharp
- FluentAssertions
- Newtonsoft.Json

## How to Run
1. Clone the repository.
2. Open the solution in Visual Studio.
3. Restore NuGet packages.
4. Run tests using Test Explorer.

## Tests Included
- Reset test data
- Buy fuel (gas/electricity)
- Verify /orders response
- Validate past order timestamps
- Negative tests for invalid input