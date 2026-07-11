using SGA.SharedKernel.Application.Persistence;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Application.Persistence;

public interface IBusRepository : IGenericRepository<Bus, Guid>
{
    Task<Bus?> GetByPlateNumberAsync(string plateNumber, CancellationToken ct = default);
}
