namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Threading.Tasks;
using Model;

public interface IReportDao
{
    Task<DiagnosisReport?> GetReport(string id);

    Task<PaginatedResults<DiagnosisReport>> SearchReports(PaginationRequest paginationRequest);

    Task<DiagnosisReport> InsertReport(DiagnosisReport report);

    Task<bool> UpdateReport(DiagnosisReport updatedReport);

    Task<bool> DeleteReport(string id);
}