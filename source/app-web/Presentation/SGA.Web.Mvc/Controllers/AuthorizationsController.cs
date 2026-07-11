using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Mvc.Models.Api;
using SGA.Web.Mvc.Services;

namespace SGA.Web.Mvc.Controllers;

[Authorize(Roles = "Student,Employee")]
public class AuthorizationsController : Controller
{
    private readonly ISgaApiClient _apiClient;

    public AuthorizationsController(ISgaApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await _apiClient.GetAsync<AuthorizationModel>("api/v1/authorizations/me", ct);
        if (result is null)
        {
            return View("~/Views/Shared/_ServiceUnavailable.cshtml");
        }

        ViewBag.ResultMessage = result.IsSuccess ? null : result.Message;
        return View(result.Data);
    }
}
