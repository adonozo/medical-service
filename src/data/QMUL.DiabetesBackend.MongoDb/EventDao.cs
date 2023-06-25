namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
using Model.Exceptions;
using Models;
using MongoDB.Driver;

/// <summary>
/// The Mongo Event Dao
/// </summary>
public class EventDao : MongoDaoBase, IEventDao
{
    private readonly IMongoCollection<MongoEvent> eventCollection;
    private readonly ILogger<EventDao> logger;
    private readonly IMapper mapper;

    private const string CollectionName = "healthEvent";
    private const int DefaultLimit = 3;

    public EventDao(IMongoDatabase database, IMapper mapper, ILogger<EventDao> logger) : base(database)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.eventCollection = this.Database.GetCollection<MongoEvent>(CollectionName);
    }

    /// <inheritdoc />
    public async Task<bool> CreateEvents(List<HealthEvent> events)
    {
        if (events.Count == 0)
        {
            this.logger.LogDebug("No health events added");
            return true;
        }

        this.logger.LogDebug("Creating health events");
        var mongoEvents = events.Select(this.mapper.Map<MongoEvent>).ToArray();

        await this.eventCollection.InsertManyAsync(mongoEvents);
        this.logger.LogDebug("Created {Count} events", mongoEvents.Length);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAllRelatedResources(string[] resourceIds)
    {
        if (resourceIds.Length == 0)
        {
            return true;
        }

        var filter = Builders<MongoEvent>.Filter.In(@event => @event.ResourceReference.DomainResourceId, resourceIds);
        var result = await this.eventCollection.DeleteManyAsync(filter);
        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteRelatedEvents(string resourceId)
    {
        this.logger.LogDebug("Deleting events with a resource ID: {Id}", resourceId);
        var result = await this.eventCollection.DeleteManyAsync(request =>
            request.ResourceReference.DomainResourceId == resourceId);
        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteEventSeries(string referenceId)
    {
        this.logger.LogDebug("Deleting events with a reference ID: {Id}", referenceId);
        var result = await this.eventCollection.DeleteManyAsync(request =>
            request.ResourceReference.EventReferenceId == referenceId);
        return result.IsAcknowledged;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateEventsTiming(string patientId, CustomEventTiming timing, DateTimeOffset time)
    {
        this.logger.LogDebug("Updating event timing for patient: {PatientId}", patientId);
        var currentTime = DateTime.UtcNow;
        var setTime = new Func<DateTime, DateTime>(oldTime =>
            oldTime.Date.AddHours(time.Hour).AddMinutes(time.Minute));
        var eventsToUpdate = await this.eventCollection.FindAsync(healthEvent =>
            healthEvent.PatientId == patientId
            && healthEvent.EventTiming == timing
            && healthEvent.EventDateTime > currentTime);

        using var session = await this.Database.Client.StartSessionAsync();
        session.StartTransaction();
        await eventsToUpdate.ForEachAsync(async healthEvent =>
        {
            healthEvent.EventDateTime = setTime(healthEvent.EventDateTime);
            var updateResult = await this.eventCollection.UpdateOneAsync(session,
                item => item.Id == healthEvent.Id,
                Builders<MongoEvent>.Update.Set(item => item.EventDateTime, healthEvent.EventDateTime));
            var errorMessage = $"Could not update the healthEvent with ID {healthEvent.Id}";
            this.CheckAcknowledgedOrThrow(updateResult.IsAcknowledged, new WriteResourceException(errorMessage),
                () => this.logger.LogWarning("{ErrorMessage}", errorMessage));
        });
        await session.CommitTransactionAsync();

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HealthEvent>> GetEvents(string patientId, EventType type, DateTime start,
        DateTime end)
    {
        var result = this.eventCollection.Find(healthEvent =>
                healthEvent.PatientId == patientId
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
        timingFilter &= this.eventCollection.Find(healthEvent =>
                healthEvent.PatientId == patientId
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
        filter &= this.eventCollection.Find(healthEvent =>
                healthEvent.PatientId == patientId
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
        filter &= this.eventCollection.Find(healthEvent =>
                healthEvent.PatientId == patientId
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
        var result = this.eventCollection.Find(healthEvent =>
                healthEvent.PatientId == patientId
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
        filter &= this.eventCollection.Find(healthEvent =>
            healthEvent.PatientId == patientId
            && healthEvent.EventDateTime > date).Filter;
        var result = this.eventCollection.Find(filter)
            .Sort(sort)
            .Limit(DefaultLimit)
            .Project(mongoEvent => this.mapper.Map<HealthEvent>(mongoEvent));
        return await result.ToListAsync();
    }
}