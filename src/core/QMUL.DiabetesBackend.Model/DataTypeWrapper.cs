namespace QMUL.DiabetesBackend.Model;

public class DataTypeWrapper
{
    /// <summary>
    /// The object to parse into a <see cref="Value"/>.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// The name of the concrete DataType instance.
    /// </summary>
    public string Type { get; set; }
}