using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class GoalsController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public GoalsController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("/app/goals")]
    public async Task<IActionResult> Index()
    {
        var goals = await _apiClient.GoalsAsync();
        return View(new GoalsPageViewModel { Goals = goals });
    }

    [HttpGet("/app/goals/new")]
    public IActionResult Create()
    {
        return View(new GoalFormViewModel());
    }

    [HttpPost("/app/goals/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GoalFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!DateTime.TryParse(model.TargetDate, out var targetDate))
        {
            ModelState.AddModelError(nameof(model.TargetDate), "Select a valid date.");
            return View(model);
        }

        try
        {
            var createdGoal = await _apiClient.CreateGoalAsync(new StudyGoalCreateRequest
            {
                Title = model.Title,
                Description = model.Description,
                TargetDate = targetDate,
                DailyAvailableHours = model.DailyAvailableHours,
                DifficultyLevel = model.DifficultyLevel,
                Priority = model.Priority,
                PreferredStudyTime = model.PreferredStudyTime,
                BreakPreference = model.BreakPreference,
                Status = model.Status,
                Subjects = SplitSubjects(model.Subjects),
                AutoGeneratePlan = model.AutoGeneratePlan
            });

            if (model.AutoGeneratePlan)
            {
                await _apiClient.GeneratePlanAsync(createdGoal.Id, regenerate: false);
            }

            TempData["SuccessMessage"] = "Goal created successfully.";
            return RedirectToAction(nameof(Details), new { goalId = createdGoal.Id });
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet("/app/goals/{goalId:guid}")]
    public async Task<IActionResult> Details(Guid goalId)
    {
        var goal = await _apiClient.GoalByIdAsync(goalId);
        var plans = await _apiClient.PlansByGoalAsync(goalId);

        return View(new GoalDetailsPageViewModel
        {
            Goal = goal,
            Plans = plans
        });
    }

    [HttpPost("/app/goals/{goalId:guid}/regenerate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Regenerate(Guid goalId)
    {
        await _apiClient.GeneratePlanAsync(goalId, regenerate: true);
        TempData["SuccessMessage"] = "Plan regenerated successfully.";
        return RedirectToAction(nameof(Details), new { goalId });
    }

    private static List<string> SplitSubjects(string subjects)
    {
        return subjects
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }
}