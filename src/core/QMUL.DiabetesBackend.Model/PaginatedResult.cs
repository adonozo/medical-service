namespace QMUL.DiabetesBackend.Model
{
    public class PaginatedResult<T>
    {
        public long TotalResults { get; set; }

        public long RemainingCount { get; set; }

        public string LastDataCursor { get; set; }

        public T Results { get; set; }
    }
}