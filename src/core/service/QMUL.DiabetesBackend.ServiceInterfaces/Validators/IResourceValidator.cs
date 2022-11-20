namespace QMUL.DiabetesBackend.ServiceInterfaces.Validators;

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Model.Exceptions;
using Newtonsoft.Json.Linq;

public interface IResourceValidator<T> where T : Resource
{
    /// <summary>
    /// Parses a <see cref="JObject"/> object into a given <see cref="Resource"/> object type. If the object is parsed
    /// correctly, the object is validated against custom rules.
    /// </summary>
    /// <param name="requestObject">The <see cref="JObject"/> to parse.</param>
    /// <returns>A parsed resource object.</returns>
    /// <exception cref="ValidationException">If the object could not be parsed or is invalid.</exception>
    Task<T> ParseAndValidateAsync(JObject requestObject);
}