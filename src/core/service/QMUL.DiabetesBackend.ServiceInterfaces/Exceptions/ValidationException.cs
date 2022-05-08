namespace QMUL.DiabetesBackend.ServiceInterfaces.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class ValidationException : ServiceExceptionBase
    {
        public ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public Dictionary<string, List<string>> ValidationErrors { get; set; }
    }
}