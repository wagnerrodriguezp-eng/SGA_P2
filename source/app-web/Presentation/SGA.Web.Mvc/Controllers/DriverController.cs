using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Mvc.Models;
using SGA.Web.Mvc.Models.Api;
using SGA.Web.Mvc.Services;

namespace SGA.Web.Mvc.Controllers;

[Authorize(Roles = "Driver")]
public class DriverController : Controller
{
    private readonly ISgaApiClient _apiClient;

    public DriverController(ISgaApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> AssignedTrips(CancellationToken ct)
    {
        var result = await _apiClient.GetAsync<List<TripModel>>("api/v1/trips/assigned", ct);
        if (result is null)
        {
            return View("~/Views/Shared/_ServiceUnavailable.cshtml");
        }

        return View(result.Data ?? new List<TripModel>());
    }

    public async Task<IActionResult> TripDetails(Guid id, CancellationToken ct)
    {
        var result = await _apiClient.GetAsync<List<TripModel>>("api/v1/trips/assigned", ct);
        var trip = result?.Data?.FirstOrDefault(t => t.Id == id);
        if (trip is null)
        {
            return NotFound();
        }

        return View(trip);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var result = await _apiClient.PostAsync($"api/v1/trips/{id}/start", null, ct);
        TempData["Message"] = result?.IsSuccess == true ? "Trip started." : result?.Message ?? "Unable to start the trip.";
        return RedirectToAction(nameof(TripDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> End(Guid id, CancellationToken ct)
    {
        var result = await _apiClient.PostAsync($"api/v1/trips/{id}/end", null, ct);
        TempData["Message"] = result?.IsSuccess == true ? "Trip ended." : result?.Message ?? "Unable to end the trip.";
        return RedirectToAction(nameof(TripDetails), new { id });
    }

    [HttpGet]
    public IActionResult ReportIncident(Guid id) => View(new ReportIncidentViewModel { TripId = id });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportIncident(ReportIncidentViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.PostAsync($"api/v1/trips/{model.TripId}/incidents",
            new { IncidentType = (int)model.IncidentType, model.Description }, ct);

        if (result is not null && result.IsSuccess)
        {
            TempData["Message"] = "Incident reported.";
            return RedirectToAction(nameof(TripDetails), new { id = model.TripId });
        }

        ModelState.AddModelError(string.Empty, result?.Message ?? "Unable to report incident.");
        return View(model);
    }
}
