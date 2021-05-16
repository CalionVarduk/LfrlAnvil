using BenchmarkDotNet.Attributes;
using LfrlSoft.NET.Common.Internal;
using System;

namespace LfrlSoft.NET.Common.Benchmarks
{
    [MemoryDiagnoser]
    public class StructEqualityBenchmark
    {
        private readonly int _value1;
        private readonly int _value2;

        public StructEqualityBenchmark()
        {
            var rng = new Random();
            _value1 = rng.Next();
            _value2 = rng.Next();
        }

        [Benchmark]
        public bool StructEquality_Operator()
        {
            return _value1 == _value2;
        }

        [Benchmark]
        public bool StructEquality_IEquatable()
        {
            return _value1.Equals( _value2 );
        }

        [Benchmark]
        public bool StructEquality_Boxing()
        {
            return ((object) _value1).Equals( _value2 );
        }

        [Benchmark]
        public bool StructEquality_InternalGeneric()
        {
            return Generic<int>.AreEqual( _value1, _value2 );
        }

        [Benchmark]
        public bool StructEquality_EqualityFactory()
        {
            return Equality.Create( _value1, _value2 ).Result;
        }

        [Benchmark( Baseline = true )]
        public bool StructEquality_EqualityCtor()
        {
            return new Equality<int>( _value1, _value2 ).Result;
        }
    }
}
