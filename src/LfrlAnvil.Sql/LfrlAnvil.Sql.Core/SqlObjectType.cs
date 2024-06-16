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

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a type of an SQL object.
/// </summary>
public enum SqlObjectType : byte
{
    /// <summary>
    /// Specifies an unknown type of object.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Specifies a schema object.
    /// </summary>
    Schema = 1,

    /// <summary>
    /// Specifies a table object.
    /// </summary>
    Table = 2,

    /// <summary>
    /// Specifies table's column object.
    /// </summary>
    Column = 3,

    /// <summary>
    /// Specifies table's primary key object.
    /// </summary>
    PrimaryKey = 4,

    /// <summary>
    /// Specifies table's foreign key object.
    /// </summary>
    ForeignKey = 5,

    /// <summary>
    /// Specifies table's check object.
    /// </summary>
    Check = 6,

    /// <summary>
    /// Specifies table's index object.
    /// </summary>
    Index = 7,

    /// <summary>
    /// Specifies a view object.
    /// </summary>
    View = 8,

    /// <summary>
    /// Specifies view's data field object.
    /// </summary>
    ViewDataField = 9
}
