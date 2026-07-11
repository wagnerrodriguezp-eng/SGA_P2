using SGA.Desktop.Infrastructure.Persistence;

namespace SGA.Desktop.Application.UnitTests.Fixtures;

internal class TestCurrentUserAccessor : ICurrentUserAccessor
{
    public string? UserId => null;
}
