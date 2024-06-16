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

namespace LfrlAnvil.Reactive.State.Exceptions;

internal static class Resources
{
    internal const string ParentNodeIsDisposed = "Parent node is disposed.";
    internal const string ChildNodeIsDisposed = "Child node is disposed.";
    internal const string ChildNodeAlreadyHasParent = "Child node already has a parent.";
    internal const string ParentNodeCannotRegisterSelf = "Parent node cannot register itself as a child node.";

    internal const string CannotRegisterChildNodesInParentNodeThatAlreadyHasParent =
        "Cannot register child nodes in parent node that already has a parent.";
}
