namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
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