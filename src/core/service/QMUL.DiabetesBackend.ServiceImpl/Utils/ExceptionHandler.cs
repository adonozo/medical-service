using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("QMUL.DiabetesBackend.ServiceImpl.Tests")]
namespace QMUL.DiabetesBackend.ServiceImpl.Utils
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using ServiceInterfaces.Exceptions;

    /// <summary>
    /// This static class has methods for handling exceptions.
    /// </summary>
    internal static class ExceptionHandler
    {
        /// <summary>
        /// Tries to execute the async <see cref="Func{TResult}"/>. If it throws an error, it will be caught,
        /// converted into a Service Exception, and logged. Mostly used when calling data methods.
        /// </summary>
        /// <param name="method">The async <see cref="Func{TResult}"/> to execute</param>
        /// <param name="logger">The logger instance.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> with the method's result.</returns>
        /// <exception cref="ServiceExceptionBase">If an exception is handled.</exception>
        /// <exception cref="Exception">If the exception was not expected.</exception>
        public static async Task<T> ExecuteAndHandleAsync<T>(Func<Task<T>> method, ILogger logger)
        {
            try
            {
                return await method.Invoke();
            }
            catch (DataInterfaces.Exceptions.CreateException e)
            {
                logger.LogWarning("Error creating resource");
                throw new CreateException(e.Message, e);
            }
            catch (DataInterfaces.Exceptions.UpdateException e)
            {
                logger.LogWarning("Error updating resource");
                throw new UpdateException(e.Message, e);
            }
            catch (DataInterfaces.Exceptions.NotFoundException e)
            {
                logger.LogDebug("The resource was not found");
                throw new NotFoundException(e.Message, e);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unexpected exception");
                throw;
            }
        } 
    }
}