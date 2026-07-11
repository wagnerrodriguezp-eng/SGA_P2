using SGA.Web.Infrastructure.Persistence;

namespace SGA.Web.Application.UnitTests.Fixtures;

internal class TestCurrentUserAccessor : ICurrentUserAccessor
{
    public string? UserId => null;
}
