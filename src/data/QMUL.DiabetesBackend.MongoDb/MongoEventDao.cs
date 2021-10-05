namespace QMUL.DiabetesBackend.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Model;
    using Model.Enums;
    using Models;
    using MongoDB.Driver;
    using Utils;

    /// <summary>
    /// The Mongo Event Dao
    /// </summary>
    public class MongoEventDao : MongoDaoBase, IEventDao
    {
        private readonly IMongoCollection<MongoEvent> eventCollection;
        private const string CollectionName = "healthEvent";
        private const int DefaultLimit = 3;

        public MongoEventDao(IDatabaseSettings settings) : base(settings)
        {
            this.eventCollection = this.Database.GetCollection<MongoEvent>(CollectionName);
        }

        /// <inheritdoc />
        public async Task<bool> CreateEvents(IEnumerable<HealthEvent> events)
        {
            try
            {
                var mongoEvents = events.Select(Mapper.ToMongoEvent);
                await this.eventCollection.InsertManyAsync(mongoEvents);
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteEventSeries(string referenceId)
        {
            var result = await
                this.eventCollection.DeleteManyAsync(request => request.Resource.EventReferenceId == referenceId);
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateEventsTiming(string patientId, CustomEventTiming timing, DateTime time)
        {
            var currentTime = DateTime.UtcNow;
            var setTime = new Func<DateTime, DateTime>(oldTime => oldTime.Date.AddHours(time.Hour).AddMinutes(time.Minute));
            
            var eventsToUpdate = await this.eventCollection.FindAsync(healthEvent => healthEvent.PatientId == patientId
                                                                && healthEvent.EventTiming == timing
                                                                && healthEvent.EventDateTime > currentTime);

            await eventsToUpdate.ForEachAsync(async healthEvent =>
            {
                healthEvent.EventDateTime = setTime(healthEvent.EventDateTime);
                var updateResult = await this.eventCollection.UpdateOneAsync(item => item.Id == healthEvent.Id,
                    Builders<MongoEvent>.Update.Set(item => item.EventDateTime, healthEvent.EventDateTime));
                if (!updateResult.IsAcknowledged)
                {
                    throw new ArgumentException("Could not update the healthEvent", nameof(timing));
                }
            });
            
            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start, DateTime end)
        {
            var result = this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                  && healthEvent.Resource.EventType == type
                                                                  && healthEvent.EventDateTime > start 
                                                                  && healthEvent.EventDateTime < end)
                .Project(mongoEvent => mongoEvent.ToHealthEvent());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start,
            DateTime end, CustomEventTiming[] timings)
        {
            var timingFilter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.EventTiming, timings);
            timingFilter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                  && healthEvent.Resource.EventType == type
                                                                  && healthEvent.EventDateTime > start
                                                                  && healthEvent.EventDateTime < end)
                .Filter;
            var result = this.eventCollection.Find(timingFilter).Project(mongoEvent => mongoEvent.ToHealthEvent());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start, DateTime end)
        {
            var filter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.Resource.EventType, types);
            filter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                               && healthEvent.EventDateTime > start
                                                               && healthEvent.EventDateTime < end)
                .Filter;
            var result = this.eventCollection.Find(filter).Project(mongoEvent => mongoEvent.ToHealthEvent());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType[] types, DateTime start,
            DateTime end, CustomEventTiming[] timings)
        {
            var filter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.EventTiming, timings);
            filter &= Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.Resource.EventType, types);
            filter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                     && healthEvent.EventDateTime > start
                                                                     && healthEvent.EventDateTime < end)
                .Filter;
            var result = this.eventCollection.Find(filter).Project(mongoEvent => mongoEvent.ToHealthEvent());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType type)
        {
            var date = DateTime.UtcNow;
            var sort = Builders<MongoEvent>.Sort.Ascending(healthEvent => healthEvent.EventDateTime);
            var result = this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                  && healthEvent.Resource.EventType == type
                                                                  && healthEvent.EventDateTime > date )
                .Sort(sort)
                .Limit(DefaultLimit)
                .Project(mongoEvent => mongoEvent.ToHealthEvent());
            return await result.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<HealthEvent>> GetNextEvents(string patientId, EventType[] types)
        {
            var date = DateTime.UtcNow;
            var sort = Builders<MongoEvent>.Sort.Ascending(healthEvent => healthEvent.EventDateTime);
            var filter = Builders<MongoEvent>.Filter.In(healthEvent => healthEvent.Resource.EventType, types);
            filter &= this.eventCollection.Find(healthEvent => healthEvent.PatientId == patientId
                                                                  && healthEvent.EventDateTime > date ).Filter;
            var result = this.eventCollection.Find(filter)
                .Sort(sort)
                .Limit(DefaultLimit)
                .Project(mongoEvent => mongoEvent.ToHealthEvent());
            return await result.ToListAsync();
        }
    }
}