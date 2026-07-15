using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Persistence;
using SGA.Desktop.Application.Reporting;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Application.Services;

public class ReportingService
{
    private readonly IReportingRepository _reportingRepository;

    public ReportingService(IReportingRepository reportingRepository)
    {
        _reportingRepository = reportingRepository;
    }

    public Task<UsageReportResult> GetUsageReportAsync(ReportFilterDto filter, CancellationToken ct = default) =>
        _reportingRepository.GetUsageReportAsync(filter, ct);

    public Task<OccupancyReportResult> GetOccupancyReportAsync(ReportFilterDto filter, CancellationToken ct = default) =>
        _reportingRepository.GetOccupancyReportAsync(filter, ct);

    public Task<PunctualityReportResult> GetPunctualityReportAsync(ReportFilterDto filter, CancellationToken ct = default) =>
        _reportingRepository.GetPunctualityReportAsync(filter, ct);

    public Task<RevenueReportResult> GetRevenueReportAsync(ReportFilterDto filter, CancellationToken ct = default) =>
        _reportingRepository.GetRevenueReportAsync(filter, ct);

    public Task<IReadOnlyList<UsageRecord>> GetUsageRecordsAsync(ReportFilterDto filter, CancellationToken ct = default) =>
        _reportingRepository.GetUsageRecordsAsync(filter, ct);
}
