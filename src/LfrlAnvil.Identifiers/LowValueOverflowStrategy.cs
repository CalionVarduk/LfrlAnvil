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

namespace LfrlAnvil.Identifiers;

/// <summary>
/// Represents possible strategies for resolving low value overflow in <see cref="IdentifierGenerator"/> instances.
/// </summary>
public enum LowValueOverflowStrategy : byte
{
    /// <summary>
    /// Specifies that generator should not generate new identifiers as long as the low value overflow is in progress.
    /// </summary>
    Forbidden = 0,

    /// <summary>
    /// Specifies that generator will increment its high value by <b>1</b> regardless of the current time,
    /// which will reset the low value counter.
    /// </summary>
    AddHighValue = 1,

    /// <summary>
    /// Specifies that generator will <see cref="System.Threading.SpinWait"/> as long as the low value overflow is in progress.
    /// </summary>
    SpinWait = 2,

    /// <summary>
    /// Specifies that generator will <see cref="System.Threading.Thread.Sleep(int)"/> as long as the low value overflow is in progress.
    /// </summary>
    Sleep = 3
}
