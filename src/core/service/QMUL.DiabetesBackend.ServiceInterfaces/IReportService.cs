namespace QMUL.DiabetesBackend.ServiceInterfaces;

using System.Threading.Tasks;
using Model;

public interface IReportService
{
    Task<DiagnosisReport?> GetReport(string id);

    Task<PaginatedResults<DiagnosisReport>> SearchReports(PaginationRequest paginationRequest);

    Task<DiagnosisReport> CreateReport(DiagnosisReport report);

    Task<bool> UpdateReport(string id, DiagnosisReport updatedReport);

    Task<bool> DeleteReport(string id);
}