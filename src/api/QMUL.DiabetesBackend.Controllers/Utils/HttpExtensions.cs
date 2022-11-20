namespace QMUL.DiabetesBackend.Api.Utils
{
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Model;

    public static class HttpExtensions
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

        public static IActionResult OkOrNotFound<T>(this ControllerBase controller, T? result) where T: Resource
        {
            if (result is null)
            {
                return controller.NotFound();
            }

            return controller.Ok(result.ToJObject());
        }
    }
}