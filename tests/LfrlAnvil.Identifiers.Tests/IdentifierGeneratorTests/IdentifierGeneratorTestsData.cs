using System;
using AutoFixture;
using LfrlAnvil.Chrono;
using Xunit;

namespace LfrlAnvil.Identifiers.Tests.IdentifierGeneratorTests
{
    public class IdentifierGeneratorTestsData
    {
        public static TheoryData<Timestamp, Timestamp, ulong> GetCtorData(IFixture fixture)
        {
            return new TheoryData<Timestamp, Timestamp, ulong>
            {
                { Timestamp.Zero, Timestamp.Zero, 0 },
                { new Timestamp( 1 ), Timestamp.Zero, 0 },
                { new Timestamp( Duration.FromMilliseconds( 1 ).Ticks - 1 ), Timestamp.Zero, 0 },
                { new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ), new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ), 1 },
                { new Timestamp( Duration.FromMilliseconds( 1 ).Ticks + 1 ), new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ), 1 },
                { new Timestamp( Duration.FromMilliseconds( 2 ).Ticks - 1 ), new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ), 1 },
                { new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ), new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ), 2 }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, Timestamp, Timestamp, Timestamp, ulong, int> GetCtorWithParamsData(
            IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, Timestamp, Timestamp, Timestamp, ulong, int>
            {
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks - 1 ),
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) ),
                    1,
                    -1
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) ),
                    2,
                    -1
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2 ).Ticks - 1 ),
                        LowValueBounds = new Bounds<ushort>( 100, 2000 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( Duration.FromMilliseconds( 4 ).Ticks - 1 ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 3 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) ),
                    2,
                    99
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 123, 4567 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) ),
                    3,
                    122
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2 ).Ticks + 1 ),
                        LowValueBounds = new Bounds<ushort>( 1, 1 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) ),
                    3,
                    0
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 4 ).Ticks - 1 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 8 ).Ticks - 1 ),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 6 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 2 ).SubtractTicks( 1 ) ),
                    2,
                    -1
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 4 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 4 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 2 ).SubtractTicks( 1 ) ),
                    3,
                    -1
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1.5 ).Ticks - 1 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks - 1 ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks )
                        .Add( Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) ),
                    2,
                    -1
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 4.5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 4.5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks )
                        .Add( Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) ),
                    4,
                    -1
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 3 ) },
                    Timestamp.Zero,
                    Timestamp.Zero,
                    Timestamp.Zero,
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 3 ).SubtractTicks( 1 ) ),
                    0,
                    -1
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, Identifier> GetGenerateFirstTimeForTheCurrentHighValueData(
            IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, Identifier>
            {
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    new Identifier( 0, 0 )
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 3, 0 )
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 1, 1 ) },
                    Timestamp.Zero,
                    new Identifier( 0, 1 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 3 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7 ).Ticks ),
                    new Identifier( 4, 100 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Timestamp.Zero,
                    new Identifier( 0, 0 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 4 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 3, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 1, 1 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Timestamp.Zero,
                    new Identifier( 0, 1 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 6 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 14 ).Ticks ),
                    new Identifier( 4, 100 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Timestamp.Zero,
                    new Identifier( 0, 0 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 3, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 1, 1 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Timestamp.Zero,
                    new Identifier( 0, 1 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1.5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 3.5 ).Ticks ),
                    new Identifier( 4, 100 )
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, int, Identifier>
            GetGenerateNextTimeForTheCurrentHighValueWithoutExceedingLowValueBoundsData(
                IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, int, Identifier>
            {
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    1,
                    new Identifier( 0, 1 )
                },
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    10,
                    new Identifier( 0, 10 )
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    1,
                    new Identifier( 5, 1 )
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    10,
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    1,
                    new Identifier( 3, 1 )
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    10,
                    new Identifier( 3, 10 )
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 1, 11 ) },
                    Timestamp.Zero,
                    1,
                    new Identifier( 0, 2 )
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 1, 11 ) },
                    Timestamp.Zero,
                    10,
                    new Identifier( 0, 11 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 3 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7 ).Ticks ),
                    1,
                    new Identifier( 4, 101 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 3 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7 ).Ticks ),
                    10,
                    new Identifier( 4, 110 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Timestamp.Zero,
                    1,
                    new Identifier( 0, 1 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Timestamp.Zero,
                    10,
                    new Identifier( 0, 10 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    1,
                    new Identifier( 5, 1 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    10,
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 4 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    1,
                    new Identifier( 3, 1 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 4 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    10,
                    new Identifier( 3, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 1, 11 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Timestamp.Zero,
                    1,
                    new Identifier( 0, 2 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 1, 11 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Timestamp.Zero,
                    10,
                    new Identifier( 0, 11 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 6 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 14 ).Ticks ),
                    1,
                    new Identifier( 4, 101 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 6 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 14 ).Ticks ),
                    10,
                    new Identifier( 4, 110 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Timestamp.Zero,
                    1,
                    new Identifier( 0, 1 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Timestamp.Zero,
                    10,
                    new Identifier( 0, 10 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    1,
                    new Identifier( 5, 1 )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    10,
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    1,
                    new Identifier( 3, 1 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    10,
                    new Identifier( 3, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 1, 11 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Timestamp.Zero,
                    1,
                    new Identifier( 0, 2 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 1, 11 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Timestamp.Zero,
                    10,
                    new Identifier( 0, 11 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1.5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 3.5 ).Ticks ),
                    1,
                    new Identifier( 4, 101 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 1.5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 3.5 ).Ticks ),
                    10,
                    new Identifier( 4, 110 )
                }
            };
        }

        public static TheoryData<Bounds<ushort>, Timestamp, Identifier>
            GetGenerateNextTimeForTheFutureHighValueData(IFixture fixture)
        {
            return new TheoryData<Bounds<ushort>, Timestamp, Identifier>
            {
                {
                    new Bounds<ushort>( ushort.MinValue, ushort.MaxValue ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Identifier( 1, 0 )
                },
                {
                    new Bounds<ushort>( ushort.MinValue, ushort.MaxValue ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 10, 0 )
                },
                {
                    new Bounds<ushort>( 100, 200 ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Identifier( 1, 100 )
                },
                {
                    new Bounds<ushort>( 100, 200 ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 10, 100 )
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, Timestamp, Identifier>
            GetGenerateNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, Timestamp, Identifier>
            {
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 1, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 2, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 1, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 2, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 1, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( Duration.FromMilliseconds( 0.5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 2, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( Duration.FromMilliseconds( 0.5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                    new Identifier( 5, 10 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    Timestamp.Zero,
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 5, 0 )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( Duration.FromMilliseconds( 0.5 ).Ticks ),
                    new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                    new Identifier( 5, 10 )
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp>
            GetGenerateNextTimeForTheCurrentMaxHighValueAndExceedingLowValueBoundsData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp>
            {
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    Timestamp.Zero.Add(
                        Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue
                    },
                    Timestamp.Zero.Add(
                        Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    Timestamp.Zero.Add(
                        Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait
                    },
                    Timestamp.Zero.Add(
                        Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 0, 0 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    Timestamp.Zero.Add(
                        Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 ),
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep
                    },
                    Timestamp.Zero.Add(
                        Duration.FromTicks( (long)((Identifier.MaxHighValue - 1) * (ChronoConstants.TicksPerMillisecond / 2)) ) )
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Identifier, Timestamp> GetGetTimestampData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Identifier, Timestamp>
            {
                {
                    new IdentifierGeneratorParams(),
                    new Identifier( 0, 0 ),
                    Timestamp.Zero
                },
                {
                    new IdentifierGeneratorParams(),
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams(),
                    new Identifier( 10, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ) },
                    new Identifier( 0, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ) },
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 11 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ) },
                    new Identifier( 10, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 20 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Identifier( 0, 0 ),
                    Timestamp.Zero
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 2 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Identifier( 10, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 20 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 20 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Identifier( 0, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 20 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 20 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 22 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 20 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Identifier( 10, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 40 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Identifier( 0, 0 ),
                    Timestamp.Zero
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 0.5 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Identifier( 10, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Identifier( 0, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 5.5 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Identifier( 10, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 10 ).Ticks )
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Duration, ulong> GetCalculateThroughputData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Duration, ulong>
            {
                {
                    new IdentifierGeneratorParams(),
                    Duration.FromTicks( -1 ),
                    0
                },
                {
                    new IdentifierGeneratorParams(),
                    Duration.Zero,
                    0
                },
                {
                    new IdentifierGeneratorParams(),
                    Duration.FromMilliseconds( 1 ),
                    65536
                },
                {
                    new IdentifierGeneratorParams(),
                    Duration.FromMilliseconds( 2 ),
                    131072
                },
                {
                    new IdentifierGeneratorParams(),
                    Duration.FromMilliseconds( 0.5 ),
                    32768
                },
                {
                    new IdentifierGeneratorParams(),
                    Duration.FromMilliseconds( 1.5 ),
                    98304
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 100, 200 ) },
                    Duration.FromMilliseconds( 1 ),
                    101
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 100, 200 ) },
                    Duration.FromMilliseconds( 2 ),
                    202
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 100, 200 ) },
                    Duration.FromMilliseconds( 0.5 ),
                    50
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 100, 200 ) },
                    Duration.FromMilliseconds( 1.5 ),
                    151
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Duration.FromMilliseconds( 2 ),
                    65536
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Duration.FromMilliseconds( 4 ),
                    131072
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Duration.FromMilliseconds( 1 ),
                    32768
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Duration.FromMilliseconds( 3 ),
                    98304
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Duration.FromMilliseconds( 2 ),
                    101
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Duration.FromMilliseconds( 4 ),
                    202
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Duration.FromMilliseconds( 1 ),
                    50
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    Duration.FromMilliseconds( 3 ),
                    151
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Duration.FromMilliseconds( 0.5 ),
                    65536
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Duration.FromMilliseconds( 1 ),
                    131072
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Duration.FromMilliseconds( 0.25 ),
                    32768
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Duration.FromMilliseconds( 0.75 ),
                    98304
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Duration.FromMilliseconds( 0.5 ),
                    101
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Duration.FromMilliseconds( 1 ),
                    202
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Duration.FromMilliseconds( 0.25 ),
                    50
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueBounds = new Bounds<ushort>( 100, 200 ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    Duration.FromMilliseconds( 0.75 ),
                    151
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, Identifier, Timestamp>
            GetStateUpdateGenerateNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, Identifier, Timestamp>
            {
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.AddHighValue,
                        LowValueBounds = new Bounds<ushort>( 0, 3 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 1, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 1 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.BusyWait,
                        LowValueBounds = new Bounds<ushort>( 0, 3 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks )
                },
                {
                    new IdentifierGeneratorParams
                    {
                        LowValueExceededHandlingStrategy = LowValueExceededHandlingStrategy.Sleep,
                        LowValueBounds = new Bounds<ushort>( 0, 3 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                    new Identifier( 5, 0 ),
                    new Timestamp( Duration.FromMilliseconds( 5 ).Ticks )
                }
            };
        }

        public static TheoryData<Bounds<ushort>, int> GetLowValuesLeftAtTheStartOfHighValueData(IFixture fixture)
        {
            return new TheoryData<Bounds<ushort>, int>
            {
                { new Bounds<ushort>( ushort.MinValue, ushort.MaxValue ), 65536 },
                { new Bounds<ushort>( 0, 0 ), 1 },
                { new Bounds<ushort>( 10, 20 ), 11 }
            };
        }

        public static TheoryData<Bounds<ushort>, int, int> GetLowValuesLeftAtTheInstantOfLastIdentifierGenerationData(IFixture fixture)
        {
            return new TheoryData<Bounds<ushort>, int, int>
            {
                { new Bounds<ushort>( ushort.MinValue, ushort.MaxValue ), 1, 65535 },
                { new Bounds<ushort>( ushort.MinValue, ushort.MaxValue ), 10, 65526 },
                { new Bounds<ushort>( 0, 0 ), 1, 0 },
                { new Bounds<ushort>( 10, 20 ), 1, 10 },
                { new Bounds<ushort>( 10, 20 ), 5, 6 },
                { new Bounds<ushort>( 10, 20 ), 10, 1 },
                { new Bounds<ushort>( 10, 20 ), 11, 0 }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, ulong> GetHighValuesLeftAtTheInstantOfGeneratorConstructionData(
            IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, ulong>
            {
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    253402300800000
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    253402300799985
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    253402300799985
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Timestamp.Zero,
                    126701150400000
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    126701150399985
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    126701150399985
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Timestamp.Zero,
                    281474976710655
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    281474976710640
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    281474976710645
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, int, ulong>
            GetHighValuesLeftAtTheInstantOfLastIdentifierGenerationData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, int, ulong>
            {
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    Timestamp.Zero,
                    10,
                    253402300800000
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    10,
                    253402300799985
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    10,
                    253402300799985
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    Timestamp.Zero,
                    11,
                    253402300799999
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    11,
                    253402300799984
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    11,
                    253402300799984
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, ulong> GetHighValuesLeftInTheFutureData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, ulong>
            {
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    253402300799985
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    253402300799985
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    126701150399985
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    126701150399985
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    281474976710640
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    281474976710645
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, ulong> GetValuesLeftAtTheInstantOfGeneratorConstructionData(
            IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, ulong>
            {
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    16606973185228800000
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    16606973185227816960
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    16606973185227816960
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    Timestamp.Zero,
                    8303486592614400000
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    8303486592613416960
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    8303486592613416960
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    Timestamp.Zero,
                    18446744073709486080
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    18446744073708503040
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    18446744073708830720
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    Timestamp.Zero,
                    2787425308800000
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    2787425308799835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    2787425308799835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    Timestamp.Zero,
                    1393712654400000
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    1393712654399835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    1393712654399835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    Timestamp.Zero,
                    3096224743817205
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    3096224743817040
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    3096224743817095
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, int, ulong> GetValuesLeftAtTheInstantOfLastIdentifierGenerationData(
            IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, int, ulong>
            {
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    10,
                    16606973185228799990
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    10,
                    16606973185227816950
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    10,
                    16606973185227816950
                },
                {
                    new IdentifierGeneratorParams(),
                    Timestamp.Zero,
                    11,
                    16606973185228799989
                },
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    11,
                    16606973185227816949
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    11,
                    16606973185227816949
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    Timestamp.Zero,
                    10,
                    2787425308799990
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    10,
                    2787425308799825
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    10,
                    2787425308799825
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    Timestamp.Zero,
                    11,
                    2787425308799989
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    11,
                    2787425308799824
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    11,
                    2787425308799824
                }
            };
        }

        public static TheoryData<IdentifierGeneratorParams, Timestamp, ulong> GetValuesLeftInTheFutureData(IFixture fixture)
        {
            return new TheoryData<IdentifierGeneratorParams, Timestamp, ulong>
            {
                {
                    new IdentifierGeneratorParams(),
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    16606973185227816960
                },
                {
                    new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    16606973185227816960
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 2 ) },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    8303486592613416960
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    8303486592613416960
                },
                {
                    new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 0.5 ) },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    18446744073708503040
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    18446744073708830720
                },
                {
                    new IdentifierGeneratorParams { LowValueBounds = new Bounds<ushort>( 10, 20 ) },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    2787425308799835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 5 ).Ticks ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 15 ).Ticks ),
                    2787425308799835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    1393712654399835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 10 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 2 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 30 ).Ticks ),
                    1393712654399835
                },
                {
                    new IdentifierGeneratorParams
                    {
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    3096224743817040
                },
                {
                    new IdentifierGeneratorParams
                    {
                        BaseTimestamp = new Timestamp( Duration.FromMilliseconds( 2.5 ).Ticks ),
                        TimeEpsilon = Duration.FromMilliseconds( 0.5 ),
                        LowValueBounds = new Bounds<ushort>( 10, 20 )
                    },
                    new Timestamp( Duration.FromMilliseconds( 7.5 ).Ticks ),
                    3096224743817095
                }
            };
        }
    }
}
