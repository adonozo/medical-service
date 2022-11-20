namespace QMUL.DiabetesBackend.Service.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Utils;
    using ServiceInterfaces.Validators;
    using Utils;
    using ValidationException = Model.Exceptions.ValidationException;

    public class DataTypeValidator : AbstractValidator<DataTypeWrapper>, IDataTypeValidator
    {
        private readonly HashSet<string> allowedTypes = new()
        {
            nameof(Quantity),
            "string",
            "boolean",
            "integer"
        };

        public DataTypeValidator()
        {
            RuleFor(wrapper => wrapper.Value)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The provided value is empty");

            RuleFor(wrapper => wrapper.Type)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(type => allowedTypes.Contains(type))
                .WithMessage("The type is not: Quantity, string, boolean, or integer");
        }

        /// <inheritdoc/>
        public async Task<DataType> ParseAndValidateAsync(DataTypeWrapper wrapper)
        {
            var validationResult = this.Validate(wrapper);
            if (!validationResult.IsValid)
            {
                throw new ValidationException($"DataType {wrapper.Type} is invalid")
                {
                    ValidationErrors = validationResult.GetErrorsDictionary()
                };
            }

            return await ParseObjectAsync(wrapper);
        }

        private async Task<DataType> ParseObjectAsync(DataTypeWrapper wrapper)
        {
            try
            {
                return wrapper.Type switch
                {
                    nameof(Quantity) => await Converter.ToDataTypeAsync<Quantity>(wrapper.Value),
                    "string" => new FhirString(wrapper.Value as string),
                    "boolean" => new FhirBoolean(wrapper.Value as bool?),
                    "integer" => new Integer(int.Parse(wrapper.Value.ToString() ?? string.Empty)),
                    _ => throw new ValidationException($"DataType {wrapper.Type} is invalid")
                };
            }
            catch (Exception e)
            {
                var errors = new Dictionary<string, List<string>>
                {
                    { "RequestBody", new List<string> { "The value and type provided are not valid" } }
                };

                throw new ValidationException($"Could not parse the request to a valid object.", e)
                {
                    ValidationErrors = errors
                };
            }
        }
    }
}