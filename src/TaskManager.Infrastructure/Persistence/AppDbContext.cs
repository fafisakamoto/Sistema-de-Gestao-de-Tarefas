using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Tasks;

namespace TaskManager.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
