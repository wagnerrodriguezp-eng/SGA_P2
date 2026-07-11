using SGA.SharedKernel.Domain.Notifications;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Domain.Validation;

public interface IRouteScheduleValidator
{
    Task<NotificationContext> ValidateRouteForCreateAsync(CreateRouteDto dto, CancellationToken ct = default);
    Task<NotificationContext> ValidateRouteForUpdateAsync(Guid routeId, UpdateRouteDto dto, CancellationToken ct = default);
    Task<NotificationContext> ValidateStopForCreateAsync(CreateStopDto dto, CancellationToken ct = default);
    Task<NotificationContext> ValidateScheduleForCreateAsync(CreateScheduleDto dto, CancellationToken ct = default);
}
