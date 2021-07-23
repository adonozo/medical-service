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
    public class MongoEventDao : BaseMongoDao, IEventDao
    {
        private readonly IMongoCollection<MongoHealthEvent> eventCollection;
        private const string CollectionName = "healthEvent";
    
        public MongoEventDao(IDatabaseSettings settings) : base(settings)
        {
            this.eventCollection = this.Database.GetCollection<MongoHealthEvent>(CollectionName);
        }

        public async Task<bool> CreateEvents(IEnumerable<HealthEvent> events)
        {
            var mongoEvents = events.Select(item => item.ToMongoHealthEvent());
            try
            {
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
            var mongoEvent = healthEvent.ToMongoHealthEvent();
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
            return result.ToList();
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime, int offset)
        {
            var startDate = dateTime.Date.AddMinutes(offset * -1);
            var endDate = dateTime.Date.AddMinutes(offset);
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && timeCompare(healthEvent.EventDateTime));
            return result.ToList();
        }

        public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, DateTime dateTime, CustomEventTiming time)
        {
            var startDate = dateTime.Date;
            var endDate = dateTime.Date;
            var timeCompare = new Func<DateTime, bool>(date => date > startDate && date < endDate);
            var result = await this.eventCollection.FindAsync(healthEvent => healthEvent.Id == patientId
                                                                             && healthEvent.Resource.EventTiming == time
                                                                             && timeCompare(healthEvent.EventDateTime));
            return result.ToList();
        }
    }
}