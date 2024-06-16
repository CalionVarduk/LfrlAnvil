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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Pipelines;

/// <summary>
/// Creates instances of <see cref="IPipelineProcessor{TArgs,TResult}"/> type.
/// </summary>
public static class PipelineProcessor
{
    /// <summary>
    /// Creates a new <see cref="IPipelineProcessor{TArgs,TResult}"/> instance from a delegate.
    /// </summary>
    /// <param name="action">Processor's action.</param>
    /// <typeparam name="TArgs">Type of pipeline's input arguments.</typeparam>
    /// <typeparam name="TResult">Type of pipeline's result.</typeparam>
    /// <returns>New <see cref="IPipelineProcessor{TArgs,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IPipelineProcessor<TArgs, TResult> Create<TArgs, TResult>(Action<PipelineContext<TArgs, TResult>> action)
    {
        return new Lambda<TArgs, TResult>( action );
    }

    private sealed class Lambda<TArgs, TResult> : IPipelineProcessor<TArgs, TResult>
    {
        private readonly Action<PipelineContext<TArgs, TResult>> _action;

        internal Lambda(Action<PipelineContext<TArgs, TResult>> action)
        {
            _action = action;
        }

        public void Process(PipelineContext<TArgs, TResult> context)
        {
            _action( context );
        }
    }
}
