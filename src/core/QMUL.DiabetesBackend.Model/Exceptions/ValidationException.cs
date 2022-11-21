namespace QMUL.DiabetesBackend.Model.Exceptions;

using System;
using System.Collections.Generic;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public Dictionary<string, List<string>> ValidationErrors { get; set; }
}