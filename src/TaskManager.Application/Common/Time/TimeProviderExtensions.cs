namespace TaskManager.Application.Common.Time;

public static class TimeProviderExtensions
{
    public static DateTime GetUtcDateTime(this TimeProvider timeProvider) =>
        timeProvider.GetUtcNow().UtcDateTime;

    public static DateOnly GetToday(this TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
}
