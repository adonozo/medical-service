namespace QMUL.DiabetesBackend.Integration.Tests.Utils;

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

public static class HttpExtensions
{
    public static async Task<HttpResponseMessage> PostResource(this HttpClient client, string uri, Resource resource)
    {
        var json = await resource.ToJsonAsync();
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(uri, data);
    }
}