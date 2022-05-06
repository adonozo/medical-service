namespace QMUL.DiabetesBackend.Api.Utils
{
    using System;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Http;
    using Model;
    using Newtonsoft.Json.Linq;

    public static class Helpers
    {
        /// <summary>
        /// Parses a <see cref="JObject"/> object into a <see cref="Resource"/> that can be used.
        /// </summary>
        /// <param name="jObject">The object to parse. It should have the shape of a resource, e.g., a patient.</param>
        /// <typeparam name="T">The resource type to convert into.</typeparam>
        /// <returns>The converted object</returns>
        /// <exception cref="FormatException">If the object is malformed.</exception>
        public static async Task<T> ParseResourceAsync<T>(JObject jObject) where T : Resource
        {
            var parser = new FhirJsonParser(new ParserSettings
                { AllowUnrecognizedEnums = true, AcceptUnknownMembers = false, PermissiveParsing = false });
            var resource = await parser.ParseAsync<T>(jObject.ToString());
            return resource;
        }

        public static void SetPaginatedResult<T>(this HttpContext context, PaginatedResult<T> paginatedResult)
        {
            context.Response.Headers[HttpConstants.LastCursorHeader] = paginatedResult.LastDataCursor;
            context.Response.Headers[HttpConstants.RemainingCountHeader] = paginatedResult.RemainingCount.ToString();
        }
    }
}