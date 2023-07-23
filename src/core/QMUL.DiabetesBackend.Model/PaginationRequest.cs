namespace QMUL.DiabetesBackend.Model;

public class PaginationRequest
{
    private const int DefaultLimit = 20;

    public PaginationRequest(int? limit, string lastCursorId)
    {
        this.Limit = limit ?? DefaultLimit;
        this.LastCursorId = lastCursorId;
    }

    public static PaginationRequest FirstPaginatedResults => new(DefaultLimit, null);

    public int Limit { get; }

    public string LastCursorId { get; }
}