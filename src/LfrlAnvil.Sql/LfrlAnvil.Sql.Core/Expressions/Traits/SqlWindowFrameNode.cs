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

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a window frame.
/// </summary>
public class SqlWindowFrameNode : SqlNodeBase
{
    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameNode"/> instance with <see cref="SqlWindowFrameType.Custom"/> type.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    protected SqlWindowFrameNode(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
        : this( SqlWindowFrameType.Custom, start, end ) { }

    internal SqlWindowFrameNode(SqlWindowFrameType frameType, SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
        : base( SqlNodeType.WindowFrame )
    {
        Assume.IsDefined( frameType );
        FrameType = frameType;
        Start = start;
        End = end;
    }

    /// <summary>
    /// <see cref="SqlWindowFrameType"/> of this frame.
    /// </summary>
    public SqlWindowFrameType FrameType { get; }

    /// <summary>
    /// Beginning <see cref="SqlWindowFrameBoundary"/> of this frame.
    /// </summary>
    public SqlWindowFrameBoundary Start { get; }

    /// <summary>
    /// Ending <see cref="SqlWindowFrameBoundary"/> of this frame.
    /// </summary>
    public SqlWindowFrameBoundary End { get; }
}
