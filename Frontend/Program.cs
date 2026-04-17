using System.Globalization;
using System.Net.Http.Headers;
using AIStudyPlanner.Web.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var envFilePath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envFilePath))
{
    Env.NoClobber().Load(envFilePath);
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.AddScoped<AuthSessionService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "AIStudyPlanner.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient<StudyPlannerApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-IN");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-IN");

var app = builder.Build();

var frontendApiOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
app.Logger.LogInformation("Frontend configured backend API base URL: {ApiBaseUrl}", frontendApiOptions.BaseUrl);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    await next();
    stopwatch.Stop();

    app.Logger.LogInformation(
        "Frontend request {Method} {Path} returned {StatusCode} in {ElapsedMs}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();