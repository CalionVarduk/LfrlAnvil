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
/// Creates instances of <see cref="SqlObjectBuilderReference{T}"/> type.
/// </summary>
public static class SqlObjectBuilderReference
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderReference{T}"/> instance.
    /// </summary>
    /// <param name="source">Underlying reference source.</param>
    /// <param name="target">Target object builder.</param>
    /// <typeparam name="T">SQL object builder type.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderReference{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderReference<T> Create<T>(SqlObjectBuilderReferenceSource<T> source, T target)
        where T : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReference<T>( source, target );
    }
}
