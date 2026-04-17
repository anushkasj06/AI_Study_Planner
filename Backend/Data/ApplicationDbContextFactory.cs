using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace AIStudyPlanner.Api.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var backendDirectory = ResolveBackendDirectory(currentDirectory);

        var envFilePath = Path.Combine(backendDirectory, ".env");
        if (File.Exists(envFilePath))
        {
            Env.Load(envFilePath);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(backendDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "server=localhost;port=3306;database=AIStudyPlannerDb;user=root;password=YOUR_PASSWORD;";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveBackendDirectory(string currentDirectory)
    {
        if (File.Exists(Path.Combine(currentDirectory, "appsettings.json")))
        {
            return currentDirectory;
        }

        var candidate = Path.Combine(currentDirectory, "Backend");
        if (File.Exists(Path.Combine(candidate, "appsettings.json")))
        {
            return candidate;
        }

        return currentDirectory;
    }
}
