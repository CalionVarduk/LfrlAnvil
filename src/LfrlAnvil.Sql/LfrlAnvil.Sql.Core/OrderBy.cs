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

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents an ordering strategy.
/// </summary>
public sealed class OrderBy : Enumeration<OrderBy, OrderBy.Values>
{
    /// <summary>
    /// Represents underlying <see cref="OrderBy"/> values.
    /// </summary>
    public enum Values : byte
    {
        /// <summary>
        /// <see cref="OrderBy.Asc"/> value.
        /// </summary>
        Asc = 0,

        /// <summary>
        /// <see cref="OrderBy.Desc"/> value.
        /// </summary>
        Desc = 1
    }

    /// <summary>
    /// Specifies that the ordering should be in ascending order.
    /// </summary>
    public static readonly OrderBy Asc = new OrderBy( "ASC", Values.Asc );

    /// <summary>
    /// Specifies that the ordering should be in descending order.
    /// </summary>
    public static readonly OrderBy Desc = new OrderBy( "DESC", Values.Desc );

    private OrderBy(string name, Values value)
        : base( name, value ) { }
}
