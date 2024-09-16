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
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ListSlimTests;

public class ListSlimTests : TestsBase
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
        var sut = ListSlim<string>.Create( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.Capacity.Should().Be( expectedCapacity );
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_ShouldCopyElementsFromMaterializedSource()
    {
        var source = new[] { "x1", "x2", "x3", "x4", "x5" };
        var sut = ListSlim<string>.Create( source );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 5 );
            sut.IsEmpty.Should().BeFalse();
            sut.Capacity.Should().Be( 8 );
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4", "x5" );
        }
    }

    [Fact]
    public void Create_ShouldCopyElementsFromNonMaterializedSource()
    {
        var source = new[] { "x1", "x2", "x3", "x4", "x5", "x6", "x7" }.Where( (_, i) => i > 0 && i < 6 );
        var sut = ListSlim<string>.Create( source, minCapacity: 16 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 5 );
            sut.IsEmpty.Should().BeFalse();
            sut.Capacity.Should().Be( 16 );
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x2", "x3", "x4", "x5", "x6" );
        }
    }

    [Fact]
    public void Add_ShouldAddItemToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.Add( "foo" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "foo" );
            sut.First().Should().Be( "foo" );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3" );
            sut.First().Should().Be( "x1" );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
        }
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4", "x5", "x6" );
        }
    }

    [Fact]
    public void AddRange_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = ListSlim<string>.Create();

        sut.AddRange( ReadOnlySpan<string>.Empty );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddRange_ShouldAddItemsToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.AddRange( new[] { "x1", "x2" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2" );
        }
    }

    [Fact]
    public void AddRange_ShouldAddItemsSequentiallyToEmptyList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1" } );
        sut.AddRange( new[] { "x2", "x3" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3" );
        }
    }

    [Fact]
    public void AddRange_ShouldAddItemsSequentiallyToEmptyList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1" } );
        sut.AddRange( new[] { "x2", "x3", "x4" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
        }
    }

    [Fact]
    public void AddRange_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1" } );
        sut.AddRange( new[] { "x2", "x3", "x4", "x5", "x6" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4", "x5", "x6" );
        }
    }

    [Fact]
    public void InsertAt_ShouldAddItemToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.InsertAt( 0, "foo" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "foo" );
        }
    }

    [Fact]
    public void InsertAt_ShouldAddItemToList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2" } );

        sut.InsertAt( 1, "x3" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x3", "x2" );
        }
    }

    [Fact]
    public void InsertAt_ShouldAddItemToList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3" } );
        sut.InsertAt( 3, "x4" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
        }
    }

    [Fact]
    public void InsertAt_ShouldAddItemToList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertAt( 2, "x6" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x6", "x3", "x4", "x5" );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void InsertAt_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.InsertAt( index, "x4" ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InsertRangeAt_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = ListSlim<string>.Create();

        sut.InsertRangeAt( 0, ReadOnlySpan<string>.Empty );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsToEmptyList()
    {
        var sut = ListSlim<string>.Create();

        sut.InsertRangeAt( 0, new[] { "x1", "x2" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2" );
        }
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsToList_BelowCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 8 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertRangeAt( 2, new[] { "x6", "x7" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 7 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x6", "x7", "x3", "x4", "x5" );
        }
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsToList_UpToCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 8 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertRangeAt( 5, new[] { "x6", "x7", "x8" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 8 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8" );
        }
    }

    [Fact]
    public void InsertRangeAt_ShouldAddItemsSequentiallyToEmptyList_ExceedingCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 8 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5" } );
        sut.InsertRangeAt( 3, new[] { "x6", "x7", "x8", "x9" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 9 );
            sut.Capacity.Should().Be( 16 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x6", "x7", "x8", "x9", "x4", "x5" );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void InsertRangeAt_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.InsertRangeAt( index, ReadOnlySpan<string>.Empty ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RemoveLast_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create();

        var result = sut.RemoveLast();

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void RemoveLast_ShouldRemoveOnlyItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        var result = sut.RemoveLast();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void RemoveLast_ShouldRemoveLastItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var result = sut.RemoveLast();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2" );
        }
    }

    [Fact]
    public void RemoveLastRange_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create();

        var result = sut.RemoveLastRange( 1 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void RemoveLastRange_ShouldDoNothing_WhenCountIsLessThanOrEqualToZero(int count)
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        var result = sut.RemoveLastRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "foo" );
        }
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void RemoveLastRange_ShouldRemoveAllItemsFromList(int count)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var result = sut.RemoveLastRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 3 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void RemoveLastRange_ShouldRemoveLastItemsFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var result = sut.RemoveLastRange( 2 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1" );
        }
    }

    [Fact]
    public void RemoveAt_ShouldRemoveOnlyItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        sut.RemoveAt( 0 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void RemoveAt_ShouldRemoveFirstItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.RemoveAt( 0 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x2", "x3" );
        }
    }

    [Fact]
    public void RemoveAt_ShouldRemoveMiddleItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.RemoveAt( 2 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x4" );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void RemoveAt_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.RemoveAt( index ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RemoveRangeAt_ShouldRemoveOnlyItemFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.Add( "foo" );

        sut.RemoveRangeAt( 0, 1 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void RemoveRangeAt_ShouldRemoveFirstItemsFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.RemoveRangeAt( 0, 2 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x3" );
        }
    }

    [Fact]
    public void RemoveRangeAt_ShouldRemoveMiddleItemsFromList()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8" } );

        sut.RemoveRangeAt( 2, 3 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 5 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x6", "x7", "x8" );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void RemoveRangeAt_ShouldDoNothing_WhenCountIsLessThanOne(int count)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.RemoveRangeAt( 1, count );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3" );
        }
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1, 3 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 0 )]
    public void RemoveRangeAt_ShouldThrowArgumentOutOfRangeException_WhenIndexOrCountAreOutOfRange(int index, int count)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut.RemoveRangeAt( index, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create();

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_AtFullCapacity()
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenListIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenListIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
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
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.Add( "foo" );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "foo" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenListIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 16 );
        sut.AddRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 16 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenListIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeTrue();
            sut.AsSpan().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.AsSpan().ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnEmpty_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsMemory();
        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsMemory();

        result.ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
    }

    [Fact]
    public void AsSpan_ShouldReturnEmpty_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        var result = sut.AsSpan();
        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void AsSpan_ShouldReturnCorrectResult()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsSpan();

        result.ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 1, "x2" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void Indexer_ShouldReturnCorrectItem(int index, string expected)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = ListSlim<string>.Create();
        sut.AddRange( new[] { "x1", "x2", "x3" } );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenListIsEmpty()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var sut = ListSlim<string>.Create( minCapacity: 4 );
        sut.AddRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
    }
}
