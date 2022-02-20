using System.Linq;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.ConsoleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var normalGen = new TimestampProvider();
            var preciseGen = new PreciseTimestampProvider( ChronoConstants.TicksPerWeek );

            //Task.Delay( TimeSpan.FromSeconds( 10 ) ).Wait();

            var genCount = 100;

            var result = new (Timestamp Normal, Timestamp Precise)[genCount];

            for ( var i = 0; i < genCount; ++i )
            {
                result[i] = (normalGen.GetNow(), preciseGen.GetNow());
            }

            var uniqueNormal = result.Select( r => r.Normal ).ToHashSet();
            var uniquePrecise = result.Select( r => r.Precise ).ToHashSet();
        }
    }
}
