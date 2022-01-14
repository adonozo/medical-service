namespace QMUL.DiabetesBackend.ServiceInterfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class NotFoundException : ServiceExceptionBase
    {
        public NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}