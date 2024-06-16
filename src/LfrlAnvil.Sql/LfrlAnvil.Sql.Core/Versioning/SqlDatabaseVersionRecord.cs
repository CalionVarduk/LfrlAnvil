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

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents information about a single version applied to the database.
/// </summary>
/// <param name="Ordinal">Ordinal number of this version.</param>
/// <param name="Version">Identifier of this version.</param>
/// <param name="Description">Description of this version.</param>
/// <param name="CommitDateUtc">Specifies the date and time at which this version has been applied to the database.</param>
/// <param name="CommitDuration">Specifies the time it took to fully apply this version to the database.</param>
public sealed record SqlDatabaseVersionRecord(
    int Ordinal,
    Version Version,
    string Description,
    DateTime CommitDateUtc,
    TimeSpan CommitDuration
);
