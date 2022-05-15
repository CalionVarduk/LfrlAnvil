using System;
using LfrlAnvil.Chrono.Extensions;

namespace LfrlAnvil.Chrono.Exceptions
{
    public class InvalidZonedDateTimeException : ArgumentException
    {
        public InvalidZonedDateTimeException(DateTime dateTime, TimeZoneInfo timeZone)
            : base( CreateMessage( dateTime, timeZone ) )
        {
            DateTime = dateTime;
            TimeZone = timeZone;
        }

        public DateTime DateTime { get; }
        public TimeZoneInfo TimeZone { get; }

        private static string CreateMessage(DateTime dateTime, TimeZoneInfo timeZone)
        {
            var invalidityRange = timeZone.GetContainingInvalidityRange( dateTime );
            return invalidityRange is null
                ? Resources.InvalidDateTimeInTimeZone( dateTime, timeZone )
                : Resources.InvalidDateTimeInTimeZoneBecauseOfRange( dateTime, timeZone, invalidityRange.Value );
        }
    }
}
