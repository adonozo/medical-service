# A voice assistant for type 2 diabetes mellitus treatments
Author: Alvaro Rene Donozo Fernandez

Student ID: 200775173

Supervisor: Dr. Eliane Bodanese

---
## REST API Service
This project was built with [.NET 5.0](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) and written in C#.

The initial folder structure was created from the .NET Web API template. Then, more projects were added.

Besides Microsoft's built-in libraries, this projects uses the following libraries:
 
 - The official library of the HL7 FHIR for .NET: [Hl7.Fhir.R4 3.0.4](https://github.com/FirelyTeam/firely-net-sdk)
 - The official .NET driver for MongoDB: [MongoDB.Driver 2.13.0](https://www.nuget.org/packages/MongoDB.Driver)
 - Noda Time, a date and time API: [NodaTime 3.0.5](https://www.nuget.org/packages/NodaTime)

### Requirements

The .NET 5.0 SDK or later must be installed. More information in the [official documentation](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net50).

It is important to connect to a MongoDB database instance. To do so, add the connection string to the `appsettings.json` and/or `appsettings.Development.json` files. They are located in:

```
./src/api/QMUL.DiabetesBackend.Controllers
```

You must change the `MongoDatabaseSettings` property with adequate values:

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

### Running the provided executable

An executable version of the system in the `executable` folder. To run it, add the MongoDB connection string to the `appsettings.json` and `appsettings.Development.json` in the `./executable/` folder.

Then, open a terminal in the `executable` folder and execute:

```bash
dotnet QMUL.DiabetesBackend.Controllers.dll
```
