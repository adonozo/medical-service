namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System.Collections.Generic;
    using FluentValidation.Results;

    public static class ValidationExtensions
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
    }
}