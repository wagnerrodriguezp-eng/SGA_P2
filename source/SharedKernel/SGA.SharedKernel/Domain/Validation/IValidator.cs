using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Notifications;

namespace SGA.SharedKernel.Domain.Validation;

public interface IValidator<TEntity, in TCreateDto, in TUpdateDto>
    where TEntity : BaseEntity<Guid>
{
    Task<NotificationContext> ValidateForCreateAsync(TCreateDto dto, CancellationToken ct = default);
    Task<NotificationContext> ValidateForUpdateAsync(Guid id, TUpdateDto dto, CancellationToken ct = default);
    Task<NotificationContext> ValidateForDeactivateAsync(Guid id, CancellationToken ct = default);
}
