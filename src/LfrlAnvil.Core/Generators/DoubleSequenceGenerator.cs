// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace LfrlAnvil.Generators;

/// <summary>
/// Represents <see cref="Double"/> sequence generator of values within specified range.
/// </summary>
public class DoubleSequenceGenerator : SequenceGeneratorBase<double>
{
    /// <summary>
    /// Creates a new <see cref="DoubleSequenceGenerator"/> instance that starts with <b>0</b>,
    /// with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>
    /// and with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    public DoubleSequenceGenerator()
        : this( start: 0 ) { }

    /// <summary>
    /// Creates a new <see cref="DoubleSequenceGenerator"/> instance with greatest possible <see cref="SequenceGeneratorBase{T}.Bounds"/>.
    /// </summary>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentException">
    /// When <paramref name="step"/> is equal to <b>0</b> or any of the values is not a finite number.
    /// </exception>
    public DoubleSequenceGenerator(double start, double step = 1)
        : this( new Bounds<double>( double.MinValue, double.MaxValue ), start, step ) { }

    /// <summary>
    /// Creates a new <see cref="DoubleSequenceGenerator"/> instance that starts with
    /// minimum possible value defined by <paramref name="bounds"/>, with <see cref="SequenceGeneratorBase{T}.Step"/> equal to <b>1</b>.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    public DoubleSequenceGenerator(Bounds<double> bounds)
        : this( bounds, start: bounds.Min ) { }

    /// <summary>
    /// Creates a new <see cref="DoubleSequenceGenerator"/> instance.
    /// </summary>
    /// <param name="bounds">Range of values that can be generated.</param>
    /// <param name="start">Next value to generate.</param>
    /// <param name="step">Difference between two consecutively generated values. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bounds"/> do not contain <paramref name="start"/>.</exception>
    /// <exception cref="ArgumentException">
    /// When <paramref name="step"/> is equal to <b>0</b> or any of the values is not a finite number.
    /// </exception>
    public DoubleSequenceGenerator(Bounds<double> bounds, double start, double step = 1)
        : base( bounds, start, step )
    {
        Ensure.False( double.IsNaN( bounds.Min ) );
        Ensure.NotEquals( bounds.Min, double.NegativeInfinity );
        Ensure.NotEquals( bounds.Min, double.PositiveInfinity );
        Ensure.False( double.IsNaN( bounds.Max ) );
        Ensure.NotEquals( bounds.Max, double.NegativeInfinity );
        Ensure.NotEquals( bounds.Max, double.PositiveInfinity );
        Ensure.False( double.IsNaN( step ) );
        Ensure.NotEquals( step, double.NegativeInfinity );
        Ensure.NotEquals( step, double.PositiveInfinity );
        Ensure.NotEquals( step, 0 );
    }

    /// <inheritdoc />
    protected sealed override double AddStep(double value)
    {
        var result = value + Step;
        if ( result.Equals( value ) )
            throw new OverflowException();

        return result;
    }
}
