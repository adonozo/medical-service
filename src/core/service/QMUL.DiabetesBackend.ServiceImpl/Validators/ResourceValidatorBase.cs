namespace QMUL.DiabetesBackend.ServiceImpl.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Hl7.Fhir.Model;
    using Newtonsoft.Json.Linq;
    using ServiceInterfaces.Validators;
    using Utils;
    using ValidationException = ServiceInterfaces.Exceptions.ValidationException;

    public abstract class ResourceValidatorBase<T> : AbstractValidator<T>, IResourceValidator<T> where T : Resource
    {
        /// <inheritdoc/>
        public virtual async Task<T> ParseAndValidateAsync(JObject requestObject)
        {
            var resource = await this.ParseObjectAsync(requestObject);
            this.ValidateResource(resource);

            return resource;
        }

        private async Task<T> ParseObjectAsync(JObject jObject)
        {
            try
            {
                return await ValidationHelpers.ParseResourceAsync<T>(jObject);
            }
            catch (Exception e)
            {
                var errors = new Dictionary<string, List<string>>
                {
                    { "RequestBody", new List<string> { "The request is not a valid FHIR object" } }
                };

                throw new ValidationException($"Could not parse the request to a valid FHIR {nameof(T)}", e)
                {
                    ValidationErrors = errors
                };
            }
        }

        private void ValidateResource(T resource)
        {
            var validationResult = this.Validate(resource);
            if (validationResult.IsValid)
            {
                return;
            }

            throw new ValidationException("Medication is invalid")
            {
                ValidationErrors = validationResult.GetErrorsDictionary()
            };
        }
    }
}