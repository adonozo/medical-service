namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Newtonsoft.Json.Linq;

    public static class ValidationHelpers
    {
        /// <summary>
        /// Extracts errors from a <see cref="ValidationResult"/> into a dictionary where the keys are property names,
        /// and the values, the validation errors.
        /// </summary>
        /// <param name="validationResult">The <see cref="ValidationResult"/> from a failed object validation.</param>
        /// <returns>A dictionary with the validation errors.</returns>
        public static Dictionary<string, List<string>> GetErrorsDictionary(this ValidationResult validationResult)
        {
            var errors = new Dictionary<string, List<string>>();
            foreach (var error in validationResult.Errors)
            {
                if (errors.TryGetValue(error.PropertyName, out var message))
                {
                    message.Add(error.ErrorMessage);
                }
                else
                {
                    errors[error.PropertyName] = new List<string> { error.ErrorMessage };
                }
            }

            return errors;
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
            var parser = new FhirJsonParser(new ParserSettings
                { AllowUnrecognizedEnums = true, AcceptUnknownMembers = false, PermissiveParsing = false });
            var resource = await parser.ParseAsync<T>(jObject.ToString());
            return resource;
        }
    }
}