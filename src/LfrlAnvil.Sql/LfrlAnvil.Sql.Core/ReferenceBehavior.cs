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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a foreign key behavior.
/// </summary>
public sealed class ReferenceBehavior : Enumeration<ReferenceBehavior, ReferenceBehavior.Values>
{
    /// <summary>
    /// Represents underlying <see cref="ReferenceBehavior"/> values.
    /// </summary>
    public enum Values : byte
    {
        /// <summary>
        /// <see cref="ReferenceBehavior.Restrict"/> value.
        /// </summary>
        Restrict = 0,

        /// <summary>
        /// <see cref="ReferenceBehavior.Cascade"/> value.
        /// </summary>
        Cascade = 1,

        /// <summary>
        /// <see cref="ReferenceBehavior.SetNull"/> value.
        /// </summary>
        SetNull = 2,

        /// <summary>
        /// <see cref="ReferenceBehavior.NoAction"/> value.
        /// </summary>
        NoAction = 3
    }

    /// <summary>
    /// Specifies that a foreign key should not allow to delete the referenced record, or to change its identity.
    /// </summary>
    public static readonly ReferenceBehavior Restrict = new ReferenceBehavior( "RESTRICT", Values.Restrict );

    /// <summary>
    /// Specifies that a foreign key should allow to delete the referenced record, or to change its identity,
    /// and should propagate the change to the referencing record.
    /// </summary>
    public static readonly ReferenceBehavior Cascade = new ReferenceBehavior( "CASCADE", Values.Cascade );

    /// <summary>
    /// Specifies that a foreign key should allow to delete the referenced record, or to change its identity,
    /// and should modify the referencing record by setting referencing columns' values to null.
    /// </summary>
    public static readonly ReferenceBehavior SetNull = new ReferenceBehavior( "SET NULL", Values.SetNull );

    /// <summary>
    /// Specifies that a foreign key should do nothing when the referenced record is deleted, or its identity changes.
    /// </summary>
    public static readonly ReferenceBehavior NoAction = new ReferenceBehavior( "NO ACTION", Values.NoAction );

    private ReferenceBehavior(string name, Values value)
        : base( name, value ) { }

    /// <summary>
    /// Returns <see cref="ReferenceBehavior"/> instance associated with the underlying <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <returns><see cref="ReferenceBehavior"/> instance associated with the underlying <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReferenceBehavior GetBehavior(Values value)
    {
        Ensure.IsDefined( value );
        return value switch
        {
            Values.Restrict => Restrict,
            Values.Cascade => Cascade,
            Values.SetNull => SetNull,
            _ => NoAction
        };
    }
}
