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
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal readonly record struct SqliteColumnRename(SqliteColumnBuilder Column, string OriginalName, string Name, bool IsPending)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnRename Create(SqliteColumnBuilder column, string originalName)
    {
        return new SqliteColumnRename( column, originalName, column.Name, IsPending: true );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnRename CreateTemporary(SqliteColumnRename @base, string temporaryName)
    {
        return new SqliteColumnRename( @base.Column, temporaryName, @base.Name, IsPending: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteColumnRename Complete()
    {
        return new SqliteColumnRename( Column, OriginalName, Name, IsPending: false );
    }
}
