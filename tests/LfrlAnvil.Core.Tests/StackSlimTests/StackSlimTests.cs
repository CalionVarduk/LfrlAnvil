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
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.StackSlimTests;

public class StackSlimTests : TestsBase
{
    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 8 )]
    [InlineData( 17, 32 )]
    public void Create_ShouldReturnEmptyStack(int minCapacity, int expectedCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.Capacity.Should().Be( expectedCapacity );
        }
    }

    [Fact]
    public void Push_ShouldAddItemToEmptyStack()
    {
        var sut = StackSlim<string>.Create();

        sut.Push( "foo" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "foo" );
        }
    }

    [Fact]
    public void Push_ShouldAddItemsSequentiallyToEmptyStack_BelowCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "x1" );
        sut.Push( "x2" );
        sut.Push( "x3" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x3" );
        }
    }

    [Fact]
    public void Push_ShouldAddItemsSequentiallyToEmptyStack_UpToCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "x1" );
        sut.Push( "x2" );
        sut.Push( "x3" );
        sut.Push( "x4" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x4" );
        }
    }

    [Fact]
    public void Push_ShouldAddItemsSequentiallyToEmptyStack_ExceedingCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "x1" );
        sut.Push( "x2" );
        sut.Push( "x3" );
        sut.Push( "x4" );
        sut.Push( "x5" );
        sut.Push( "x6" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x6" );
        }
    }

    [Fact]
    public void PushRange_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = StackSlim<string>.Create();

        sut.PushRange( ReadOnlySpan<string>.Empty );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void PushRange_ShouldAddItemsToEmptyStack()
    {
        var sut = StackSlim<string>.Create();

        sut.PushRange( new[] { "x1", "x2" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x2" );
        }
    }

    [Fact]
    public void PushRange_ShouldAddItemsSequentiallyToEmptyStack_BelowCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1" } );
        sut.PushRange( new[] { "x2", "x3" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x3" );
        }
    }

    [Fact]
    public void PushRange_ShouldAddItemsSequentiallyToEmptyStack_UpToCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1" } );
        sut.PushRange( new[] { "x2", "x3", "x4" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x4" );
        }
    }

    [Fact]
    public void PushRange_ShouldAddItemsSequentiallyToEmptyStack_ExceedingCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1" } );
        sut.PushRange( new[] { "x2", "x3", "x4", "x5", "x6" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x6" );
        }
    }

    [Fact]
    public void Pop_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        var result = sut.Pop();

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Pop_ShouldRemoveOnlyItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.Push( "foo" );

        var result = sut.Pop();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Pop_ShouldRemoveTopItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.Pop();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x2" );
        }
    }

    [Fact]
    public void TryPop_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        var result = sut.TryPop( out var outResult );

        using ( new AssertionScope() )
        {
            outResult.Should().BeNull();
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void TryPop_ShouldRemoveOnlyItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.Push( "foo" );

        var result = sut.TryPop( out var outResult );

        using ( new AssertionScope() )
        {
            outResult.Should().Be( "foo" );
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void TryPop_ShouldRemoveTopItemFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.TryPop( out var outResult );

        using ( new AssertionScope() )
        {
            outResult.Should().Be( "x3" );
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x2" );
        }
    }

    [Fact]
    public void PopRange_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        var result = sut.PopRange( 1 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void PopRange_ShouldDoNothing_WhenCountIsLessThanOrEqualToZero(int count)
    {
        var sut = StackSlim<string>.Create();
        sut.Push( "foo" );

        var result = sut.PopRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void PopRange_ShouldRemoveAllItemsFromStack(int count)
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.PopRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 3 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void PopRange_ShouldRemoveTopItemsFromStack()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var result = sut.PopRange( 2 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x1" );
        }
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create();

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_AtFullCapacity()
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenStackIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenStackIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.Push( "foo" );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "foo" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenStackIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 16 );
        sut.PushRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x2" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 16 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x4" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenStackIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x2" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.Top().Should().Be( "x4" );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnEmpty_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsMemory();
        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsMemory();

        result.ToArray().Should().BeSequentiallyEqualTo( "x4", "x3", "x2", "x1" );
    }

    [Fact]
    public void AsSpan_ShouldReturnEmpty_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsSpan();
        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectResult()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsSpan();

        result.ToArray().Should().BeSequentiallyEqualTo( "x4", "x3", "x2", "x1" );
    }

    [Theory]
    [InlineData( 0, "x4" )]
    [InlineData( 1, "x3" )]
    [InlineData( 2, "x2" )]
    [InlineData( 3, "x1" )]
    public void Indexer_ShouldReturnCorrectItem(int index, string expected)
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = StackSlim<string>.Create();
        sut.PushRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenStackIsEmpty()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var sut = StackSlim<string>.Create( minCapacity: 4 );
        sut.PushRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x4", "x3", "x2", "x1" );
    }
}
