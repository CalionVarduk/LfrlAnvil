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

using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal.Delegates;

internal readonly struct ExpressionReplacement
{
    internal readonly Expression Original;
    internal readonly Expression Replacement;

    internal ExpressionReplacement(Expression original)
    {
        Original = original;
        Replacement = original;
    }

    internal ExpressionReplacement(Expression original, Expression replacement)
    {
        Original = original;
        Replacement = replacement;
    }

    [Pure]
    public override string ToString()
    {
        return $"{Original} => {Replacement}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExpressionReplacement SetReplacement(Expression replacement)
    {
        return new ExpressionReplacement( Original, replacement );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsMatched(Expression expression)
    {
        return ReferenceEquals( Original, expression );
    }
}
