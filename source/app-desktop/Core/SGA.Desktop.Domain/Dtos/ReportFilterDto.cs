namespace SGA.Desktop.Domain.Dtos;

public class ReportFilterDto
{
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public Guid? RouteId { get; set; }
}
