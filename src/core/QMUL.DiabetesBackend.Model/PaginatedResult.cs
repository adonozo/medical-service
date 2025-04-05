namespace QMUL.DiabetesBackend.Model;

using System;
using System.Collections.Generic;

[Obsolete("Results should be an enumerable")]
public class PaginatedResult<T>
{
    public long TotalResults { get; set; }

    public long RemainingCount { get; set; }

    public string LastDataCursor { get; set; }

    public T Results { get; set; }
}

// TODO Rename to PaginatedResult once the above can be removed
public class PaginatedResults<TObject>
{
    public long TotalResults { get; set; }

    public long RemainingCount { get; set; }

    public string LastDataCursor { get; set; }

    public IEnumerable<TObject> Results { get; set; }

    public static PaginatedResults<TObject> Empty => new() { Results = Array.Empty<TObject>() };
}