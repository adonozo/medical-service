namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Threading.Tasks;
    using DataInterfaces.Exceptions;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Driver;

    /// <summary>
    /// The Mongo Dao Base class. All Mongo classes must inherit from this class.
    /// </summary>
    public abstract class MongoDaoBase
    {
        /// <summary>
        /// A reference to the database.
        /// </summary>
        protected readonly IMongoDatabase Database;

        /// <summary>
        /// Set the database connection. Gets the connection string as an argument.
        /// </summary>
        /// <param name="database">A set up <see cref="IMongoDatabase"/> instance.</param>
        protected MongoDaoBase(IMongoDatabase database)
        {
            this.Database = database;
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("camelCase", conventionPack, _ => true);
        }

        /// <summary>
        /// Gets a single document from the database. If the result is not found (null), it throws an exception.
        /// </summary>
        /// <param name="find">The <see cref="IFindFluent{TDocument,TProjection}"/> command to get the document.</param>
        /// <param name="exception">A <see cref="Exception"/> to throw if the document is not found.</param>
        /// <param name="fallback">An <see cref="Action"/> to execute before throwing the exception.</param>
        /// <typeparam name="TDocument">The Mongo Document type to look in the database</typeparam>
        /// <typeparam name="TProjection">The projected type to return.</typeparam>
        /// <returns>A document found in the database.</returns>
        /// <exception cref="Exception">Thrown if the result is not found.</exception>
        protected async Task<TProjection> GetSingleOrThrow<TDocument, TProjection>(
            IFindFluent<TDocument, TProjection> find, Exception exception, Action fallback = null)
        {
            var result = await find.FirstOrDefaultAsync();
            if (result != null)
            {
                return result;
            }

            fallback?.Invoke();
            throw exception;
        }

        /// <summary>
        /// Checks if a result was acknowledged. It throws the exception if the result is false. Useful for update or
        /// delete operations.
        /// </summary>
        /// <param name="result">A boolean value that matches an acknowledged result.</param>
        /// <param name="exception">A <see cref="Exception"/> exception to throw if the result is false.</param>
        /// <param name="fallback">An <see cref="Action"/> to execute before throwing the exception.</param>
        /// <exception cref="Exception">Thrown if the result is false.</exception>
        protected void CheckAcknowledgedOrThrow(bool result, Exception exception, Action fallback = null)
        {
            if (result)
            {
                return;
            }

            fallback?.Invoke();
            throw exception;
        }
    }
}