using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QMUL.DiabetesBackend.Controllers.Tests")]

namespace QMUL.DiabetesBackend.Api.Utils
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ServiceInterfaces.Exceptions;

    internal static class ExceptionHandler
    {
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
            catch (CreateException)
            {
                return context.BadRequest();
            }
            catch (UpdateException)
            {
                return context.BadRequest();
            }
            catch (ArgumentException)
            {
                return context.BadRequest();
            }
            catch (Exception e)
            {
                logger.LogError(e, "There was an error while processing the request");
                return context.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}