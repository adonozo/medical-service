namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using DataInterfaces;
    using DataInterfaces.Exceptions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Model.Enums;
    using Models;
    using MongoDB.Driver;

    /// <summary>
    /// The Mongo Event Dao
    /// </summary>
    public class MongoEventDao : MongoDaoBase, IEventDao
    {
        private readonly IMongoCollection<MongoEvent> eventCollection;
        private readonly ILogger<MongoEventDao> logger;
        private readonly IMapper mapper;
        
        private const string CollectionName = "healthEvent";
        private const int DefaultLimit = 3;

        public MongoEventDao(IMongoDatabase database, IMapper mapper, ILogger<MongoEventDao> logger) : base(database)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.eventCollection = this.Database.GetCollection<MongoEvent>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> CreateEvents(IEnumerable<HealthEvent> events)
        {
            this.logger.LogDebug("Creating health events");
            var mongoEvents = events.Select(this.mapper.Map<MongoEvent>).ToArray();
            await this.eventCollection.InsertManyAsync(mongoEvents);
            this.logger.LogDebug("Created {Count} events", mongoEvents.Length);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteRelatedEvents(string resourceId)
        {
            this.logger.LogDebug("Deleting events with a resource ID: {Id}", resourceId);
            var result = await
                this.eventCollection.DeleteManyAsync(request => request.ResourceReference.ResourceId == resourceId);
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteEventSeries(string referenceId)
        {
            this.logger.LogDebug("Deleting events with a reference ID: {Id}", referenceId);
            var result = await
                this.eventCollection.DeleteManyAsync(request => request.ResourceReference.EventReferenceId == referenceId);
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateEventsTiming(string patientId, CustomEventTiming timing, DateTimeOffset time)
        {
            this.logger.LogDebug("Updating event timing for patient: {PatientId}", patientId);
            var currentTime = DateTime.UtcNow;
            var setTime =
                new Func<DateTime, DateTime>(oldTime => oldTime.Date.AddHours(time.Hour).AddMinutes(time.Minute));
            var eventsToUpdate = await this.eventCollection.FindAsync(healthEvent => healthEvent.PatientId == patientId
                && healthEvent.EventTiming == timing
                && healthEvent.EventDateTime > currentTime);

            await eventsToUpdate.ForEachAsync(async healthEvent =>
            {
                healthEvent.EventDateTime = setTime(healthEvent.EventDateTime);
                var updateResult = await this.eventCollection.UpdateOneAsync(item => item.Id == healthEvent.Id,
                    Builders<MongoEvent>.Update.Set(item => item.EventDateTime, healthEvent.EventDateTime));
                var errorMessage = $"Could not update the healthEvent with ID {healthEvent.Id}";
                this.CheckAcknowledgedOrThrow(updateResult.IsAcknowledged, new UpdateException(errorMessage),
                    () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
            });

            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start,
            DateTime end)
        {
            var result = this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                  && healthEvent.ResourceReference.EventType == type
                                                                  && healthEvent.EventDateTime > start
                                                                  && healthEvent.EventDateTime < end)
                .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start,
            DateTime end, CustomEventTiming[] timings)
        {
            var timingFilter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.EventTiming, timings);
            timingFilter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                     && healthEvent.ResourceReference.EventType == type
                                                                     && healthEvent.EventDateTime > start
                                                                     && healthEvent.EventDateTime < end)
                .Filter;
            var result = this.eventCollection.Find(timingFilter)
                .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start,
            DateTime end)
        {
            var filter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.ResourceReference.EventType, types);
            filter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                               && healthEvent.EventDateTime > start
                                                               && healthEvent.EventDateTime < end)
                .Filter;
            var result = this.eventCollection.Find(filter)
                .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start,
            DateTime end, CustomEventTiming[] timings)
        {
            var filter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.EventTiming, timings);
            filter &= Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.ResourceReference.EventType, types);
            filter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                               && healthEvent.EventDateTime > start
                                                               && healthEvent.EventDateTime < end)
                .Filter;
            var result = this.eventCollection.Find(filter)
                .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType type)
        {
            var date = DateTime.UtcNow;
            var sort = Builders<MongoEvent>.Sort.Ascending(healthEvent => healthEvent.EventDateTime);
            var result = this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                  && healthEvent.ResourceReference.EventType == type
                                                                  && healthEvent.EventDateTime > date)
                .Sort(sort)
                .Limit(DefaultLimit)
                .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType[] types)
        {
            var date = DateTime.UtcNow;
            var sort = Builders<MongoEvent>.Sort.Ascending(healthEvent => healthEvent.EventDateTime);
            var filter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.ResourceReference.EventType, types);
            filter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                               && healthEvent.EventDateTime > date).Filter;
            var result = this.eventCollection.Find(filter)
                .Sort(sort)
                .Limit(DefaultLimit)
                .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
            return await result.ToListAsync();
        }
    }
}