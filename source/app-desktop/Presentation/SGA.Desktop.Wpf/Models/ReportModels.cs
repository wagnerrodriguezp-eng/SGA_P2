namespace SGA.Desktop.Wpf.Models;

public class UsageReportModel
{
    public int TotalAttempts { get; set; }
    public int GrantedCount { get; set; }
    public int DeniedExpiredCount { get; set; }
    public int DeniedNoBalanceCount { get; set; }
    public int DeniedNoCapacityCount { get; set; }
    public int DeniedUnauthorizedCount { get; set; }
}

public class OccupancyReportModel
{
    public int TotalTrips { get; set; }
    public double AverageOccupancyPercentage { get; set; }
}

public class PunctualityReportModel
{
    public int TotalTrips { get; set; }
    public int OnTimeCount { get; set; }
    public int DelayedCount { get; set; }
    public double OnTimePercentage { get; set; }
}

public class RevenueReportModel
{
    public int MonthlyTicketsIssued { get; set; }
    public int RechargeableCardsIssued { get; set; }
}
