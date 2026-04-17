using AIStudyPlanner.Api.Interfaces;

namespace AIStudyPlanner.Api.Data;

public class DataSeeder : IDataSeeder
{
    public Task SeedAsync()
    {
        return Task.CompletedTask;
    }
}
