namespace QMUL.DiabetesBackend.Model.Exceptions;

using System;

public class WriteResourceException : Exception
{
    public WriteResourceException(string message) : base(message)
    {
    }
}