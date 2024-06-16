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

using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a named <see cref="SqlObjectBuilder"/> instance.
/// </summary>
/// <param name="Name">Name of the object.</param>
/// <param name="Object">SQL object builder.</param>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly record struct SqlNamedObject<T>(string Name, T Object)
    where T : SqlObjectBuilder;
