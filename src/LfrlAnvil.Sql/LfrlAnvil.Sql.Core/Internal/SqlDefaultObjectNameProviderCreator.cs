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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a delegate for creating <see cref="ISqlDefaultObjectNameProvider"/> instances.
/// </summary>
/// <param name="serverVersion"><see cref="DbConnection.ServerVersion"/>.</param>
/// <param name="defaultSchemaName">Name of the default DB schema.</param>
/// <typeparam name="TResult">SQL default object name provider type.</typeparam>
[Pure]
public delegate TResult SqlDefaultObjectNameProviderCreator<out TResult>(string serverVersion, string defaultSchemaName)
    where TResult : ISqlDefaultObjectNameProvider;
