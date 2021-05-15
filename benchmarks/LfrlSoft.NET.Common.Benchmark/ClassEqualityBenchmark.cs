using BenchmarkDotNet.Attributes;
using LfrlSoft.NET.Common.Internal;
using System;

namespace LfrlSoft.NET.Common.Benchmarks
{
    [MemoryDiagnoser]
    public class ClassEqualityBenchmark
    {
        private readonly string _value1;
        private readonly string _value2;

        public ClassEqualityBenchmark()
        {
            _value1 = Guid.NewGuid().ToString();
            _value2 = Guid.NewGuid().ToString();
        }

        [Benchmark]
        public bool ClassEquality_Operator()
        {
            return _value1 == _value2;
        }

        [Benchmark]
        public bool ClassEquality_IEquatable()
        {
            return _value1.Equals( _value2 );
        }

        [Benchmark]
        public bool ClassEquality_Boxing()
        {
            return (( object )_value1).Equals( _value2 );
        }

        [Benchmark]
        public bool ClassEquality_InternalGeneric()
        {
            return Generic<string>.AreEqual( _value1, _value2 );
        }

        [Benchmark]
        public bool ClassEquality_EqualityFactory()
        {
            return Equality.Create( _value1, _value2 ).Result;
        }

        [Benchmark( Baseline = true )]
        public bool ClassEquality_EqualityCtor()
        {
            return new Equality<string>( _value1, _value2 ).Result;
        }
    }
}
