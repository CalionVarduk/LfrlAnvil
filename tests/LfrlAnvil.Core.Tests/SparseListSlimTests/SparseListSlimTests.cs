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
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.SparseListSlimTests;

public class SparseListSlimTests : TestsBase
{
    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 8 )]
    [InlineData( 17, 32 )]
    public void Create_ShouldReturnEmptyList(int minCapacity, int expectedCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.Capacity.Should().Be( expectedCapacity );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Fact]
    public void Add_ShouldAddItemToEmptyList()
    {
        var sut = SparseListSlim<string>.Create();

        var result = sut.Add( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "foo" );
            AssertLast( sut, 0, "foo" );
            AssertEnumerator( sut, (0, "foo") );
            AssertSequenceEnumerator( sut, (0, "foo") );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.Add( "x1" ),
            sut.Add( "x2" ),
            sut.Add( "x3" )
        };

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( 0, 1, 2 );
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 2, "x3" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3") );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.Add( "x1" ),
            sut.Add( "x2" ),
            sut.Add( "x3" ),
            sut.Add( "x4" )
        };

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( 0, 1, 2, 3 );
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        var result = new List<int>
        {
            sut.Add( "x1" ),
            sut.Add( "x2" ),
            sut.Add( "x3" ),
            sut.Add( "x4" ),
            sut.Add( "x5" ),
            sut.Add( "x6" )
        };

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( 0, 1, 2, 3, 4, 5 );
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 5, "x6" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4"), (4, "x5"), (5, "x6") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4"), (4, "x5"), (5, "x6") );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsToEmptyListAtCorrectPositions_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Remove( 0 );
        sut.Remove( 1 );
        sut.Remove( 2 );

        var result = new List<int>
        {
            sut.Add( "x4" ),
            sut.Add( "x5" ),
            sut.Add( "x6" ),
            sut.Add( "x7" ),
            sut.Add( "x8" )
        };

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( 2, 1, 0, 3, 4 );
            sut.Count.Should().Be( 5 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 2, "x4" );
            AssertLast( sut, 4, "x8" );
            AssertEnumerator( sut, (2, "x4"), (1, "x5"), (0, "x6"), (3, "x7"), (4, "x8") );
            AssertSequenceEnumerator( sut, (0, "x6"), (1, "x5"), (2, "x4"), (3, "x7"), (4, "x8") );
        }
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 1, "x2" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void GetRefOrAddDefault_ShouldReturnExistingValue_WhenIndexIsOccupied(int index, string expected)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        ref var result = ref sut.GetRefOrAddDefault( index, out var exists )!;

        using ( new AssertionScope() )
        {
            exists.Should().BeTrue();
            result.Should().Be( expected );
            sut.Count.Should().Be( 4 );
        }
    }

    [Theory]
    [InlineData( 0, 4 )]
    [InlineData( 2, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 8 )]
    [InlineData( 10, 16 )]
    public void GetRefOrAddDefault_ShouldAddItemToEmptyList_WhenIndexIsNotOccupied(int index, int expectedCapacity)
    {
        var sut = SparseListSlim<string>.Create();

        ref var result = ref sut.GetRefOrAddDefault( index, out var exists )!;

        using ( new AssertionScope() )
        {
            exists.Should().BeFalse();
            result.Should().BeNull();
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( expectedCapacity );
            AssertFirst( sut, index, result );
            AssertLast( sut, index, result );
            AssertEnumerator( sut, (index, result) );
            AssertSequenceEnumerator( sut, (index, result) );
        }
    }

    [Theory]
    [InlineData( 0, 4 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 4 )]
    [InlineData( 4, 8 )]
    public void GetRefOrAddDefault_ShouldAddItemToEmptyList_WhenIndexIsNotOccupied_AfterRemoval(int index, int expectedCapacity)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Remove( 2 );
        sut.Remove( 1 );
        sut.Remove( 0 );

        ref var result = ref sut.GetRefOrAddDefault( index, out var exists )!;

        using ( new AssertionScope() )
        {
            exists.Should().BeFalse();
            result.Should().BeNull();
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( expectedCapacity );
            AssertFirst( sut, index, result );
            AssertLast( sut, index, result );
            AssertEnumerator( sut, (index, result) );
            AssertSequenceEnumerator( sut, (index, result) );
        }
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldAddItemsToNonEmptyList_WhenIndexIsNotOccupied_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 0 );
        sut.Remove( 3 );
        sut.Remove( 2 );

        ref var value = ref sut.GetRefOrAddDefault( 6, out _ );
        value = "x6";
        value = ref sut.GetRefOrAddDefault( 0, out _ );
        value = "x7";
        value = ref sut.GetRefOrAddDefault( 3, out _ );
        value = "x8";
        value = ref sut.GetRefOrAddDefault( 2, out _ );
        value = "x9";
        value = ref sut.GetRefOrAddDefault( 5, out _ );
        value = "x10";

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 7 );
            sut.Capacity.Should().Be( 8 );
            AssertFirst( sut, 1, "x2" );
            AssertLast( sut, 5, "x10" );
            AssertEnumerator( sut, (1, "x2"), (4, "x5"), (6, "x6"), (0, "x7"), (3, "x8"), (2, "x9"), (5, "x10") );
            AssertSequenceEnumerator( sut, (0, "x7"), (1, "x2"), (2, "x9"), (3, "x8"), (4, "x5"), (5, "x10"), (6, "x6") );
        }
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldAddItemsToList_WhenIndexIsNotOccupied_FollowedByItemsAddition()
    {
        var sut = SparseListSlim<string>.Create();
        sut.TryAdd( 3, "x1" );

        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 5 );
            sut.Capacity.Should().Be( 8 );
            AssertFirst( sut, 3, "x1" );
            AssertLast( sut, 4, "x5" );
            AssertEnumerator( sut, (3, "x1"), (0, "x2"), (1, "x3"), (2, "x4"), (4, "x5") );
            AssertSequenceEnumerator( sut, (0, "x2"), (1, "x3"), (2, "x4"), (3, "x1"), (4, "x5") );
        }
    }

    [Fact]
    public void GetRefOrAddDefault_ShouldThrowArgumentOutOfRangeException_WhenIndexIsLessThanZero()
    {
        var sut = SparseListSlim<string>.Create();
        var action = Lambda.Of( () => _ = sut.GetRefOrAddDefault( -1, out _ ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryAdd_ShouldAddItem_WhenIndexIsNotOccupied()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Remove( 2 );

        var result = sut.TryAdd( 2, "x5" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 2, "x5" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (3, "x4"), (2, "x5") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x5"), (3, "x4") );
        }
    }

    [Fact]
    public void TryAdd_ShouldDoNothing_WhenIndexIsOccupied()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var result = sut.TryAdd( 2, "x5" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void Remove_ShouldDoNothing_WhenIndexIsNotOccupied(int index)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Remove( index );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 8 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveFirstItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 0 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 1, "x2" );
            AssertLast( sut, 2, "x3" );
            AssertEnumerator( sut, (1, "x2"), (2, "x3") );
            AssertSequenceEnumerator( sut, (1, "x2"), (2, "x3") );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveLastItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 2 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x2" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveMiddleItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 1 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 2, "x3" );
            AssertEnumerator( sut, (0, "x1"), (2, "x3") );
            AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3") );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveOnlyItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );

        var result = sut.Remove( 0 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void Remove_WithRemoved_ShouldDoNothing_WhenIndexIsNotOccupied(int index)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Remove( index, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 8 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3"), (3, "x4") );
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveFirstItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 0, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( "x1" );
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 1, "x2" );
            AssertLast( sut, 2, "x3" );
            AssertEnumerator( sut, (1, "x2"), (2, "x3") );
            AssertSequenceEnumerator( sut, (1, "x2"), (2, "x3") );
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveLastItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 2, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( "x3" );
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x2" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") );
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveMiddleItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        var result = sut.Remove( 1, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( "x2" );
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 2, "x3" );
            AssertEnumerator( sut, (0, "x1"), (2, "x3") );
            AssertSequenceEnumerator( sut, (0, "x1"), (2, "x3") );
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveOnlyItem()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );

        var result = sut.Remove( 0, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( "x1" );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, true )]
    [InlineData( 1, false )]
    [InlineData( 2, true )]
    [InlineData( 3, true )]
    [InlineData( 4, false )]
    [InlineData( 5, false )]
    [InlineData( 8, false )]
    [InlineData( 9, false )]
    public void Contains_ShouldReturnTrue_WhenIndexIsOccupied(int index, bool expected)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.Contains( index );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void GetNode_ShouldReturnNull_WhenIndexIsNotOccupied(int index)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.GetNode( index );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 0, "x1", null, 2, "[0]: x1" )]
    [InlineData( 2, "x3", 0, 3, "[2]: x3" )]
    [InlineData( 3, "x4", 2, null, "[3]: x4" )]
    public void GetNode_ShouldReturnNode_WhenIndexIsOccupied(
        int index,
        string expectedValue,
        int? expectedPrev,
        int? expectedNext,
        string expectedString)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        var result = sut.GetNode( index );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.Value.Index.Should().Be( index );
            result.Value.Value.Should().Be( expectedValue );
            (result.Value.Prev?.Index).Should().Be( expectedPrev );
            (result.Value.Next?.Index).Should().Be( expectedNext );
            result.ToString().Should().Be( expectedString );
        }
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 16 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Remove( 0 );
        sut.Remove( 1 );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 16 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_WhenListIsNotEmpty()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne_AfterRemoval(int minCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "foo" );
        sut.Remove( 0 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenListIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
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
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "foo" );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "foo" );
            AssertLast( sut, 0, "foo" );
            AssertEnumerator( sut, (0, "foo") );
            AssertSequenceEnumerator( sut, (0, "foo") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Remove( 0 );
        sut.Remove( 1 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x2" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Remove( 3 );
        sut.Remove( 2 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x2" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity_AfterRemoval()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );
        sut.Remove( 5 );
        sut.Remove( 4 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldDoNothing_WhenMaxOccupiedIndexDoesNotAllowToReduceCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.TryAdd( 10, "x2" );
        sut.Add( "x3" );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 16 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x3" );
            AssertEnumerator( sut, (0, "x1"), (10, "x2"), (1, "x3") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x3"), (10, "x2") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenMaxOccupiedIndexLimitsCapacityReduction()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.TryAdd( 6, "x2" );
        sut.Add( "x3" );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x3" );
            AssertEnumerator( sut, (0, "x1"), (6, "x2"), (1, "x3") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x3"), (6, "x2") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenMaxOccupiedIndexLimitsCapacityReduction_FollowedByItemsAddition()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 16 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );
        sut.Add( "x7" );
        sut.Remove( 1 );
        sut.Remove( 3 );
        sut.Remove( 5 );
        sut.Remove( 4 );

        sut.ResetCapacity( minCapacity: 4 );

        sut.Add( "x8" );
        sut.Add( "x9" );
        sut.Add( "x10" );
        sut.Add( "x11" );
        sut.Add( "x12" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 8 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 7, "x12" );
            AssertEnumerator( sut, (0, "x1"), (2, "x3"), (6, "x7"), (1, "x8"), (3, "x9"), (4, "x10"), (5, "x11"), (7, "x12") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x8"), (2, "x3"), (3, "x9"), (4, "x10"), (5, "x11"), (6, "x7"), (7, "x12") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenListIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeTrue();
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
            AssertEnumerator( sut );
            AssertSequenceEnumerator( sut );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 1, "x2" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = SparseListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 0, "x1" );
            AssertLast( sut, 3, "x4" );
            AssertEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
            AssertSequenceEnumerator( sut, (0, "x1"), (1, "x2"), (2, "x3"), (3, "x4") );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_FollowedByItemsAddition()
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Remove( 2 );
        sut.Remove( 1 );
        sut.Remove( 0 );

        sut.ResetCapacity( minCapacity: 16 );

        sut.Add( "x5" );
        sut.Add( "x6" );
        sut.Add( "x7" );
        sut.Add( "x8" );
        sut.Add( "x9" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 16 );
            sut.IsEmpty.Should().BeFalse();
            AssertFirst( sut, 3, "x4" );
            AssertLast( sut, 5, "x9" );
            AssertEnumerator( sut, (3, "x4"), (0, "x5"), (1, "x6"), (2, "x7"), (4, "x8"), (5, "x9") );
            AssertSequenceEnumerator( sut, (0, "x5"), (1, "x6"), (2, "x7"), (3, "x4"), (4, "x8"), (5, "x9") );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 8 )]
    [InlineData( 9 )]
    public void Indexer_ShouldReturnNull_WhenIndexIsNotOccupied(int index)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        ref var result = ref sut[index];

        Unsafe.IsNullRef( ref result ).Should().BeTrue();
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void Indexer_ShouldReturnValue_WhenIndexIsOccupied(int index, string expected)
    {
        var sut = SparseListSlim<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Remove( 1 );
        sut.Remove( 4 );

        ref var result = ref sut[index];

        using ( new AssertionScope() )
        {
            Unsafe.IsNullRef( ref result ).Should().BeFalse();
            if ( ! Unsafe.IsNullRef( ref result ) )
                result.Should().Be( expected );
        }
    }

    private static void AssertEnumerator<T>(SparseListSlim<T> source, params (int, T)[] expected)
    {
        var i = 0;
        var result = new KeyValuePair<int, T>[source.Count];
        foreach ( var e in source )
            result[i++] = e;

        result.Should().BeSequentiallyEqualTo( expected.Select( static e => KeyValuePair.Create( e.Item1, e.Item2 ) ) );
    }

    private static void AssertSequenceEnumerator<T>(SparseListSlim<T> source, params (int, T)[] expected)
    {
        var i = 0;
        var result = new KeyValuePair<int, T>[source.Count];
        foreach ( var e in source.Sequential )
            result[i++] = e;

        result.Should().BeSequentiallyEqualTo( expected.Select( static e => KeyValuePair.Create( e.Item1, e.Item2 ) ) );
    }

    private static void AssertFirst<T>(SparseListSlim<T> source, int index, T value)
    {
        source.First.Should().NotBeNull();
        if ( source.First is null )
            return;

        source.First.Value.Index.Should().Be( index );
        source.First.Value.Value.Should().Be( value );
        source.First.Value.Prev.Should().BeNull();
    }

    private static void AssertLast<T>(SparseListSlim<T> source, int index, T value)
    {
        source.Last.Should().NotBeNull();
        if ( source.Last is null )
            return;

        source.Last.Value.Index.Should().Be( index );
        source.Last.Value.Value.Should().Be( value );
        source.Last.Value.Next.Should().BeNull();
    }
}
