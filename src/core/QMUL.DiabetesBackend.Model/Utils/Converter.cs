namespace QMUL.DiabetesBackend.Model.Utils;

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public static class Converter
{
    private static readonly ParserSettings DefaultParserSettings = new()
    {
        AllowUnrecognizedEnums = true,
        AcceptUnknownMembers = false,
        PermissiveParsing = false
    };

    /// <summary>
    /// Converts an object to an instance of <see cref="DataType"/>, using JSON underneath.
    /// </summary>
    /// <param name="data">The object to convert.</param>
    /// <typeparam name="T">The data type to convert into.</typeparam>
    /// <returns>The converted object, or null if the conversion was not successful.</returns>
    public static async Task<T> ToDataTypeAsync<T>(object data) where T : DataType
    {
        var serializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        var jObject = JObject.FromObject(data, serializer);
        var parser = new FhirJsonParser(DefaultParserSettings);
        var resource = await parser.ParseAsync<T>(jObject.ToString());
        return resource;
    }

    /// <summary>
    /// Parses a <see cref="JObject"/> object into a <see cref="Resource"/> that can be used.
    /// </summary>
    /// <param name="jObject">The object to parse. It should have the shape of a resource, e.g., a patient.</param>
    /// <typeparam name="T">The resource type to convert into.</typeparam>
    /// <returns>The converted object</returns>
    /// <exception cref="FormatException">If the object is malformed.</exception>
    public static async Task<T> ParseResourceAsync<T>(JObject jObject) where T : Resource
    {
        var parser = new FhirJsonParser(DefaultParserSettings);
        var resource = await parser.ParseAsync<T>(jObject.ToString());
        return resource;
    }
}