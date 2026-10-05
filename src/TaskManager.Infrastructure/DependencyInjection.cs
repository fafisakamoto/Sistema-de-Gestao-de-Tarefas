using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Tasks;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Repositories;
using TaskManager.Infrastructure.Time;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public const string DatabaseName = "TaskManagerDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(DatabaseName));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ITaskRepository, TaskRepository>();

        services.AddOptions<ClockOptions>()
            .BindConfiguration(ClockOptions.SectionName)
            .Validate(options => TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _),
                "Clock:TimeZone deve ser um fuso horário válido (ex.: America/Sao_Paulo).")
            .ValidateOnStart();

        services.Replace(ServiceDescriptor.Singleton<TimeProvider>(provider =>
            BusinessTimeProvider.From(provider.GetRequiredService<IOptions<ClockOptions>>().Value)));

        return services;
    }
}
