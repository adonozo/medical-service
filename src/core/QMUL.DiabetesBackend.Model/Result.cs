namespace QMUL.DiabetesBackend.Model;

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

    public static Result<TSuccess, TError> Success(TSuccess results) => new (results);

    public static Result<TSuccess, TError> Fail(TError error) => new(error);

    public TSuccess Results { get; }

    public TError Error { get; }

    public bool IsSuccess { get; }
}
