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

using System.Data.Common;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a delegate for creating <see cref="ISqlColumnTypeDefinitionProvider"/> instances.
/// </summary>
/// <param name="serverVersion"><see cref="DbConnection.ServerVersion"/>.</param>
/// <param name="dataTypes"><see cref="ISqlDataTypeProvider"/> instance.</param>
/// <typeparam name="TDataTypeProvider">SQL data type provider type.</typeparam>
/// <typeparam name="TResult">SQL column type definition provider type.</typeparam>
[Pure]
public delegate TResult SqlColumnTypeDefinitionProviderCreator<in TDataTypeProvider, out TResult>(
    string serverVersion,
    TDataTypeProvider dataTypes)
    where TDataTypeProvider : ISqlDataTypeProvider
    where TResult : ISqlColumnTypeDefinitionProvider;
