using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

public sealed class ErrorController : Controller
{
    [HttpGet("/error")]
    public IActionResult Index()
    {
        ViewData["HideShell"] = true;
        ViewData["Title"] = "Error";
        return View();
    }
}