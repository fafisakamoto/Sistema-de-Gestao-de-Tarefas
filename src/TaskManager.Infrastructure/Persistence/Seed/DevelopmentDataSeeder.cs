using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Common.Time;
using TaskManager.Domain.Tasks;

namespace TaskManager.Infrastructure.Persistence.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        if (await context.Tasks.AnyAsync(cancellationToken))
            return;

        var now = timeProvider.GetUtcDateTime();
        var today = timeProvider.GetToday();

        context.Tasks.AddRange(
            TaskItem.Create("Configurar pipeline de CI", "Build, testes e cobertura no GitHub Actions", today.AddDays(2), TaskItemStatus.InProgress, now),
            TaskItem.Create("Revisar pull requests", "PRs pendentes do time de pagamentos", today, TaskItemStatus.Pending, now),
            TaskItem.Create("Atualizar documentação da API", null, today.AddDays(7), TaskItemStatus.Pending, now),
            TaskItem.Create("Corrigir bug de timezone", "Datas exibidas com um dia de diferença", today.AddDays(-3), TaskItemStatus.Pending, now),
            TaskItem.Create("Planejar sprint 42", null, today.AddDays(-1), TaskItemStatus.Completed, now),
            TaskItem.Create("Estudar Clean Architecture", null, null, TaskItemStatus.Pending, now));

        await context.SaveChangesAsync(cancellationToken);
    }
}
