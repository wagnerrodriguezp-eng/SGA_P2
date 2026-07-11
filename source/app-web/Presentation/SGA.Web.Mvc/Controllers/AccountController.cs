using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.Web.Mvc.Models.Account;
using SGA.Web.Mvc.Models.Api;
using SGA.Web.Mvc.Services;

namespace SGA.Web.Mvc.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ISgaApiClient _apiClient;

    public AccountController(ISgaApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.PostAsync("api/v1/auth/register", new
        {
            model.Email,
            model.Password,
            model.FirstName,
            model.LastName,
            Role = (int)model.Role,
            model.StudentCode,
            model.EmployeeCode,
            model.LicenseNumber
        }, ct);

        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "The service is temporarily unavailable — please try again shortly.");
            return View(model);
        }
        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors.DefaultIfEmpty(result.Message ?? "Registration failed."))
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["Message"] = result.Message ?? "Registration successful. Please check your email to confirm your account.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.PostAsync<TokenResponseModel>(
            "api/v1/auth/login", new { model.Email, model.Password }, ct);

        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "The service is temporarily unavailable — please try again shortly.");
            return View(model);
        }
        if (!result.IsSuccess || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInWithTokenAsync(result.Data.Token);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ConfirmEmail(Guid userId, string token) =>
        View(new ConfirmEmailViewModel { UserId = userId, Token = token });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model, CancellationToken ct)
    {
        var result = await _apiClient.PostAsync("api/v1/auth/confirm-email", new { model.UserId, model.Token }, ct);
        ViewBag.Success = result?.IsSuccess ?? false;
        ViewBag.ResultMessage = result?.Message
            ?? "The service is temporarily unavailable — please try again shortly.";
        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _apiClient.PostAsync("api/v1/auth/forgot-password", new { model.Email }, ct);
        ViewBag.ResultMessage = "If that email exists, a password reset link has been sent.";
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword(Guid userId, string token) =>
        View(new ResetPasswordViewModel { UserId = userId, Token = token });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.PostAsync(
            "api/v1/auth/reset-password", new { model.UserId, model.Token, model.NewPassword }, ct);

        if (result is not null && result.IsSuccess)
        {
            TempData["Message"] = "Password reset successfully. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, result?.Message ?? "Unable to reset password.");
        return View(model);
    }

    private async Task SignInWithTokenAsync(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        // ReadJwtToken does not apply JwtSecurityTokenHandler's inbound claim-type mapping, so the
        // "email" claim keeps its short JWT name here, not the long ClaimTypes.Email URI.
        var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme, JwtRegisteredClaimNames.Email, ClaimTypes.Role);
        identity.AddClaim(new Claim("access_token", token));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });
    }
}
