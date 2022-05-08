namespace QMUL.DiabetesBackend.Api.Utils
{
    using Microsoft.AspNetCore.Http;
    using Model;

    public static class Helpers
    {
        /// <summary>
        /// Sets the HTTP response headers for a paginated result.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/></param>
        /// <param name="paginatedResult">The <see cref="PaginatedResult{T}"/></param>
        /// <typeparam name="T">The paginated result type.</typeparam>
        public static void SetPaginatedResult<T>(this HttpContext context, PaginatedResult<T> paginatedResult)
        {
            context.Response.Headers[HttpConstants.LastCursorHeader] = paginatedResult.LastDataCursor;
            context.Response.Headers[HttpConstants.RemainingCountHeader] = paginatedResult.RemainingCount.ToString();
        }
    }
}