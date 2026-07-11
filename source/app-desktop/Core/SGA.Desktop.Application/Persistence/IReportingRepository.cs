using SGA.Desktop.Domain.Dtos;
using SGA.Desktop.Application.Reporting;

namespace SGA.Desktop.Application.Persistence;

// Read-only aggregate LINQ queries backing ReportingService — the one place in the system where
// raw cross-table joins are expected and acceptable, since they produce projected report DTOs, not
// domain entities.
public interface IReportingRepository
{
    Task<UsageReportResult> GetUsageReportAsync(ReportFilterDto filter, CancellationToken ct = default);
    Task<OccupancyReportResult> GetOccupancyReportAsync(ReportFilterDto filter, CancellationToken ct = default);
    Task<PunctualityReportResult> GetPunctualityReportAsync(ReportFilterDto filter, CancellationToken ct = default);
    Task<RevenueReportResult> GetRevenueReportAsync(ReportFilterDto filter, CancellationToken ct = default);
}
