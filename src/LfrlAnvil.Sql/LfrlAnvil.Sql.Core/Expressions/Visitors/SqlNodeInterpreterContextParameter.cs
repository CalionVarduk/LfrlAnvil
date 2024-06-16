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

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a single SQL parameter registered in an <see cref="SqlNodeInterpreterContext"/>.
/// </summary>
/// <param name="Name">Parameter's name.</param>
/// <param name="Type">Optional runtime type of this parameter.</param>
/// <param name="Index">Optional 0-based position of this parameter.</param>
public readonly record struct SqlNodeInterpreterContextParameter(string Name, TypeNullability? Type, int? Index);
