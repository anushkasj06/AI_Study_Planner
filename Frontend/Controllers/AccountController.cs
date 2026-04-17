using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;
    private readonly AuthSessionService _authSessionService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        StudyPlannerApiClient apiClient,
        AuthSessionService authSessionService,
        ILogger<AccountController> logger)
    {
        _apiClient = apiClient;
        _authSessionService = authSessionService;
        _logger = logger;
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        ViewData["HideShell"] = true;
        ViewData["Title"] = "Login";
        return View(new LoginViewModel());
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["HideShell"] = true;
        ViewData["Title"] = "Login";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var authResponse = await _apiClient.LoginAsync(model);
            await _authSessionService.SignInAsync(HttpContext, authResponse);
            TempData["SuccessMessage"] = "Welcome back!";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Login failed for email {Email}", model.Email);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet("/register")]
    public IActionResult Register()
    {
        ViewData["HideShell"] = true;
        ViewData["Title"] = "Register";
        return View(new RegisterViewModel());
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewData["HideShell"] = true;
        ViewData["Title"] = "Register";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var authResponse = await _apiClient.RegisterAsync(model);
            await _authSessionService.SignInAsync(HttpContext, authResponse);
            TempData["SuccessMessage"] = "Account created successfully!";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Registration failed for email {Email}", model.Email);
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authSessionService.SignOutAsync(HttpContext);
        TempData["SuccessMessage"] = "You have been signed out.";
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}