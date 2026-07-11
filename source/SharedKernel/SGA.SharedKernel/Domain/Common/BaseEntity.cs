using SGA.SharedKernel.Domain.Enums;

namespace SGA.SharedKernel.Domain.Common;

public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
}
