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

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a strategy for handling <see cref="IBatch{T}"/> element overflow,
/// according to its <see cref="IBatch{T}.QueueSizeLimitHint"/> property.
/// </summary>
public enum BatchQueueOverflowStrategy : byte
{
    /// <summary>
    /// Specifies that when <see cref="IBatch{T}.QueueSizeLimitHint"/> is exceeded,
    /// the <see cref="IBatch{T}"/> will not accept any new elements, until enough of its currently enqueued elements get processed.
    /// </summary>
    DiscardLast = 0,

    /// <summary>
    /// Specifies that when <see cref="IBatch{T}.QueueSizeLimitHint"/> is exceeded,
    /// the <see cref="IBatch{T}"/> will dequeue and discard enough of its current elements to make room for new elements.
    /// </summary>
    DiscardFirst = 1,

    /// <summary>
    /// Specifies that <see cref="IBatch{T}"/> will ignore its <see cref="IBatch{T}.QueueSizeLimitHint"/>
    /// and always allow to add new elements.
    /// </summary>
    Ignore = 2
}
