using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SGA.Web.Infrastructure.Persistence;

public interface ICurrentUserAccessor
{
    string? UserId { get; }
}

public class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
