using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AIStudyPlanner.Api.Filters;

public sealed class ActionLoggingFilter(ILogger<ActionLoggingFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var controller = descriptor?.ControllerName ?? "UnknownController";
        var action = descriptor?.ActionName ?? "UnknownAction";
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path.ToString();
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        logger.LogInformation(
            "Action started {Controller}.{Action} for {Method} {Path} by user {UserId}",
            controller,
            action,
            method,
            path,
            userId);

        var stopwatch = Stopwatch.StartNew();
        var executedContext = await next();
        stopwatch.Stop();

        if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
        {
            logger.LogError(
                executedContext.Exception,
                "Action failed {Controller}.{Action} for {Method} {Path} in {ElapsedMs}ms",
                controller,
                action,
                method,
                path,
                stopwatch.ElapsedMilliseconds);
            return;
        }

        logger.LogInformation(
            "Action completed {Controller}.{Action} for {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
            controller,
            action,
            method,
            path,
            context.HttpContext.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
