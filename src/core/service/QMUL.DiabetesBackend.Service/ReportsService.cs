namespace QMUL.DiabetesBackend.Service;

using System.Threading.Tasks;
using DataInterfaces;
using Model;
using Model.Exceptions;
using ServiceInterfaces;

public class ReportsService : IReportService
{
    private readonly IReportDao reportDao;

    public ReportsService(IReportDao reportDao)
    {
        this.reportDao = reportDao;
    }

    public async Task<DiagnosisReport?> GetReport(string id)
    {
        return await this.reportDao.GetReport(id);
    }

    public async Task<PaginatedResults<DiagnosisReport>> SearchReports(PaginationRequest paginationRequest)
    {
        return await this.reportDao.SearchReports(paginationRequest);
    }

    public async Task<DiagnosisReport> CreateReport(DiagnosisReport report)
    {
        return await this.reportDao.InsertReport(report);
    }

    public async Task<bool> UpdateReport(string id, DiagnosisReport updatedReport)
    {
        await this.EnsureReportExists(id);
        return await this.reportDao.UpdateReport(updatedReport);
    }

    public async Task<bool> DeleteReport(string id)
    {
        await this.EnsureReportExists(id);
        return await this.reportDao.DeleteReport(id);
    }

    private async Task EnsureReportExists(string id)
    {
        var report = await this.GetReport(id);
        if (report is null)
        {
            throw new NotFoundException();
        }
    }
}