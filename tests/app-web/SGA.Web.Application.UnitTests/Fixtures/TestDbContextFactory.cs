using Microsoft.EntityFrameworkCore;
using SGA.Web.Infrastructure.Persistence;

namespace SGA.Web.Application.UnitTests.Fixtures;

internal static class TestDbContextFactory
{
    public static WebAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<WebAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WebAppDbContext(options, new TestCurrentUserAccessor());
    }
}
