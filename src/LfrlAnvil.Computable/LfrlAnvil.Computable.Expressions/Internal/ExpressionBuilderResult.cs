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
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct ExpressionBuilderResult
{
    internal ExpressionBuilderResult(
        Expression bodyExpression,
        ParameterExpression parameterExpression,
        IReadOnlyList<CompilableInlineDelegate> delegates,
        IReadOnlyDictionary<StringSegment, int> argumentIndexes,
        HashSet<StringSegment> discardedArguments)
    {
        BodyExpression = bodyExpression;
        ParameterExpression = parameterExpression;
        Delegates = delegates;
        ArgumentIndexes = argumentIndexes;
        DiscardedArguments = discardedArguments;
    }

    internal Expression BodyExpression { get; }
    internal ParameterExpression ParameterExpression { get; }
    internal IReadOnlyList<CompilableInlineDelegate> Delegates { get; }
    internal IReadOnlyDictionary<StringSegment, int> ArgumentIndexes { get; }
    internal HashSet<StringSegment> DiscardedArguments { get; }
}
