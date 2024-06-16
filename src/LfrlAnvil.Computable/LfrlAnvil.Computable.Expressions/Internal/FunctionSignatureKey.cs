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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct FunctionSignatureKey : IEquatable<FunctionSignatureKey>
{
    internal FunctionSignatureKey(IReadOnlyList<Expression> parameters)
    {
        Parameters = parameters;
    }

    internal IReadOnlyList<Expression> Parameters { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{string.Join( ", ", Parameters.Select( static e => e.Type.GetDebugString() ) )}]";
    }

    [Pure]
    public override int GetHashCode()
    {
        var count = Parameters.Count;

        var hash = Hash.Default.Add( count );
        for ( var i = 0; i < count; ++i )
            hash = hash.Add( Parameters[i].Type );

        return hash.Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is FunctionSignatureKey k && Equals( k );
    }

    [Pure]
    public bool Equals(FunctionSignatureKey other)
    {
        var count = Parameters.Count;
        var otherCount = other.Parameters.Count;

        if ( count != otherCount )
            return false;

        for ( var i = 0; i < count; ++i )
        {
            if ( Parameters[i].Type != other.Parameters[i].Type )
                return false;
        }

        return true;
    }
}
