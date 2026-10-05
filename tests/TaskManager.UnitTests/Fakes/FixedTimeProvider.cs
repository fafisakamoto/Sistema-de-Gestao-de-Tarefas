namespace TaskManager.UnitTests.Fakes;

internal sealed class FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo? timeZone = null) : TimeProvider
{
    public static readonly DateTimeOffset DefaultNow = new(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

    public FixedTimeProvider() : this(DefaultNow) { }

    public override DateTimeOffset GetUtcNow() => utcNow;

    public override TimeZoneInfo LocalTimeZone => timeZone ?? TimeZoneInfo.Utc;
}
