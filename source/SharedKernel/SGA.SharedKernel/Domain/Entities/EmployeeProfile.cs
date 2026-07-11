using SGA.SharedKernel.Domain.Common;

namespace SGA.SharedKernel.Domain.Entities;

public class EmployeeProfile : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string? Department { get; set; }
}
