namespace QMUL.DiabetesBackend.DataInterfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This exception is thrown when an object was not found in the data storage.
    /// </summary>
    public class NotFoundException : DataExceptionBase
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