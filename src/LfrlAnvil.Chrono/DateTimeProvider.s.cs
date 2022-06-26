namespace LfrlAnvil.Chrono;

public static class DateTimeProvider
{
    public static readonly IDateTimeProvider Utc = new UtcDateTimeProvider();
    public static readonly IDateTimeProvider Local = new LocalDateTimeProvider();
}
