namespace QMUL.DiabetesBackend.Service.Validators;

using FluentValidation;
using Utils;
using ValidationException = Model.Exceptions.ValidationException;

public abstract class ValidatorBase<T> : AbstractValidator<T>
{
    // make other validators inherit this
    public void ValidateResource(T resource)
    {
        var validationResult = this.Validate(resource);
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