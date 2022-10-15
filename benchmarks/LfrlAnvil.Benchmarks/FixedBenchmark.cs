using System;
using BenchmarkDotNet.Attributes;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Benchmarks;

[MemoryDiagnoser]
public class FixedBenchmark
{
    private readonly decimal _decimal;
    private readonly double _double;
    private readonly Fixed _fixed;
    private readonly decimal _otherDecimal;
    private readonly double _otherDouble;
    private readonly Fixed _otherFixed;

    public FixedBenchmark()
    {
        var rng = new Random();
        var value = rng.Next() % Fixed.GetScale( 9 );
        var otherValue = rng.Next() % Fixed.GetScale( 9 );
        if ( otherValue == 0 )
            otherValue = 1;

        _fixed = Fixed.CreateRaw( value, 5 );
        _decimal = (decimal)value / Fixed.GetScale( 5 );
        _double = (double)value / Fixed.GetScale( 5 );
        _otherFixed = Fixed.CreateRaw( otherValue, 5 );
        _otherDecimal = (decimal)otherValue / Fixed.GetScale( 5 );
        _otherDouble = (double)otherValue / Fixed.GetScale( 5 );
    }

    [Benchmark]
    public decimal Round_Decimal()
    {
        return Math.Round( _decimal, 5 );
    }

    [Benchmark]
    public double Round_Double()
    {
        return Math.Round( _double, 5 );
    }

    [Benchmark]
    public Fixed Round_Fixed()
    {
        return _fixed.Round( 5 );
    }

    [Benchmark]
    public decimal Floor_Decimal()
    {
        return Math.Floor( _decimal );
    }

    [Benchmark]
    public double Floor_Double()
    {
        return Math.Floor( _double );
    }

    [Benchmark]
    public Fixed Floor_Fixed()
    {
        return _fixed.Floor();
    }

    [Benchmark]
    public decimal Ceiling_Decimal()
    {
        return Math.Ceiling( _decimal );
    }

    [Benchmark]
    public double Ceiling_Double()
    {
        return Math.Ceiling( _double );
    }

    [Benchmark]
    public Fixed Ceiling_Fixed()
    {
        return _fixed.Ceiling();
    }

    [Benchmark]
    public decimal Add_Decimal()
    {
        return _decimal + _otherDecimal;
    }

    [Benchmark]
    public double Add_Double()
    {
        return _double + _otherDouble;
    }

    [Benchmark]
    public Fixed Add_Fixed()
    {
        return _fixed + _otherFixed;
    }

    [Benchmark]
    public decimal Subtract_Decimal()
    {
        return _decimal - _otherDecimal;
    }

    [Benchmark]
    public double Subtract_Double()
    {
        return _double - _otherDouble;
    }

    [Benchmark]
    public Fixed Subtract_Fixed()
    {
        return _fixed - _otherFixed;
    }

    [Benchmark]
    public decimal Multiply_Decimal()
    {
        return _decimal * _otherDecimal;
    }

    [Benchmark]
    public double Multiply_Double()
    {
        return _double * _otherDouble;
    }

    [Benchmark]
    public Fixed Multiply_Fixed()
    {
        return _fixed * _otherFixed;
    }

    [Benchmark]
    public decimal Divide_Decimal()
    {
        return _decimal / _otherDecimal;
    }

    [Benchmark]
    public double Divide_Double()
    {
        return _double / _otherDouble;
    }

    [Benchmark]
    public Fixed Divide_Fixed()
    {
        return _fixed / _otherFixed;
    }

    [Benchmark]
    public decimal Modulo_Decimal()
    {
        return _decimal % _otherDecimal;
    }

    [Benchmark]
    public double Modulo_Double()
    {
        return _double % _otherDouble;
    }

    [Benchmark]
    public Fixed Modulo_Fixed()
    {
        return _fixed % _otherFixed;
    }
}
