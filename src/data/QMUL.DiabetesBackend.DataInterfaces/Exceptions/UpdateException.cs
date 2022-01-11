namespace QMUL.DiabetesBackend.DataInterfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class UpdateException : DataExceptionBase
    {
        public UpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UpdateException(string message) : base(message)
        {
        }

        public UpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}