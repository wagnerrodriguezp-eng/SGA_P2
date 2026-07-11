using Microsoft.EntityFrameworkCore;
using SGA.SharedKernel.Domain.Entities;
using SGA.Desktop.Application.Persistence;

namespace SGA.Desktop.Infrastructure.Persistence.Repositories;

public class BusRepository : GenericRepository<Bus, Guid>, IBusRepository
{
    public BusRepository(DesktopAppDbContext context) : base(context)
    {
    }

    public async Task<Bus?> GetByPlateNumberAsync(string plateNumber, CancellationToken ct = default) =>
        await Context.Buses.FirstOrDefaultAsync(b => b.PlateNumber == plateNumber, ct);
}
