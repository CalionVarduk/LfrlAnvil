using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.TreeDictionaryNodeTests;

public abstract class GenericTreeDictionaryNodeExtensionsTests<TKey, TValue> : TestsBase
    where TKey : notnull
{
    [Fact]
    public void GetRoot_ShouldReturnSelf_WhenNodeIsRoot()
    {
        var sut = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), null );
        var result = sut.GetRoot();
        result.Should().Be( sut );
    }

    [Fact]
    public void GetRoot_ShouldReturnCorrectResult_WhenNodeIsRootsChild()
    {
        var parent = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), null );
        var sut = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), parent );

        var result = sut.GetRoot();

        result.Should().Be( parent );
    }

    [Fact]
    public void GetRoot_ShouldReturnCorrectResult_WhenNodeIsRootsIndirectDescendant()
    {
        var parent = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), null );
        var child = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), parent );
        var sut = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), child );

        var result = sut.GetRoot();

        result.Should().Be( parent );
    }

    [Fact]
    public void VisitAncestors_ShouldReturnCorrectResult()
    {
        var a = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), null );
        var b = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var c = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), b );
        var d = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), c );

        var result = d.VisitAncestors();

        result.Should().BeSequentiallyEqualTo( c, b, a );
    }

    [Fact]
    public void VisitDescendants_ShouldReturnCorrectResult_AccordingToBreadthFirstTraversal()
    {
        var a = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), null );
        var b = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var c = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var d = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var e = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), b );
        var f = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), b );
        var g = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), d );
        var h = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), f );
        var i = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), f );
        var j = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), g );
        var k = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), g );

        var result = a.VisitDescendants();

        result.Should().BeSequentiallyEqualTo( b, c, d, e, f, g, h, i, j, k );
    }

    [Fact]
    public void VisitDescendants_WithStopPredicate_ShouldReturnCorrectResult_AccordingToBreadthFirstTraversal()
    {
        var a = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), null );
        var b = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var c = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var d = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), a );
        var e = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), b );
        var f = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), b );
        var g = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), d );
        var h = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), f );
        var i = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), f );
        var j = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), g );
        var k = new TestNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>(), g );
        var nodesToStopAt = new[] { b, g };

        var result = a.VisitDescendants( n => nodesToStopAt.Contains( n ) );

        result.Should().BeSequentiallyEqualTo( b, c, d, g );
    }

    [Fact]
    public void GetRoot_WithTreeDictionaryNode_ShouldReturnSelf_WhenNodeIsRoot()
    {
        var tree = new TreeDictionary<TKey, TValue>();
        var sut = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        sut.SetTree( tree );

        var result = sut.GetRoot();

        result.Should().Be( sut );
    }

    [Fact]
    public void GetRoot_WithTreeDictionaryNode_ShouldReturnCorrectResult_WhenNodeIsRootsChild()
    {
        var tree = new TreeDictionary<TKey, TValue>();
        var parent = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var sut = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        parent.SetTree( tree );
        sut.SetTree( tree );
        sut.SetParent( parent );

        var result = sut.GetRoot();

        result.Should().Be( parent );
    }

    [Fact]
    public void GetRoot_WithTreeDictionaryNode_ShouldReturnCorrectResult_WhenNodeIsRootsIndirectDescendant()
    {
        var tree = new TreeDictionary<TKey, TValue>();
        var parent = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var child = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var sut = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        parent.SetTree( tree );
        child.SetTree( tree );
        sut.SetTree( tree );
        child.SetParent( parent );
        sut.SetParent( child );

        var result = sut.GetRoot();

        result.Should().Be( parent );
    }

    [Fact]
    public void VisitAncestors_WithTreeDictionaryNode_ShouldReturnCorrectResult()
    {
        var tree = new TreeDictionary<TKey, TValue>();
        var a = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var b = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var c = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var d = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        a.SetTree( tree );
        b.SetTree( tree );
        c.SetTree( tree );
        d.SetTree( tree );
        b.SetParent( a );
        c.SetParent( b );
        d.SetParent( c );

        var result = d.VisitAncestors();

        result.Should().BeSequentiallyEqualTo( c, b, a );
    }

    [Fact]
    public void VisitDescendants_WithTreeDictionaryNode_ShouldReturnCorrectResult_AccordingToBreadthFirstTraversal()
    {
        var tree = new TreeDictionary<TKey, TValue>();
        var a = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var b = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var c = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var d = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var e = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var f = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var g = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var h = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var i = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var j = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var k = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        a.SetTree( tree );
        b.SetTree( tree );
        c.SetTree( tree );
        d.SetTree( tree );
        e.SetTree( tree );
        f.SetTree( tree );
        g.SetTree( tree );
        h.SetTree( tree );
        i.SetTree( tree );
        j.SetTree( tree );
        k.SetTree( tree );
        b.SetParent( a );
        c.SetParent( a );
        d.SetParent( a );
        e.SetParent( b );
        f.SetParent( b );
        g.SetParent( d );
        h.SetParent( f );
        i.SetParent( f );
        j.SetParent( g );
        k.SetParent( g );

        var result = a.VisitDescendants();

        result.Should().BeSequentiallyEqualTo( b, c, d, e, f, g, h, i, j, k );
    }

    [Fact]
    public void VisitDescendants_WithTreeDictionaryNode_WithStopPredicate_ShouldReturnCorrectResult_AccordingToBreadthFirstTraversal()
    {
        var tree = new TreeDictionary<TKey, TValue>();
        var a = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var b = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var c = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var d = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var e = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var f = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var g = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var h = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var i = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var j = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var k = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        a.SetTree( tree );
        b.SetTree( tree );
        c.SetTree( tree );
        d.SetTree( tree );
        e.SetTree( tree );
        f.SetTree( tree );
        g.SetTree( tree );
        h.SetTree( tree );
        i.SetTree( tree );
        j.SetTree( tree );
        k.SetTree( tree );
        b.SetParent( a );
        c.SetParent( a );
        d.SetParent( a );
        e.SetParent( b );
        f.SetParent( b );
        g.SetParent( d );
        h.SetParent( f );
        i.SetParent( f );
        j.SetParent( g );
        k.SetParent( g );
        var nodesToStopAt = new[] { b, g };

        var result = a.VisitDescendants( n => nodesToStopAt.Contains( n ) );

        result.Should().BeSequentiallyEqualTo( b, c, d, g );
    }

    [Fact]
    public void CreateTree_ShouldReturnCorrectResult_WhenNodeIsNotLinkedToAnyTree()
    {
        var sut = new TreeDictionaryNode<TKey, TValue>( Fixture.Create<TKey>(), Fixture.Create<TValue>() );
        var result = sut.CreateTree();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 1 );
            result.Root!.Key.Should().Be( sut.Key );
            result.Comparer.Should().Be( EqualityComparer<TKey>.Default );
            result[sut.Key].Should().Be( sut.Value );
        }
    }

    [Fact]
    public void CreateTree_ShouldReturnCorrectResult_WhenNodeIsLinkedToTree()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
        var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );
        var tree = new TreeDictionary<TKey, TValue>( comparer );
        var a = new TreeDictionaryNode<TKey, TValue>( keys[0], Fixture.Create<TValue>() );
        var b = new TreeDictionaryNode<TKey, TValue>( keys[1], Fixture.Create<TValue>() );
        var c = new TreeDictionaryNode<TKey, TValue>( keys[2], Fixture.Create<TValue>() );
        var d = new TreeDictionaryNode<TKey, TValue>( keys[3], Fixture.Create<TValue>() );
        var e = new TreeDictionaryNode<TKey, TValue>( keys[4], Fixture.Create<TValue>() );
        var f = new TreeDictionaryNode<TKey, TValue>( keys[5], Fixture.Create<TValue>() );
        var g = new TreeDictionaryNode<TKey, TValue>( keys[6], Fixture.Create<TValue>() );
        a.SetTree( tree );
        b.SetTree( tree );
        c.SetTree( tree );
        d.SetTree( tree );
        e.SetTree( tree );
        f.SetTree( tree );
        g.SetTree( tree );
        b.SetParent( a );
        c.SetParent( a );
        d.SetParent( b );
        e.SetParent( b );
        f.SetParent( d );
        g.SetParent( e );

        var result = b.CreateTree();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 5 );
            result.Root!.Key.Should().Be( b.Key );
            result.Comparer.Should().Be( comparer );
            result.GetNode( b.Key )!.Children.Select( n => n.Key ).Should().BeSequentiallyEqualTo( d.Key, e.Key );
            result.GetNode( d.Key )!.Children.Select( n => n.Key ).Should().BeSequentiallyEqualTo( f.Key );
            result.GetNode( e.Key )!.Children.Select( n => n.Key ).Should().BeSequentiallyEqualTo( g.Key );
        }
    }
}

internal sealed class TestNode<TKey, TValue> : ITreeDictionaryNode<TKey, TValue>
{
    private readonly List<ITreeDictionaryNode<TKey, TValue>> _children = new List<ITreeDictionaryNode<TKey, TValue>>();

    public TKey Key { get; }
    public TValue Value { get; }
    public ITreeDictionaryNode<TKey, TValue>? Parent { get; }
    public IReadOnlyList<ITreeDictionaryNode<TKey, TValue>> Children => _children;

    ITreeNode<TValue>? ITreeNode<TValue>.Parent => Parent;
    IReadOnlyList<ITreeNode<TValue>> ITreeNode<TValue>.Children => _children;

    internal TestNode(TKey key, TValue value, TestNode<TKey, TValue>? parent)
    {
        Key = key;
        Value = value;
        Parent = parent;
        parent?._children.Add( this );
    }
}
