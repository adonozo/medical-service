namespace QMUL.DiabetesBackend.Service.Validators;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Hl7.Fhir.Model;
using Model.Utils;
using Newtonsoft.Json.Linq;
using ServiceInterfaces.Validators;
using Utils;
using Task = System.Threading.Tasks.Task;
using ValidationException = Model.Exceptions.ValidationException;

public abstract class ResourceValidatorBase<T> : AbstractValidator<T>, IResourceValidator<T> where T : Resource
{
    /// <inheritdoc/>
    public virtual async Task<T> ParseAndValidateAsync(JObject requestObject)
    {
        var resource = await this.ParseObjectAsync(requestObject);
        await this.ValidateResource(resource);

        return resource;
    }

    private async Task<T> ParseObjectAsync(JObject jObject)
    {
        try
        {
            return await Converter.ParseResourceAsync<T>(jObject);
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

    private async Task ValidateResource(T resource)
    {
        var validationResult = await this.ValidateAsync(resource);
        if (validationResult.IsValid)
        {
            return;
        }

        throw new ValidationException("Resource is invalid")
        {
            ValidationErrors = validationResult.GetErrorsDictionary()
        };
    }
}