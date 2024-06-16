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
using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Identifiers;

/// <summary>
/// Represents parameters used for <see cref="IdentifierGenerator"/> creation.
/// </summary>
public struct IdentifierGeneratorParams
{
    private Timestamp? _baseTimestamp;
    private Bounds<ushort>? _lowValueBounds;
    private Duration? _timeEpsilon;

    /// <summary>
    /// Gets or sets the current strategy for the resolution of low value overflow.
    /// Equal to <see cref="LowValueOverflowStrategy.Forbidden"/> by default.
    /// </summary>
    public LowValueOverflowStrategy LowValueOverflowStrategy { get; set; }

    /// <summary>
    /// Gets or sets generator's <see cref="IIdentifierGenerator.BaseTimestamp"/>. Equal to <see cref="Timestamp.Zero"/> by default.
    /// </summary>
    public Timestamp BaseTimestamp
    {
        get => _baseTimestamp ?? Timestamp.Zero;
        set => _baseTimestamp = value;
    }

    /// <summary>
    /// Gets or sets generator's <see cref="IdentifierGenerator.LowValueBounds"/>.
    /// Equal to [<see cref="UInt16.MinValue"/>, <see cref="UInt16.MaxValue"/>] by default.
    /// </summary>
    public Bounds<ushort> LowValueBounds
    {
        get => _lowValueBounds ?? Bounds.Create( ushort.MinValue, ushort.MaxValue );
        set => _lowValueBounds = value;
    }

    /// <summary>
    /// Gets or sets generator's <see cref="IdentifierGenerator.TimeEpsilon"/>. Equal to <b>1 millisecond</b> by default.
    /// </summary>
    public Duration TimeEpsilon
    {
        get => _timeEpsilon ?? Duration.FromMilliseconds( 1 );
        set => _timeEpsilon = value;
    }

    /// <summary>
    /// Returns a string representation of this <see cref="IdentifierGeneratorParams"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var baseTimestampText = $"{nameof( BaseTimestamp )}={BaseTimestamp}";
        var timeEpsilonText = $"{nameof( TimeEpsilon )}={TimeEpsilon}";
        var lowValueBoundsText = $"{nameof( LowValueBounds )}={LowValueBounds}";
        var strategyText = $"{nameof( LowValueOverflowStrategy )}={LowValueOverflowStrategy}";
        return $"{{ {baseTimestampText}, {timeEpsilonText}, {lowValueBoundsText}, {strategyText} }}";
    }
}
