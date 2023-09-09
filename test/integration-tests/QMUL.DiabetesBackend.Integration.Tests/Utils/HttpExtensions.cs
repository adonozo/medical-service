namespace QMUL.DiabetesBackend.Integration.Tests.Utils;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

public static class HttpExtensions
{
    private static readonly JsonSerializerOptions DefaultSerializer;

    static HttpExtensions()
    {
        DefaultSerializer = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        DefaultSerializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    }

    public static async Task<HttpResponseMessage> PostResource(this HttpClient client, string uri, Resource resource)
    {
        var json = await resource.ToJsonAsync();
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(uri, data);
    }

    public static async Task<HttpResponseMessage> Patch<T>(this HttpClient client, string uri, T @object)
    {
        var json = JsonSerializer.Serialize(@object, DefaultSerializer);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PatchAsync(uri, data);
    }
}