using System;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;

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
            var dateTimeText = TextFormatting.StringifyDateTime( dateTime );
            var timeZoneText = timeZone.Id;

            var invalidityRange = timeZone.GetContainingInvalidityRange( dateTime );
            if ( invalidityRange is null )
                return $"{dateTimeText} is not a valid datetime in {timeZoneText} timezone.";

            var invalidityMinText = TextFormatting.StringifyDateTime( invalidityRange.Value.Min );
            var invalidityMaxText = TextFormatting.StringifyDateTime( invalidityRange.Value.Max );
            var invalidityText = $"{invalidityMinText}, {invalidityMaxText}";

            return
                $"{dateTimeText} is not a valid datetime in {timeZoneText} timezone because it falls into the [{invalidityText}] range.";
        }
    }
}
