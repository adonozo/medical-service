using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.MongoDb.Models;
using QMUL.DiabetesBackend.MongoDb.Utils;

namespace QMUL.DiabetesBackend.MongoDb
{
    public class MongoEventDao : MongoDaoBase, IEventDao
    {
        private readonly IMongoCollection<MongoEvent> eventCollection;
        private const string CollectionName = "healthEvent";
    
        public MongoEventDao(IDatabaseSettings settings) : base(settings)
        {
            this.eventCollection = this.Database.GetCollection<MongoEvent>(CollectionName);
        }

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

        public async Task<bool> UpdateEvent(string eventId, HealthEvent healthEvent)
        {
            var mongoEvent = healthEvent.ToMongoEvent();
            var result = await this.eventCollection.ReplaceOneAsync(item => item.Id == eventId, mongoEvent);
            if (!result.IsAcknowledged)
            {
                throw new InvalidOperationException($"There was an error updating the event {eventId}");
            }

            var cursor = await this.eventCollection.FindAsync(item => item.Id == eventId);
            var updatedEvent = await cursor.FirstOrDefaultAsync();
            return updatedEvent != null;
        }

        public async Task<bool> DeleteEventSeries(string referenceId)
        {
            var result = await
                this.eventCollection.DeleteManyAsync(request => request.Resource.EventReferenceId == referenceId);
            return result.IsAcknowledged;
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string referenceId)
        {
            var result =
                await this.eventCollection.FindAsync(healthEvent => healthEvent.Resource.EventReferenceId == referenceId);
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime, int offset)
        {
            var startDate = dateTime.Date.AddMinutes(offset * -1);
            var endDate = dateTime.Date.AddMinutes(offset);
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && healthEvent.ExactTimeIsSetup
                                                                             && timeCompare(healthEvent.EventDateTime));
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime dateTime,
            int offset)
        {
            var startDate = dateTime.Date.AddMinutes(offset * -1);
            var endDate = dateTime.Date.AddMinutes(offset);
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && healthEvent.Resource.EventType == type
                                                                             && healthEvent.ExactTimeIsSetup
                                                                             && timeCompare(healthEvent.EventDateTime));
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime,
            CustomEventTiming time)
        {
            var startDate = dateTime.Date;
            var endDate = dateTime.Date;
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && healthEvent.EventTiming == time
                                                                             && timeCompare(healthEvent.EventDateTime));
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime dateTime,
            CustomEventTiming time)
        {
            var startDate = dateTime.Date;
            var endDate = dateTime.Date;
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && healthEvent.Resource.EventType == type
                                                                             && healthEvent.EventTiming == time
                                                                             && timeCompare(healthEvent.EventDateTime));
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime)
        {
            var startDate = dateTime.Date;
            var endDate = dateTime.Date;
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && timeCompare(healthEvent.EventDateTime));
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime dateTime)
        {
            var startDate = dateTime.Date;
            var endDate = dateTime.Date;
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && healthEvent.Resource.EventType == type 
                                                                             && timeCompare(healthEvent.EventDateTime));
            var events = await result.ToListAsync();
            return events.Select(Mapper.ToHealthEvent);
        }

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
    }
}