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

using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Internal;

/// <inheritdoc cref="IDateTimeProvider" />
public abstract class DateTimeProviderBase : IDateTimeProvider
{
    /// <summary>
    /// Creates a new <see cref="DateTimeProviderBase"/> instance.
    /// </summary>
    /// <param name="kind">Specifies the resulting <see cref="DateTimeKind"/> of created instances.</param>
    protected DateTimeProviderBase(DateTimeKind kind)
    {
        Kind = kind;
    }

    /// <inheritdoc />
    public DateTimeKind Kind { get; }

    /// <inheritdoc />
    public abstract DateTime GetNow();

    [Pure]
    DateTime IGenerator<DateTime>.Generate()
    {
        return GetNow();
    }

    bool IGenerator<DateTime>.TryGenerate(out DateTime result)
    {
        result = GetNow();
        return true;
    }

    [Pure]
    object IGenerator.Generate()
    {
        return GetNow();
    }

    bool IGenerator.TryGenerate(out object result)
    {
        result = GetNow();
        return true;
    }
}
