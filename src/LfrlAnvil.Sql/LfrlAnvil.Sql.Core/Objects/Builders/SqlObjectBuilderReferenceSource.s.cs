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

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Creates instances of <see cref="SqlObjectBuilderReferenceSource{T}"/> type.
/// </summary>
public static class SqlObjectBuilderReferenceSource
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.
    /// </summary>
    /// <param name="object">Referencing object.</param>
    /// <param name="property">
    /// Optional name of the referencing property of <see cref="SqlObjectBuilderReferenceSource{T}.Object"/>. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderReferenceSource<SqlObjectBuilder> Create(SqlObjectBuilder @object, string? property = null)
    {
        return new SqlObjectBuilderReferenceSource<SqlObjectBuilder>( @object, property );
    }
}
