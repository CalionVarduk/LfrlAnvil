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

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents an information about the source of <see cref="IEventListener"/> disposal.
/// </summary>
public enum DisposalSource : byte
{
    /// <summary>
    /// Specifies that the whole event source has been disposed.
    /// </summary>
    EventSource = 0,

    /// <summary>
    /// Specifies that only the event publisher attached to the event listener has been disposed.
    /// </summary>
    Subscriber = 1
}
