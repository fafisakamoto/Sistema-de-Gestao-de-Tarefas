using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Tasks;

namespace TaskManager.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).ValueGeneratedNever();

        builder.Property(task => task.Title)
            .HasMaxLength(TaskItem.TitleMaxLength)
            .IsRequired();

        builder.Property(task => task.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(task => task.Status);
        builder.HasIndex(task => task.DueDate);
    }
}
