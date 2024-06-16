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

namespace LfrlAnvil.Chrono;

/// <summary>
/// Contains instances of precise <see cref="IDateTimeProvider"/> type.
/// </summary>
public static class PreciseDateTimeProvider
{
    /// <summary>
    /// <see cref="IDateTimeProvider"/> instance that returns precise <see cref="DateTime"/> instances
    /// of <see cref="DateTimeKind.Utc"/> kind, with <see cref="PreciseUtcDateTimeProvider.PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public static readonly IDateTimeProvider Utc = new PreciseUtcDateTimeProvider();

    /// <summary>
    /// <see cref="IDateTimeProvider"/> instance that returns precise <see cref="DateTime"/> instances
    /// of <see cref="DateTimeKind.Local"/> kind, with <see cref="PreciseLocalDateTimeProvider.PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public static readonly IDateTimeProvider Local = new PreciseLocalDateTimeProvider();
}
