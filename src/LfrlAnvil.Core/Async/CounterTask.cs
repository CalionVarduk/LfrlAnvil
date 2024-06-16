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
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

/// <summary>
/// An object that contains an underlying <see cref="Task"/> that gets completed once <see cref="Count"/> reaches the <see cref="Limit"/>.
/// </summary>
public sealed class CounterTask : IDisposable
{
    private readonly TaskCompletionSource _source;
    private int _count;

    /// <summary>
    /// Creates a new <see cref="CounterTask"/> instance.
    /// </summary>
    /// <param name="limit">
    /// An immutable value.
    /// Once <see cref="Count"/> reaches this value, then the underlying <see cref="Task"/> will get completed.
    /// Negative values will be replaced with <b>0</b>.
    /// </param>
    /// <param name="count">
    /// Initial count.
    /// Equal to <b>0</b> by default. If greater than or equal to <paramref name="limit"/>,
    /// then the underlying <see cref="Task"/> wiil get completed immediately.
    /// </param>
    public CounterTask(int limit, int count = 0)
    {
        _source = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        _count = count;
        Limit = Math.Max( limit, 0 );

        if ( _count >= Limit )
        {
            _count = Limit;
            _source.SetResult();
        }
    }

    /// <summary>
    /// <see cref="Count"/> limit. When <see cref="Count"/> reaches this value, then the <see cref="Task"/> will get completed.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Current count. When this reaches the <see cref="Limit"/>, then the <see cref="Task"/> will get completed.
    /// </summary>
    public int Count
    {
        get
        {
            using ( ExclusiveLock.Enter( _source ) )
                return _count;
        }
    }

    /// <summary>
    /// Underlying task that gets completed once <see cref="Count"/> reached the <see cref="Limit"/>
    /// or when this <see cref="CounterTask"/> gets disposed, which will cancel it instead.
    /// </summary>
    public Task Task => _source.Task;

    /// <inheritdoc />
    /// <remarks>If the <see cref="Task"/> isn't completed yet, then it will get cancelled instead.</remarks>
    public void Dispose()
    {
        using ( ExclusiveLock.Enter( _source ) )
        {
            if ( ! _source.Task.IsCompleted )
                _source.SetCanceled();
        }
    }

    /// <summary>
    /// Increases the <see cref="Count"/> by <b>1</b>.
    /// When <see cref="Count"/> reaches the <see cref="Limit"/>, then the <see cref="Task"/> gets completed.
    /// </summary>
    /// <returns>
    /// <b>true</b> when the <see cref="Task"/> is already completed or when the <see cref="Count"/> reaches the <see cref="Limit"/>
    /// and <see cref="Task"/> gets completed because of that, otherwise <b>false</b>.
    /// </returns>
    /// <remarks>Equivalent to <see cref="Add(int)"/> with <b>count</b> equal to <b>1</b>.</remarks>
    public bool Increment()
    {
        using ( ExclusiveLock.Enter( _source ) )
        {
            if ( _source.Task.IsCompleted )
                return true;

            if ( ++_count < Limit )
                return false;

            _source.SetResult();
            return true;
        }
    }

    /// <summary>
    /// Increases the <see cref="Count"/> by the provided <paramref name="count"/>.
    /// When <see cref="Count"/> reaches the <see cref="Limit"/>, then the <see cref="Task"/> gets completed.
    /// </summary>
    /// <param name="count">Value to increase the <see cref="Count"/> by. Negative values will be treated as <b>0</b>.</param>
    /// <returns>
    /// <b>true</b> when the <see cref="Task"/> is already completed or when the <see cref="Count"/> reaches the <see cref="Limit"/>
    /// and <see cref="Task"/> gets completed because of that, otherwise <b>false</b>.
    /// </returns>
    public bool Add(int count)
    {
        if ( count <= 0 )
            return false;

        using ( ExclusiveLock.Enter( _source ) )
        {
            if ( _source.Task.IsCompleted )
                return true;

            _count = unchecked( ( int )Math.Min( unchecked( _count + ( long )count ), Limit ) );
            if ( _count < Limit )
                return false;

            _source.SetResult();
            return true;
        }
    }
}
