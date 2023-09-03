# A voice assistant for type 2 diabetes mellitus treatments

This is the backend service for the voice assistant.

---

## REST API Service

This project runs on [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Models built with the HL7 FHIR package: [Hl7.Fhir.R5](https://github.com/FirelyTeam/firely-net-sdk)

Unit tests created with [XUnit](https://xunit.net) and [FluentAssertions](https://fluentassertions.com)

### Requirements

The .NET 6.0 SDK or later must be installed. More information in the [official documentation](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60).

A running MongoDB instance. The connection string must be set on `appsettings.json`, or `appsettings.Development.json` when running locally. These are located in:

```
./src/api/QMUL.DiabetesBackend.Controllers
```

Update the `MongoDatabaseSettings` property with the connection string and database nam:

```json
{
  "MongoDatabaseSettings" : {
    "DatabaseName": "diabetes",
    "DatabaseConnectionString": "mongodb+srv://..."
  },
  ...
}

```

### Build the project

In a terminal, move to the project's root folder and run:

```bash
dotnet build
```

### Running the project

After a successful build, execute the following command in a terminal, from the project's root folder:

```bash
dotnet run --project ./src/api/QMUL.DiabetesBackend.Controllers/QMUL.DiabetesBackend.Controllers.csproj
```

### Running tests

Run in the project's root folder:

```bash
dotnet test
```
