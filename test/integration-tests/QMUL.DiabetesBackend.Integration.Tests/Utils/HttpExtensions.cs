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
        var requestContent = await ResourceToJsonContent(resource);
        return await client.PostAsync(uri, requestContent);
    }

    public static async Task<HttpResponseMessage> PutResource(this HttpClient client, string uri, Resource resource)
    {
        var requestContent = await ResourceToJsonContent(resource);
        return await client.PutAsync(uri, requestContent);
    }

    public static async Task<HttpResponseMessage> Patch<T>(this HttpClient client, string uri, T @object)
    {
        var json = JsonSerializer.Serialize(@object, DefaultSerializer);
        var content = DefaultStringContent(json);
        return await client.PatchAsync(uri, content);
    }

    public static async Task<HttpResponseMessage> PutEmpty(this HttpClient client, string uri)
    {
        return await client.PutAsync(uri, DefaultStringContent(string.Empty));
    }

    private static async Task<StringContent> ResourceToJsonContent(Resource resource)
    {
        var json = await resource.ToJsonAsync();
        return DefaultStringContent(json);
    }

    private static StringContent DefaultStringContent(string content) =>
        new(content, Encoding.UTF8, "application/json");
}