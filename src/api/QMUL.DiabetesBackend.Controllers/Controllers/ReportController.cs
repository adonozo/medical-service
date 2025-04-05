namespace QMUL.DiabetesBackend.Controllers.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Model;
using ServiceInterfaces;

[ApiController]
public class ReportController : ControllerBase
{
    private readonly IReportService reportService;

    public ReportController(IReportService reportService)
    {
        this.reportService = reportService;
    }

    [HttpGet("reports")]
    public async Task<IActionResult> SearchReports([FromQuery] int? limit = null, [FromQuery] string? after = null)
    {
        var paginationRequest = new PaginationRequest(limit, after);
        var results = await this.reportService.SearchReports(paginationRequest);
        return Ok(results);
    }

    [HttpGet("reports/{id}")]
    public async Task<IActionResult> GetReport([FromRoute] string id)
    {
        var report = await this.reportService.GetReport(id);
        return report is null ? NotFound() : Ok(report);
    }

    [HttpPost("reports")]
    public async Task<IActionResult> CreateReport([FromBody] DiagnosisReport report)
    {
        var result = await this.reportService.CreateReport(report);
        return Ok(result);
    }

    [HttpPut("reports/{id}")]
    public async Task<IActionResult> UpdateReport([FromRoute] string id, [FromBody] DiagnosisReport report)
    {
        var isReportUpdated = await this.reportService.UpdateReport(id, report);
        return isReportUpdated ? NoContent() : BadRequest();
    }

    [HttpDelete("reports/{id}")]
    public async Task<IActionResult> DeleteReport([FromRoute] string id)
    {
        var isReportDeleted = await this.reportService.DeleteReport(id);
        return isReportDeleted ? NoContent() : NotFound();
    }
}