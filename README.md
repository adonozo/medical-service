# A voice assistant for type 2 diabetes mellitus treatments

This is the backend service for the voice assistant.

---
## REST API Service
This project was built with [.NET 5.0](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) and written in C#.

The initial folder structure was created from the .NET Web API template.

Besides Microsoft's built-in libraries, this projects uses the following libraries:
 
 - The official library of the HL7 FHIR for .NET: [Hl7.Fhir.R4](https://github.com/FirelyTeam/firely-net-sdk)
 - The official .NET driver for MongoDB: [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver)
 - Noda Time, a date and time API: [NodaTime](https://www.nuget.org/packages/NodaTime)

Unit tests were created with [XUnit](https://xunit.net) and [FluentAssertions](https://fluentassertions.com)

### Requirements

The .NET 5.0 SDK or later must be installed. More information in the [official documentation](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net50).

It is important to connect to a MongoDB database instance. To do so, add the connection string to the `appsettings.json` and/or `appsettings.Development.json` files. They are located in:

```
./src/api/QMUL.DiabetesBackend.Controllers
```

You must change the `MongoDatabaseSettings` property with proper values:

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
