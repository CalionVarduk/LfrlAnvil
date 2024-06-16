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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Internal;

internal readonly record struct PostgreSqlObjectRename(SqlObjectBuilder Object, string OriginalName, string Name, bool IsPending)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlObjectRename Create(SqlObjectBuilder obj, string originalName)
    {
        return new PostgreSqlObjectRename( obj, originalName, obj.Name, IsPending: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlObjectRename CreateTemporary(PostgreSqlObjectRename @base, string temporaryName)
    {
        return new PostgreSqlObjectRename( @base.Object, temporaryName, @base.Name, IsPending: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlObjectRename Complete()
    {
        return new PostgreSqlObjectRename( Object, OriginalName, Name, IsPending: false );
    }
}
