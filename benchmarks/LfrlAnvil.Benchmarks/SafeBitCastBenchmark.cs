using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;

namespace LfrlAnvil.Benchmarks;

[MemoryDiagnoser]
public class SafeBitCastBenchmark
{
    private readonly ulong _value;

    public SafeBitCastBenchmark()
    {
        var rng = new Random();
        _value = ( ulong )rng.Next();
    }

    [Benchmark]
    public int TypeCodeSwitch()
    {
        return TypeCodeTest<int>.Convert( _value );
    }

    [Benchmark]
    public int Unsafe()
    {
        return UnsafeTest<int>.Convert( _value );
    }

    [Benchmark]
    public int StaticConvert()
    {
        return StaticConvertTest<int>.Convert( _value );
    }

    [Benchmark]
    public int ExprConvert()
    {
        return ExprConvertTest<int>.Convert( _value );
    }

    private struct TypeCodeTest<T>
        where T : struct, IConvertible
    {
        public static readonly TypeCode TypeCode = default( T ).GetTypeCode();

        public static T Convert(ulong value)
        {
            switch ( TypeCode )
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return ( T )(( IConvertible )value).ToType( typeof( T ), null );
                case TypeCode.SByte:
                    return ( T )(( IConvertible )( sbyte )value).ToType( typeof( T ), null );
                case TypeCode.Int16:
                    return ( T )(( IConvertible )( short )value).ToType( typeof( T ), null );
                case TypeCode.Int32:
                    return ( T )(( IConvertible )( int )value).ToType( typeof( T ), null );
                case TypeCode.Int64:
                    return ( T )(( IConvertible )( long )value).ToType( typeof( T ), null );
                default:
                    throw new Exception( "Unsupported type code" );
            }
        }
    }

    private struct UnsafeTest<T>
        where T : struct, IConvertible
    {
        public static T Convert(ulong value)
        {
            return ( T )(( IConvertible )value).ToType( typeof( T ), null );
        }
    }

    private struct StaticConvertTest<T>
        where T : struct, IConvertible
    {
        public static readonly TypeCode TypeCode = default( T ).GetTypeCode();

        public static T Convert(ulong value)
        {
            switch ( TypeCode )
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return ( T )System.Convert.ChangeType( value, TypeCode );
                case TypeCode.SByte:
                    return ( T )System.Convert.ChangeType( ( sbyte )value, TypeCode );
                case TypeCode.Int16:
                    return ( T )System.Convert.ChangeType( ( short )value, TypeCode );
                case TypeCode.Int32:
                    return ( T )System.Convert.ChangeType( ( int )value, TypeCode );
                case TypeCode.Int64:
                    return ( T )System.Convert.ChangeType( ( long )value, TypeCode );
                default:
                    throw new Exception( "Unsupported type code" );
            }
        }
    }

    private struct ExprConvertTest<T>
        where T : struct, IConvertible
    {
        public static readonly Func<ulong, T> Converter;

        static ExprConvertTest()
        {
            var parameter = Expression.Parameter( typeof( ulong ), "value" );
            var convertTo = Expression.Convert( parameter, typeof( T ) );
            Converter = Expression.Lambda<Func<ulong, T>>( convertTo, parameter ).Compile();
        }

        public static T Convert(ulong value)
        {
            return Converter( value );
        }
    }
}
