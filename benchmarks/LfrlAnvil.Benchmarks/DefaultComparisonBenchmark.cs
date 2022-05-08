using System;
using BenchmarkDotNet.Attributes;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Benchmarks
{
    [MemoryDiagnoser]
    public class DefaultComparisonBenchmark
    {
        private readonly int _valueInt;
        private readonly int? _valueIntNullable;
        private readonly string _valueString;

        public DefaultComparisonBenchmark()
        {
            var rng = new Random();
            _valueInt = rng.Next();
            _valueIntNullable = rng.Next();
            _valueString = Guid.NewGuid().ToString();
        }

        [Benchmark]
        public bool DefaultComparison_Int_Operator()
        {
            return _valueInt == default;
        }

        [Benchmark]
        public bool DefaultComparison_NullableInt_HasValue()
        {
            return ! _valueIntNullable.HasValue;
        }

        [Benchmark]
        public bool DefaultComparison_String_IsNull()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return _valueString is null;
        }

        [Benchmark]
        public bool DefaultComparison_Int_Generic()
        {
            return Generic<int>.IsDefault( _valueInt );
        }

        [Benchmark]
        public bool DefaultComparison_NullableInt_Generic()
        {
            return Generic<int?>.IsDefault( _valueIntNullable );
        }

        [Benchmark]
        public bool DefaultComparison_String_Generic()
        {
            return Generic<string>.IsDefault( _valueString );
        }

        [Benchmark]
        public bool DefaultComparison_Int_IEquatable()
        {
            return _valueInt.Equals( default );
        }

        [Benchmark]
        public bool DefaultComparison_NullableInt_IEquatable()
        {
            return _valueIntNullable.Equals( default );
        }

        [Benchmark]
        public bool DefaultComparison_String_IEquatable()
        {
            return _valueString.Equals( default );
        }
    }
}
