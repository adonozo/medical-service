namespace QMUL.DiabetesBackend.Model;

/// <summary>
/// A Result class, useful to store a successful operation, or the cause of their errors
/// </summary>
/// <typeparam name="TSuccess">The type of a successful operation</typeparam>
/// <typeparam name="TError">The type of the error cause</typeparam>
public class Result<TSuccess, TError>
{
    private Result(TSuccess results)
    {
        this.Results = results;
        this.IsSuccess = true;
    }

    private Result(TError error)
    {
        this.Error = error;
        this.IsSuccess = false;
    }

    /// <summary>
    /// Creates a successful Result object
    /// </summary>
    /// <param name="results">The object from the successful operation</param>
    /// <returns>A Result instance with a {TSuccess} object</returns>
    public static Result<TSuccess, TError> Success(TSuccess results) => new (results);

    /// <summary>
    /// Creates a failed result with an object to indicate the error cause
    /// </summary>
    /// <param name="error">An object to indicate the error cause</param>
    /// <returns>A Result instance with a {TError} cause</returns>
    public static Result<TSuccess, TError> Fail(TError error) => new(error);

    public TSuccess Results { get; }

    public TError Error { get; }

    public bool IsSuccess { get; }
}
