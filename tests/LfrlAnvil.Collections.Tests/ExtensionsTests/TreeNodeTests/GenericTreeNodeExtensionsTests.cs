using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.TreeNodeTests;

public abstract class GenericTreeNodeExtensionsTests<T> : TestsBase
{
    [Fact]
    public void IsRoot_ShouldReturnTrue_WhenNodeDoesNotHaveParent()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.IsRoot();
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoot_ShouldReturnFalse_WhenNodeHasParent()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsRoot();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsLeaf_ShouldReturnTrue_WhenNodeDoesNotHaveChildren()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsLeaf();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsLeaf_ShouldReturnFalse_WhenNodeHasChildren()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );

        var result = sut.IsLeaf();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void GetChildIndex_ShouldReturnCorrectResult_WhenChildExists(int index)
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child1 = new TestNode<T>( Fixture.Create<T>(), sut );
        var child2 = new TestNode<T>( Fixture.Create<T>(), sut );
        var child3 = new TestNode<T>( Fixture.Create<T>(), sut );
        var node = sut.Children[index];

        var result = sut.GetChildIndex( node );

        result.Should().Be( index );
    }

    [Fact]
    public void GetChildIndex_ShouldReturnMinusOne_WhenChildDoesNotExist()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var node = new TestNode<T>( Fixture.Create<T>(), null );

        var result = sut.GetChildIndex( node );

        result.Should().Be( -1 );
    }

    [Fact]
    public void IsChildOf_ShouldReturnTrue_WhenComparedWithNodesParent()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsChildOf( parent );

        result.Should().BeTrue();
    }

    [Fact]
    public void IsChildOf_ShouldReturnFalse_WhenComparedWithSelf()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.IsChildOf( sut );
        result.Should().BeFalse();
    }

    [Fact]
    public void IsChildOf_ShouldReturnFalse_WhenComparedWithNodesIndirectAncestor()
    {
        var ancestor = new TestNode<T>( Fixture.Create<T>(), null );
        var parent = new TestNode<T>( Fixture.Create<T>(), ancestor );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsChildOf( ancestor );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsChildOf_ShouldReturnFalse_WhenComparedWithNodesChild()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );

        var result = sut.IsChildOf( child );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsChildOf_ShouldReturnFalse_WhenComparedWithNodesIndirectDescendant()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );
        var descendant = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.IsChildOf( descendant );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsParentOf_ShouldReturnFalse_WhenComparedWithNodesParent()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsParentOf( parent );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsParentOf_ShouldReturnFalse_WhenComparedWithSelf()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.IsParentOf( sut );
        result.Should().BeFalse();
    }

    [Fact]
    public void IsParentOf_ShouldReturnFalse_WhenComparedWithNodesIndirectAncestor()
    {
        var ancestor = new TestNode<T>( Fixture.Create<T>(), null );
        var parent = new TestNode<T>( Fixture.Create<T>(), ancestor );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsParentOf( ancestor );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsParentOf_ShouldReturnTrue_WhenComparedWithNodesChild()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );

        var result = sut.IsParentOf( child );

        result.Should().BeTrue();
    }

    [Fact]
    public void IsParentOf_ShouldReturnFalse_WhenComparedWithNodesIndirectDescendant()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );
        var descendant = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.IsParentOf( descendant );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAncestorOf_ShouldReturnFalse_WhenComparedWithNodesParent()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsAncestorOf( parent );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAncestorOf_ShouldReturnFalse_WhenComparedWithSelf()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.IsAncestorOf( sut );
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAncestorOf_ShouldReturnFalse_WhenComparedWithNodesIndirectAncestor()
    {
        var ancestor = new TestNode<T>( Fixture.Create<T>(), null );
        var parent = new TestNode<T>( Fixture.Create<T>(), ancestor );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsAncestorOf( ancestor );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAncestorOf_ShouldReturnTrue_WhenComparedWithNodesChild()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );

        var result = sut.IsAncestorOf( child );

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAncestorOf_ShouldReturnTrue_WhenComparedWithNodesIndirectDescendant()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );
        var descendant = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.IsAncestorOf( descendant );

        result.Should().BeTrue();
    }

    [Fact]
    public void IsDescendantOf_ShouldReturnTrue_WhenComparedWithNodesParent()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsDescendantOf( parent );

        result.Should().BeTrue();
    }

    [Fact]
    public void IsDescendantOf_ShouldReturnFalse_WhenComparedWithSelf()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.IsDescendantOf( sut );
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDescendantOf_ShouldReturnTrue_WhenComparedWithNodesIndirectAncestor()
    {
        var ancestor = new TestNode<T>( Fixture.Create<T>(), null );
        var parent = new TestNode<T>( Fixture.Create<T>(), ancestor );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.IsDescendantOf( ancestor );

        result.Should().BeTrue();
    }

    [Fact]
    public void IsDescendantOf_ShouldReturnFalse_WhenComparedWithNodesChild()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );

        var result = sut.IsDescendantOf( child );

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDescendantOf_ShouldReturnFalse_WhenComparedWithNodesIndirectDescendant()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), sut );
        var descendant = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.IsDescendantOf( descendant );

        result.Should().BeFalse();
    }

    [Fact]
    public void GetLevel_ShouldReturnZero_ForRoot()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.GetLevel();
        result.Should().Be( 0 );
    }

    [Fact]
    public void GetLevel_ShouldReturnOne_ForRootsChild()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.GetLevel();

        result.Should().Be( 1 );
    }

    [Fact]
    public void GetLevel_ShouldReturnCorrectResult_ForRootsIndirectDescendant()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), parent );
        var sut = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.GetLevel();

        result.Should().Be( 2 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnZero_ForRoot()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.GetLevel( sut );
        result.Should().Be( 0 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnOne_ForRootsChild()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.GetLevel( parent );

        result.Should().Be( 1 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnZero_ForRootsChild_WhenRootsChildIsSetAsRoot()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.GetLevel( sut );

        result.Should().Be( 0 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnCorrectResult_ForRootsIndirectDescendant()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), parent );
        var sut = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.GetLevel( parent );

        result.Should().Be( 2 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnCorrectResult_ForRootsIndirectDescendant_WhenRootsChildIsSetAsRoot()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), parent );
        var sut = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.GetLevel( child );

        result.Should().Be( 1 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnCorrectResult_ForRootsIndirectDescendant_WhenRootsIndirectDescendantIsSetAsRoot()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), parent );
        var sut = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.GetLevel( sut );

        result.Should().Be( 0 );
    }

    [Fact]
    public void GetLevel_WithRoot_ShouldReturnMinusOne_WhenRootIsNotAnAncestor()
    {
        var root = new TestNode<T>( Fixture.Create<T>(), null );
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.GetLevel( root );

        result.Should().Be( -1 );
    }

    [Fact]
    public void GetRoot_ShouldReturnSelf_WhenNodeIsRoot()
    {
        var sut = new TestNode<T>( Fixture.Create<T>(), null );
        var result = sut.GetRoot();
        result.Should().Be( sut );
    }

    [Fact]
    public void GetRoot_ShouldReturnCorrectResult_WhenNodeIsRootsChild()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var sut = new TestNode<T>( Fixture.Create<T>(), parent );

        var result = sut.GetRoot();

        result.Should().Be( parent );
    }

    [Fact]
    public void GetRoot_ShouldReturnCorrectResult_WhenNodeIsRootsIndirectDescendant()
    {
        var parent = new TestNode<T>( Fixture.Create<T>(), null );
        var child = new TestNode<T>( Fixture.Create<T>(), parent );
        var sut = new TestNode<T>( Fixture.Create<T>(), child );

        var result = sut.GetRoot();

        result.Should().Be( parent );
    }

    [Fact]
    public void VisitAncestors_ShouldReturnCorrectResult()
    {
        var a = new TestNode<T>( Fixture.Create<T>(), null );
        var b = new TestNode<T>( Fixture.Create<T>(), a );
        var c = new TestNode<T>( Fixture.Create<T>(), b );
        var d = new TestNode<T>( Fixture.Create<T>(), c );

        var result = d.VisitAncestors();

        result.Should().BeSequentiallyEqualTo( c, b, a );
    }

    [Fact]
    public void VisitDescendants_ShouldReturnCorrectResult_AccordingToBreadthFirstTraversal()
    {
        var a = new TestNode<T>( Fixture.Create<T>(), null );
        var b = new TestNode<T>( Fixture.Create<T>(), a );
        var c = new TestNode<T>( Fixture.Create<T>(), a );
        var d = new TestNode<T>( Fixture.Create<T>(), a );
        var e = new TestNode<T>( Fixture.Create<T>(), b );
        var f = new TestNode<T>( Fixture.Create<T>(), b );
        var g = new TestNode<T>( Fixture.Create<T>(), d );
        var h = new TestNode<T>( Fixture.Create<T>(), f );
        var i = new TestNode<T>( Fixture.Create<T>(), f );
        var j = new TestNode<T>( Fixture.Create<T>(), g );
        var k = new TestNode<T>( Fixture.Create<T>(), g );

        var result = a.VisitDescendants();

        result.Should().BeSequentiallyEqualTo( b, c, d, e, f, g, h, i, j, k );
    }

    [Fact]
    public void VisitDescendants_WithStopPredicate_ShouldReturnCorrectResult_AccordingToBreadthFirstTraversal()
    {
        var a = new TestNode<T>( Fixture.Create<T>(), null );
        var b = new TestNode<T>( Fixture.Create<T>(), a );
        var c = new TestNode<T>( Fixture.Create<T>(), a );
        var d = new TestNode<T>( Fixture.Create<T>(), a );
        var e = new TestNode<T>( Fixture.Create<T>(), b );
        var f = new TestNode<T>( Fixture.Create<T>(), b );
        var g = new TestNode<T>( Fixture.Create<T>(), d );
        var h = new TestNode<T>( Fixture.Create<T>(), f );
        var i = new TestNode<T>( Fixture.Create<T>(), f );
        var j = new TestNode<T>( Fixture.Create<T>(), g );
        var k = new TestNode<T>( Fixture.Create<T>(), g );
        var nodesToStopAt = new[] { b, g };

        var result = a.VisitDescendants( nodesToStopAt.Contains );

        result.Should().BeSequentiallyEqualTo( b, c, d, g );
    }
}

internal sealed class TestNode<T> : ITreeNode<T>
{
    private readonly List<ITreeNode<T>> _children = new List<ITreeNode<T>>();

    public T Value { get; }
    public ITreeNode<T>? Parent { get; }
    public IReadOnlyList<ITreeNode<T>> Children => _children;

    internal TestNode(T value, TestNode<T>? parent)
    {
        Value = value;
        Parent = parent;
        parent?._children.Add( this );
    }
}
