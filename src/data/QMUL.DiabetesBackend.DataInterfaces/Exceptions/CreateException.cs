namespace QMUL.DiabetesBackend.DataInterfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class CreateException : DataExceptionBase
    {
        public CreateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CreateException(string message) : base(message)
        {
        }

        public CreateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}