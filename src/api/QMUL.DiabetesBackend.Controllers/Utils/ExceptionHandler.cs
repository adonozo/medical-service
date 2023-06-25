using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QMUL.DiabetesBackend.Controllers.Tests")]

namespace QMUL.DiabetesBackend.Controllers.Utils;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.Exceptions;

internal static class ExceptionHandler
{
    /// <summary>
    /// Executes an async method and handles any exception that it may throw as an HTTP response
    /// </summary>
    /// <param name="method">The async method to get a result from</param>
    /// <param name="logger">A <see cref="Logger{T}"/> to log errors</param>
    /// <param name="context">The <see cref="ControllerBase"/> context for returning HTTP results</param>
    /// <typeparam name="T">An HTTP <see cref="IActionResult"/></typeparam>
    /// <returns>An <see cref="IActionResult"/> response</returns>
    public static async Task<IActionResult> ExecuteAndHandleAsync<T>(Func<Task<T>> method, ILogger logger,
        ControllerBase context) where T : IActionResult
    {
        try
        {
            return await method.Invoke();
        }
        catch (NotFoundException)
        {
            return context.NotFound();
        }
        catch (WriteResourceException)
        {
            return context.StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (ArgumentException)
        {
            return context.BadRequest();
        }
        catch (ValidationException exception)
        {
            logger.LogWarning(exception, "Validation Exception");
            return context.BadRequest(exception.ValidationErrors);
        }
        catch (Exception e)
        {
            logger.LogError(e, "There was an error while processing the request");
            return context.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}