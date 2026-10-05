namespace TaskManager.Infrastructure.Time;

public sealed class ClockOptions
{
    public const string SectionName = "Clock";

    public string TimeZone { get; set; } = "America/Sao_Paulo";
}
