namespace QMUL.DiabetesBackend.ServiceInterfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The base class for service exceptions.
    /// </summary>
    public abstract class ServiceExceptionBase : Exception 
    {
        protected ServiceExceptionBase(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected ServiceExceptionBase(string message) : base(message)
        {
        }

        protected ServiceExceptionBase(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}