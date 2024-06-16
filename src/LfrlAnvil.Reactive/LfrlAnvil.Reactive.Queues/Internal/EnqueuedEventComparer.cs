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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues.Internal;

internal sealed class EnqueuedEventComparer<TEvent, TPoint, TPointDelta> : IComparer<EnqueuedEvent<TEvent, TPoint, TPointDelta>>
{
    private readonly IComparer<TPoint> _pointComparer;

    internal EnqueuedEventComparer(IComparer<TPoint> pointComparer)
    {
        _pointComparer = pointComparer;
    }

    [Pure]
    public int Compare(EnqueuedEvent<TEvent, TPoint, TPointDelta> a, EnqueuedEvent<TEvent, TPoint, TPointDelta> b)
    {
        return _pointComparer.Compare( a.DequeuePoint, b.DequeuePoint );
    }
}
