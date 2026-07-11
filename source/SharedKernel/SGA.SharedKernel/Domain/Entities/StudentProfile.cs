using SGA.SharedKernel.Domain.Common;

namespace SGA.SharedKernel.Domain.Entities;

public class StudentProfile : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string? Career { get; set; }
}
