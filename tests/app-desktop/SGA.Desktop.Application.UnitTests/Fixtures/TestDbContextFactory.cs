using Microsoft.EntityFrameworkCore;
using SGA.Desktop.Infrastructure.Persistence;

namespace SGA.Desktop.Application.UnitTests.Fixtures;

internal static class TestDbContextFactory
{
    public static DesktopAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<DesktopAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DesktopAppDbContext(options, new TestCurrentUserAccessor());
    }
}
