namespace QMUL.DiabetesBackend.Integration.Tests.Utils;

using System.Net.Http;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model.Utils;
using Newtonsoft.Json.Linq;

public static class HttpUtils
{
    public static async Task<T> ParseResult<T>(HttpContent content) where T : Resource
    {
        var json = await content.ReadAsStringAsync();
        return await ParseJson<T>(json);
    }

    public static async Task<T> ParseJson<T>(string json) where T : Resource
    {
        var jObject = JObject.Parse(json);
        return await Converter.ParseResourceAsync<T>(jObject);
    }
}