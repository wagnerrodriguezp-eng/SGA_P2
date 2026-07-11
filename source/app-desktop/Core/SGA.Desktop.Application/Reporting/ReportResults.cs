namespace SGA.Desktop.Application.Reporting;

public record UsageReportResult(
    int TotalAttempts,
    int GrantedCount,
    int DeniedExpiredCount,
    int DeniedNoBalanceCount,
    int DeniedNoCapacityCount,
    int DeniedUnauthorizedCount);

public record OccupancyReportResult(int TotalTrips, double AverageOccupancyPercentage);

// OnTime = Trip.StartedAtUtc within 10 minutes of the scheduled departure (TripDate + Schedule.DepartureTime).
public record PunctualityReportResult(int TotalTrips, int OnTimeCount, int DelayedCount, double OnTimePercentage);

// Proxy metric — the entity catalog has no price/currency field anywhere (consistent with the SRS
// explicitly excluding financial/payment management), so this counts Authorization issuance events
// instead of fabricating a currency amount that isn't part of the modeled schema.
public record RevenueReportResult(int MonthlyTicketsIssued, int RechargeableCardsIssued);
