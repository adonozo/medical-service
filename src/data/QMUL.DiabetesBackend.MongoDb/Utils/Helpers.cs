namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    using System;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Model.Constants;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public static class Helpers
    {
        /// <summary>
        /// Converts a <see cref="Resource"/> object to a <see cref="BsonDocument"/>. The resource is first serialized
        /// to JSON.
        /// </summary>
        /// <param name="resource">The <see cref="Resource"/> to convert.</param>
        /// <returns>A <see cref="BsonDocument"/> parsed from the resource's JSON string.</returns>
        public static async Task<BsonDocument> ToBsonDocumentAsync(Resource resource)
        {
            var json = await resource.ToJsonAsync();
            var bson = BsonDocument.Parse(json);

            return bson;
        }

        /// <summary>
        /// Gets a <see cref="FilterDefinition{TDocument}"/> with an "eq" operator for the ID.
        /// </summary>
        /// <param name="id">The string ID to look for. This should be a <see cref="ObjectId"/> string equivalent.</param>
        /// <returns>The ID's "eq" filter definition.</returns>
        public static FilterDefinition<BsonDocument> GetByIdFilter(string id)
        {
            return Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
        }

        /// <summary>
        /// Gets a <see cref="FilterDefinition{TDocument}"/> to equal a subject reference to a patient ID.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <returns>The subject reference filter definition.</returns>
        public static FilterDefinition<BsonDocument> GetPatientReferenceFilter(string patientId)
        {
            var patientReference = Constants.PatientPath + patientId;
            return Builders<BsonDocument>.Filter.Eq("subject.reference", patientReference);
        }

        /// <summary>
        /// Sets a <see cref="BsonDocument"/> field as a <see cref="BsonDateTime"/> date from a <see cref="DateTimeOffset"/>.
        /// If the dateTimeOffset is null, the document field will be removed.
        /// </summary>
        /// <param name="document">The <see cref="BsonDocument"/> to modify.</param>
        /// <param name="field">The document's field name.</param>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to use.</param>
        public static void SetBsonDateTimeValue(BsonDocument document, string field, DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset == null)
            {
                document.Remove(field);
                return;
            }

            var dateTime = ((DateTimeOffset)dateTimeOffset).UtcDateTime;
            document[field] = new BsonDateTime(dateTime);
        }

        /// <summary>
        /// Converts a <see cref="BsonDocument"/> into a <see cref="Resource"/> object. It uses JSON to perform parsing.
        /// </summary>
        /// <param name="document">The <see cref="BsonDocument"/> to convert.</param>
        /// <typeparam name="T">The resource type. Must be a <see cref="Resource"/> child.</typeparam>
        /// <returns>The converted object.</returns>
        public static async Task<T> ToResourceAsync<T>(BsonDocument document) where T : Resource
        {
            var id = document["_id"].ToString();
            document.Remove("_id");

            var parser = new FhirJsonParser(new ParserSettings
                {AllowUnrecognizedEnums = true, AcceptUnknownMembers = true, PermissiveParsing = true});
            var resource = await parser.ParseAsync<T>(document.ToJson());
            resource.Id = id;

            return resource;
        }

        /// <summary>
        /// Converts an object to an instance of <see cref="DataType"/>, using JSON underneath.
        /// </summary>
        /// <param name="data">The object to convert.</param>
        /// <typeparam name="T">The data type to convert into.</typeparam>
        /// <returns>The converted object, or null if the conversion was not successful.</returns>
        public static async Task<T> ToDataTypeAsync<T>(object data) where T : DataType
        {
            var serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var jObject = JObject.FromObject(data, serializer);
            var parser = new FhirJsonParser(new ParserSettings
                { AllowUnrecognizedEnums = true, AcceptUnknownMembers = false, PermissiveParsing = false });
            var resource = await parser.ParseAsync<T>(jObject.ToString());
            return resource;
        }

        /// <summary>
        /// Gets the filter to use with keyset pagination if the last data cursor is not empty.
        /// </summary>
        /// <param name="searchFilters">The filters search filters.</param>
        /// <param name="lastDataCursor">The last ID obtained from previous pagination results.</param>
        /// <returns>The pagination filter.</returns>
        public static FilterDefinition<BsonDocument> GetPaginationFilter(FilterDefinition<BsonDocument> searchFilters,
            string lastDataCursor)
        {
            if (!ObjectId.TryParse(lastDataCursor, out var lastId))
            {
                return searchFilters;
            }

            return Builders<BsonDocument>.Filter.And(searchFilters,
                Builders<BsonDocument>.Filter.Gt("_id", lastId));
        }

        /// <summary>
        /// Gets the total of results given a find definition.
        /// </summary>
        /// <param name="cursor">The find definition which should include any filters.</param>
        /// <returns>The total number of results.</returns>
        public static async Task<long> GetTotalCount(IFindFluent<BsonDocument, BsonDocument> cursor)
        {
            return await cursor.CountDocumentsAsync();
        }
    }
}