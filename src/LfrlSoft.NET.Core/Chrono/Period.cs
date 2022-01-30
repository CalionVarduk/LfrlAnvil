using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct Period : IEquatable<Period>
    {
        public static readonly Period Empty = new Period();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period(int years, int months, int weeks, int days)
            : this( years, months, weeks, days, 0, 0, 0, 0, 0 ) { }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period(int hours, long minutes, long seconds, long milliseconds, long ticks)
            : this( 0, 0, 0, 0, hours, minutes, seconds, milliseconds, ticks ) { }

        public Period(int years, int months, int weeks, int days, int hours, long minutes, long seconds, long milliseconds, long ticks)
        {
            Years = years;
            Months = months;
            Weeks = weeks;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
            Ticks = ticks;
        }

        public Period(TimeSpan timeSpan)
            : this(
                0,
                0,
                0,
                (int)timeSpan.TotalDays,
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds,
                timeSpan.Ticks % Constants.TicksPerMillisecond ) { }

        public int Years { get; }
        public int Months { get; }
        public int Weeks { get; }
        public int Days { get; }
        public int Hours { get; }
        public long Minutes { get; }
        public long Seconds { get; }
        public long Milliseconds { get; }
        public long Ticks { get; }

        public PeriodUnits ActiveUnits =>
            (Years != 0 ? PeriodUnits.Years : PeriodUnits.None) |
            (Months != 0 ? PeriodUnits.Months : PeriodUnits.None) |
            (Weeks != 0 ? PeriodUnits.Weeks : PeriodUnits.None) |
            (Days != 0 ? PeriodUnits.Days : PeriodUnits.None) |
            (Hours != 0 ? PeriodUnits.Hours : PeriodUnits.None) |
            (Minutes != 0 ? PeriodUnits.Minutes : PeriodUnits.None) |
            (Seconds != 0 ? PeriodUnits.Seconds : PeriodUnits.None) |
            (Milliseconds != 0 ? PeriodUnits.Milliseconds : PeriodUnits.None) |
            (Ticks != 0 ? PeriodUnits.Ticks : PeriodUnits.None);

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromTicks(long ticks)
        {
            return new Period( 0, 0, 0, 0, ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromMilliseconds(long milliseconds)
        {
            return new Period( 0, 0, 0, milliseconds, 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromSeconds(long seconds)
        {
            return new Period( 0, 0, seconds, 0, 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromMinutes(long minutes)
        {
            return new Period( 0, minutes, 0, 0, 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromHours(int hours)
        {
            return new Period( hours, 0, 0, 0, 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromDays(int days)
        {
            return new Period( 0, 0, 0, days );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromWeeks(int weeks)
        {
            return new Period( 0, 0, weeks, 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromMonths(int months)
        {
            return new Period( 0, months, 0, 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period FromYears(int years)
        {
            return new Period( years, 0, 0, 0 );
        }

        [Pure]
        public override string ToString()
        {
            var texts = new[]
            {
                Years != 0 ? $"{Years} year(s)" : string.Empty,
                Months != 0 ? $"{Months} month(s)" : string.Empty,
                Weeks != 0 ? $"{Weeks} week(s)" : string.Empty,
                Days != 0 ? $"{Days} day(s)" : string.Empty,
                Hours != 0 ? $"{Hours} hour(s)" : string.Empty,
                Minutes != 0 ? $"{Minutes} minute(s)" : string.Empty,
                Seconds != 0 ? $"{Seconds} second(s)" : string.Empty,
                Milliseconds != 0 ? $"{Milliseconds} millisecond(s)" : string.Empty,
                Ticks != 0 ? $"{Ticks} tick(s)" : string.Empty
            };

            var result = string.Join( ", ", texts.Where( t => t.Length > 0 ) );
            return result.Length != 0 ? result : "0 day(s)";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override int GetHashCode()
        {
            return Hash.Default
                .Add( Years )
                .Add( Months )
                .Add( Weeks )
                .Add( Days )
                .Add( Hours )
                .Add( Minutes )
                .Add( Seconds )
                .Add( Milliseconds )
                .Add( Ticks )
                .Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is Period p && Equals( p );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Equals(Period other)
        {
            return Years.Equals( other.Years ) &&
                Months.Equals( other.Months ) &&
                Weeks.Equals( other.Weeks ) &&
                Days.Equals( other.Days ) &&
                Hours.Equals( other.Hours ) &&
                Minutes.Equals( other.Minutes ) &&
                Seconds.Equals( other.Seconds ) &&
                Milliseconds.Equals( other.Milliseconds ) &&
                Ticks.Equals( other.Ticks );
        }

        [Pure]
        public Period Add(Period other)
        {
            return new Period(
                Years + other.Years,
                Months + other.Months,
                Weeks + other.Weeks,
                Days + other.Days,
                Hours + other.Hours,
                Minutes + other.Minutes,
                Seconds + other.Seconds,
                Milliseconds + other.Milliseconds,
                Ticks + other.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddTicks(long ticks)
        {
            return SetTicks( Ticks + ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddMilliseconds(long milliseconds)
        {
            return SetMilliseconds( Milliseconds + milliseconds );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddSeconds(long seconds)
        {
            return SetSeconds( Seconds + seconds );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddMinutes(long minutes)
        {
            return SetMinutes( Minutes + minutes );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddHours(int hours)
        {
            return SetHours( Hours + hours );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddDays(int days)
        {
            return SetDays( Days + days );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddWeeks(int weeks)
        {
            return SetWeeks( Weeks + weeks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddMonths(int months)
        {
            return SetMonths( Months + months );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period AddYears(int years)
        {
            return SetYears( Years + years );
        }

        [Pure]
        public Period Subtract(Period other)
        {
            return new Period(
                Years - other.Years,
                Months - other.Months,
                Weeks - other.Weeks,
                Days - other.Days,
                Hours - other.Hours,
                Minutes - other.Minutes,
                Seconds - other.Seconds,
                Milliseconds - other.Milliseconds,
                Ticks - other.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractTicks(long ticks)
        {
            return SetTicks( Ticks - ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractMilliseconds(long milliseconds)
        {
            return SetMilliseconds( Milliseconds - milliseconds );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractSeconds(long seconds)
        {
            return SetSeconds( Seconds - seconds );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractMinutes(long minutes)
        {
            return SetMinutes( Minutes - minutes );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractHours(int hours)
        {
            return SetHours( Hours - hours );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractDays(int days)
        {
            return SetDays( Days - days );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractWeeks(int weeks)
        {
            return SetWeeks( Weeks - weeks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractMonths(int months)
        {
            return SetMonths( Months - months );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SubtractYears(int years)
        {
            return SetYears( Years - years );
        }

        [Pure]
        public Period Set(Period other, PeriodUnits units)
        {
            return new Period(
                (units & PeriodUnits.Years) != 0 ? other.Years : Years,
                (units & PeriodUnits.Months) != 0 ? other.Months : Months,
                (units & PeriodUnits.Weeks) != 0 ? other.Weeks : Weeks,
                (units & PeriodUnits.Days) != 0 ? other.Days : Days,
                (units & PeriodUnits.Hours) != 0 ? other.Hours : Hours,
                (units & PeriodUnits.Minutes) != 0 ? other.Minutes : Minutes,
                (units & PeriodUnits.Seconds) != 0 ? other.Seconds : Seconds,
                (units & PeriodUnits.Milliseconds) != 0 ? other.Milliseconds : Milliseconds,
                (units & PeriodUnits.Ticks) != 0 ? other.Ticks : Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetDate(int years, int months, int weeks, int days)
        {
            return new Period(
                years,
                months,
                weeks,
                days,
                Hours,
                Minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetTime(int hours, long minutes, long seconds, long milliseconds, long ticks)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                Days,
                hours,
                minutes,
                seconds,
                milliseconds,
                ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetTicks(long ticks)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                Days,
                Hours,
                Minutes,
                Seconds,
                Milliseconds,
                ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetMilliseconds(long milliseconds)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                Days,
                Hours,
                Minutes,
                Seconds,
                milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetSeconds(long seconds)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                Days,
                Hours,
                Minutes,
                seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetMinutes(long minutes)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                Days,
                Hours,
                minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetHours(int hours)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                Days,
                hours,
                Minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetDays(int days)
        {
            return new Period(
                Years,
                Months,
                Weeks,
                days,
                Hours,
                Minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetWeeks(int weeks)
        {
            return new Period(
                Years,
                Months,
                weeks,
                Days,
                Hours,
                Minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetMonths(int months)
        {
            return new Period(
                Years,
                months,
                Weeks,
                Days,
                Hours,
                Minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period SetYears(int years)
        {
            return new Period(
                years,
                Months,
                Weeks,
                Days,
                Hours,
                Minutes,
                Seconds,
                Milliseconds,
                Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Period Negate()
        {
            return new Period(
                -Years,
                -Months,
                -Weeks,
                -Days,
                -Hours,
                -Minutes,
                -Seconds,
                -Milliseconds,
                -Ticks );
        }

        [Pure]
        public Period Skip(PeriodUnits units)
        {
            return new Period(
                (units & PeriodUnits.Years) != 0 ? 0 : Years,
                (units & PeriodUnits.Months) != 0 ? 0 : Months,
                (units & PeriodUnits.Weeks) != 0 ? 0 : Weeks,
                (units & PeriodUnits.Days) != 0 ? 0 : Days,
                (units & PeriodUnits.Hours) != 0 ? 0 : Hours,
                (units & PeriodUnits.Minutes) != 0 ? 0 : Minutes,
                (units & PeriodUnits.Seconds) != 0 ? 0 : Seconds,
                (units & PeriodUnits.Milliseconds) != 0 ? 0 : Milliseconds,
                (units & PeriodUnits.Ticks) != 0 ? 0 : Ticks );
        }

        [Pure]
        public Period Take(PeriodUnits units)
        {
            return new Period(
                (units & PeriodUnits.Years) != 0 ? Years : 0,
                (units & PeriodUnits.Months) != 0 ? Months : 0,
                (units & PeriodUnits.Weeks) != 0 ? Weeks : 0,
                (units & PeriodUnits.Days) != 0 ? Days : 0,
                (units & PeriodUnits.Hours) != 0 ? Hours : 0,
                (units & PeriodUnits.Minutes) != 0 ? Minutes : 0,
                (units & PeriodUnits.Seconds) != 0 ? Seconds : 0,
                (units & PeriodUnits.Milliseconds) != 0 ? Milliseconds : 0,
                (units & PeriodUnits.Ticks) != 0 ? Ticks : 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period operator -(Period a)
        {
            return a.Negate();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period operator +(Period a, Period b)
        {
            return a.Add( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period operator -(Period a, Period b)
        {
            return a.Subtract( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(Period a, Period b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(Period a, Period b)
        {
            return ! a.Equals( b );
        }
    }
}
