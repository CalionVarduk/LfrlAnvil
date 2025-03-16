// Copyright 2025 Łukasz Furlepa
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
using System.Threading.Tasks;

namespace LfrlAnvil.Chrono.Async;

/// <summary>
/// Represents a value task created by a <see cref="ValueTaskDelaySource"/> instance.
/// </summary>
public readonly struct DelayValueTask
{
    private readonly ValueTaskDelaySource.Node? _node;
    private readonly ValueTask<ValueTaskDelayResult> _task;

    internal DelayValueTask(ValueTaskDelaySource.Node? node, in ValueTask<ValueTaskDelayResult> task)
    {
        _node = node;
        _task = task;
    }

    /// <summary>
    /// <see cref="ValueTaskDelaySource"/> instance that created this task.
    /// </summary>
    public ValueTaskDelaySource? Owner => _node?.Source;

    /// <summary>
    /// Configures an awaiter for this task.
    /// </summary>
    /// <param name="continueOnCapturedContext">
    /// <b>true</b> to attempt to marshal the continuation back to the captured context; otherwise, <b>false</b>.
    /// </param>
    /// <returns>The configured awaiter.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
    {
        return new ConfiguredAwaitable( _node, _task.ConfigureAwait( continueOnCapturedContext ) );
    }

    /// <summary>
    /// Returns an awaiter for this <see cref="DelayValueTask"/> instance.
    /// </summary>
    /// <returns>The <see cref="Awaiter"/> for this instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Awaiter GetAwaiter()
    {
        return new Awaiter( _node, _task.ConfigureAwait( true ) );
    }

    /// <summary>
    /// Represents an awaitable type that enables configured awaits on a <see cref="DelayValueTask"/>.
    /// </summary>
    public readonly struct ConfiguredAwaitable
    {
        private readonly ValueTaskDelaySource.Node? _node;
        private readonly ConfiguredValueTaskAwaitable<ValueTaskDelayResult> _awaitable;

        internal ConfiguredAwaitable(ValueTaskDelaySource.Node? node, in ConfiguredValueTaskAwaitable<ValueTaskDelayResult> awaitable)
        {
            _node = node;
            _awaitable = awaitable;
        }

        /// <summary>
        /// Returns an awaiter for this <see cref="ConfiguredAwaitable"/> instance.
        /// </summary>
        /// <returns>The <see cref="Awaiter"/> for this instance.</returns>
        public Awaiter GetAwaiter()
        {
            return new Awaiter( _node, _awaitable );
        }
    }

    /// <summary>
    /// Represents an awaiter for a <see cref="DelayValueTask"/> or <see cref="ConfiguredAwaitable"/>.
    /// </summary>
    public readonly struct Awaiter : ICriticalNotifyCompletion
    {
        private readonly ValueTaskDelaySource.Node? _node;
        private readonly ConfiguredValueTaskAwaitable<ValueTaskDelayResult>.ConfiguredValueTaskAwaiter _base;

        internal Awaiter(ValueTaskDelaySource.Node? node, in ConfiguredValueTaskAwaitable<ValueTaskDelayResult> awaitable)
        {
            _node = node;
            _base = awaitable.GetAwaiter();
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="Awaiter"/> has completed.
        /// </summary>
        public bool IsCompleted => _base.IsCompleted;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation)
        {
            _base.OnCompleted( continuation );
        }

        /// <inheritdoc/>
        public void UnsafeOnCompleted(Action continuation)
        {
            _base.UnsafeOnCompleted( continuation );
        }

        /// <summary>
        /// Gets the result of the <see cref="DelayValueTask"/>.
        /// </summary>
        /// <returns>The result of the <see cref="DelayValueTask"/>.</returns>
        public ValueTaskDelayResult GetResult()
        {
            return _node is null ? _base.GetResult() : _node.GetResult( in _base );
        }
    }
}
