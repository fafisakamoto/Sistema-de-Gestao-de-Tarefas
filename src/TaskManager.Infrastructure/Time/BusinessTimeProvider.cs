namespace TaskManager.Infrastructure.Time;

internal sealed class BusinessTimeProvider(TimeZoneInfo timeZone) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => timeZone;

    public static BusinessTimeProvider From(ClockOptions options) =>
        new(TimeZoneInfo.FindSystemTimeZoneById(options.TimeZone));
}
