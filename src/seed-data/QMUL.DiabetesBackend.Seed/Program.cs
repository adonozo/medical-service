namespace QMUL.DiabetesBackend.Seed;

using System.Text;
using System.Text.Json;
using SeedData.observations;

class Program
{
    private const string SeedUri = "http://localhost:5000/";
    private const string SeedEndpoint = "observation-templates";

    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(SeedUri);

        var templates = HemogramTemplateData.ObservationTemplates;
        foreach (var template in templates)
        {
            var result = await PutJson(httpClient, SeedEndpoint, template);
            if (!result.IsSuccessStatusCode)
            {
                Console.WriteLine("Couldn't insert template");
                break;
            }

            Console.WriteLine($"Created: {template.Code.Coding.Code} - {template.Code.Coding.Display}");
        }

        Console.WriteLine("Done");
    }

    private static JsonSerializerOptions DefaultSerializer = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static async Task<HttpResponseMessage> PutJson<T>(HttpClient client, string uri, T body) where T : class
    {
        var requestContent = ObjectToJsonContent(body);
        return await client.PutAsync(uri, requestContent);
    }

    private static StringContent ObjectToJsonContent<T>(T @object) where T : class
    {
        var objectString = JsonSerializer.Serialize(@object, DefaultSerializer);
        return DefaultStringContent(objectString);
    }

    private static StringContent DefaultStringContent(string content) =>
        new(content, Encoding.UTF8, "application/json");
}