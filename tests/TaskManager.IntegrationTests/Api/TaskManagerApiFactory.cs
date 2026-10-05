using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Time;

namespace TaskManager.IntegrationTests.Api;

public sealed class TaskManagerApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TaskManagerTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.Configure<ClockOptions>(options => options.TimeZone = "UTC");
        });
    }
}
