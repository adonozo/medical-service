namespace QMUL.DiabetesBackend.DataInterfaces.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The base class for Data layer exceptions.
    /// </summary>
    public abstract class DataExceptionBase : Exception
    {
        protected DataExceptionBase(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected DataExceptionBase(string message) : base(message)
        {
        }

        protected DataExceptionBase(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}