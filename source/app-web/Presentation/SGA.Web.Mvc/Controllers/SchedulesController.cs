using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Mvc.Models.Api;
using SGA.Web.Mvc.Services;

namespace SGA.Web.Mvc.Controllers;

[Authorize]
public class SchedulesController : Controller
{
    private readonly ISgaApiClient _apiClient;

    public SchedulesController(ISgaApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var result = await _apiClient.GetAsync<List<RouteModel>>("api/v1/routes", ct);
        if (result is null)
        {
            return View("~/Views/Shared/_ServiceUnavailable.cshtml");
        }

        return View(result.Data ?? new List<RouteModel>());
    }
}
