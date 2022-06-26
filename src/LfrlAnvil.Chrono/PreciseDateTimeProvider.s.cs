namespace LfrlAnvil.Chrono;

public static class PreciseDateTimeProvider
{
    public static readonly IDateTimeProvider Utc = new PreciseUtcDateTimeProvider();
    public static readonly IDateTimeProvider Local = new PreciseLocalDateTimeProvider();
}
