using SGA.SharedKernel.Domain.Common;

namespace SGA.SharedKernel.Domain.Entities;

public class DriverProfile : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
