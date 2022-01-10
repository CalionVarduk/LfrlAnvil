using System;
using System.Globalization;

namespace LfrlSoft.NET.Core.Chrono.Exceptions
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
            var dateTimeText = dateTime.ToString( CultureInfo.InvariantCulture );
            var timeZoneText = timeZone.Id;
            // TODO (LF): add more info i.e. why is it invalid?
            return $"{dateTimeText} is not a valid datetime in {timeZoneText} timezone.";
        }
    }
}
