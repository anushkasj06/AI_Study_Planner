using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewData["HideShell"] = true;
        ViewData["Title"] = "AI Study Planner";
        return View();
    }
}