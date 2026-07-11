using Microsoft.AspNetCore.Identity;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.Identity.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
}
