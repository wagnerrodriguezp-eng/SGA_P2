using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Mvc.Models;
using SGA.Web.Mvc.Models.Api;
using SGA.Web.Mvc.Services;

namespace SGA.Web.Mvc.Controllers;

[Authorize(Roles = "Student,Employee")]
public class AccessController : Controller
{
    private readonly ISgaApiClient _apiClient;

    public AccessController(ISgaApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Validate() => View(new ValidateAccessViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(ValidateAccessViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.PostAsync<UsageRecordModel>(
            "api/v1/access/validate", new { model.AuthorizationIdentifier, model.TripId }, ct);

        if (result is null)
        {
            return View("~/Views/Shared/_ServiceUnavailable.cshtml");
        }
        if (!result.IsSuccess || result.Data is null)
        {
            // Honest, specific message rather than a generic error — e.g. "This trip just reached
            // full capacity" for a concurrency conflict, which is an expected, frequent outcome.
            ModelState.AddModelError(string.Empty, result.Message ?? "Unable to validate access right now, please try again.");
            return View(model);
        }

        return View("ValidateResult", result.Data);
    }
}
