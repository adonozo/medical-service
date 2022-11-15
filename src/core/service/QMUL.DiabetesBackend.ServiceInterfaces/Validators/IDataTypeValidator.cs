namespace QMUL.DiabetesBackend.ServiceInterfaces.Validators
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Model;

    public interface IDataTypeValidator
    {
        /// <summary>
        /// Parses an object into a concrete <see cref="DataType"/>. The concrete type is taken from the type parameter.
        /// If the validation or parsing fails, it will throw an exception.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns>A parsed data type object.</returns>
        /// <exception cref="ValidationException">If the object could not be parsed or the data is invalid.</exception>
        Task<DataType> ParseAndValidateAsync(DataTypeWrapper wrapper);
    }
}