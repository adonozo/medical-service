namespace QMUL.DiabetesBackend.Integration.Tests.Utils;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

    public static async Task<HttpResponseMessage> PostJson<T>(this HttpClient client, string uri, T body) where T : class
    {
        var requestContent = ObjectToJsonContent(body);
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

    public static async Task<T> Parse<T>(this HttpContent content)
    {
        return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync(), DefaultSerializer);
    }

    private static async Task<StringContent> ResourceToJsonContent(Resource resource)
    {
        var json = await resource.ToJsonAsync();
        return DefaultStringContent(json);
    }

    private static StringContent ObjectToJsonContent<T>(T @object) where T : class
    {
        var objectString = JsonSerializer.Serialize(@object, DefaultSerializer);
        return DefaultStringContent(objectString);
    }

    private static StringContent DefaultStringContent(string content) =>
        new(content, Encoding.UTF8, "application/json");
}