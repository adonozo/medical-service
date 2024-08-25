namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model;
using Model.Exceptions;
using Models;
using MongoDB.Driver;
using Utils;

public class ObservationTemplateDao : MongoMultiLingualBase, IObservationTemplateDao
{
    private const string CollectionName = "observation-template";

    private readonly IMongoCollection<MongoObservationTemplate> templateCollection;
    private readonly ILogger<ObservationTemplateDao> logger;

    public ObservationTemplateDao(IMongoDatabase database, ILogger<ObservationTemplateDao> logger) : base(database)
    {
        this.logger = logger;
        this.templateCollection = database.GetCollection<MongoObservationTemplate>(CollectionName);
    }

    public async Task<ObservationTemplate?> GetObservationTemplate(string id)
    {
        var template = await this.templateCollection.Find(Helpers.ByIdFilter<MongoObservationTemplate>(id))
            .FirstOrDefaultAsync();
        return template.ToObservationTemplate();
    }

    public async Task<PaginatedResult<IEnumerable<ObservationTemplate>>> SearchObservationTemplates(
        PaginationRequest paginationRequest,
        string? type = null)
    {
        var searchFilter = string.IsNullOrEmpty(type)
            ? Builders<MongoObservationTemplate>.Filter.Empty
            : Builders<MongoObservationTemplate>.Filter.Eq(template => template.Code.Coding.System, type);

        var resultsFilter =
            Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);
        var results = await this.templateCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .ToListAsync();
        var mappedTemplates = results
            .Where(result => result is not null)
            .Select(result => result.ToObservationTemplate()!);

        return await Helpers.GetPaginatedResult(this.templateCollection, searchFilter, mappedTemplates.ToArray());
    }

    public async Task<ObservationTemplate> CreateObservationTemplate(ObservationTemplate template)
    {
        this.logger.LogDebug("Creating template...");
        if (template is null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        await this.templateCollection.InsertOneAsync(template.ToMongoObservationTemplate()!);

        var savedTemplate = await this.GetObservationTemplate(template.Id);
        if (savedTemplate is null)
        {
            throw new WriteResourceException("Could not create template");
        }

        return savedTemplate;
    }
}