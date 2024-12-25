namespace QMUL.DiabetesBackend.MongoDb;

using System.Linq;
using System.Threading.Tasks;
using DataInterfaces;
using Model;
using Model.Exceptions;
using Models;
using MongoDB.Driver;
using Utils;

public class ReportDao : MongoDaoBase, IReportDao
{
    private const string CollectionName = "diagnosis-report";

    private readonly IMongoCollection<MongoDiagnosisReport> reportCollection;

    public ReportDao(IMongoDatabase database) : base(database)
    {
        this.reportCollection = this.Database.GetCollection<MongoDiagnosisReport>(CollectionName);
    }

    public async Task<DiagnosisReport?> GetReport(string id)
    {
        var report = await this.reportCollection.Find(
                Helpers.ByIdFilter<MongoDiagnosisReport>(id))
            .FirstOrDefaultAsync();
        return report.ToDiagnosisReport();
    }

    public async Task<PaginatedResults<DiagnosisReport>> SearchReports(PaginationRequest paginationRequest)
    {
        var resultsFilter = Helpers.GetPaginationFilter(Builders<MongoDiagnosisReport>.Filter.Empty,
            paginationRequest.LastCursorId);
        var results = await this.reportCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .ToListAsync();
        var mappedResults = results
            .Where(result => result is not null)
            .Select(result => result.ToDiagnosisReport()!);

        return await Helpers.GetPaginatedResults(this.reportCollection, resultsFilter, mappedResults.ToArray());
    }

    public async Task<DiagnosisReport> InsertReport(DiagnosisReport report)
    {
        var mongoReport = report.ToMongoDiagnosisReport()!;
        await this.reportCollection.InsertOneAsync(mongoReport);

        var savedTemplate = await this.GetReport(
            mongoReport.Id?.ToString()
            ?? throw new WriteResourceException("Could not insert Report"));
        if (savedTemplate is null)
        {
            throw new WriteResourceException("Could not insert Report");
        }

        return mongoReport.ToDiagnosisReport()!;
    }

    public async Task<bool> UpdateReport(DiagnosisReport updatedReport)
    {
        var result = await this.reportCollection.ReplaceOneAsync(
            Helpers.ByIdFilter<MongoDiagnosisReport>(updatedReport.Id),
            updatedReport.ToMongoDiagnosisReport()!);
        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteReport(string id)
    {
        var result = await this.reportCollection.DeleteOneAsync(Helpers.ByIdFilter<MongoDiagnosisReport>(id));
        return result.IsAcknowledged && result.DeletedCount == 1;
    }
}