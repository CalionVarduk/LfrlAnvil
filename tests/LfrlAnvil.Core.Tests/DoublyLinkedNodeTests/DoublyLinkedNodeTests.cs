namespace LfrlAnvil.Tests.DoublyLinkedNodeTests;

public class DoublyLinkedNodeTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithoutPrevAndNext()
    {
        var sut = new DoublyLinkedNode<int>( 42 );

        using ( new AssertionScope() )
        {
            sut.Value.Should().Be( 42 );
            sut.ValueRef.Should().Be( 42 );
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Value_Setter_ShouldUpdateValue()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        sut.Value = 123;
        sut.Value.Should().Be( 123 );
    }

    [Fact]
    public void LinkPrev_ShouldLinkOtherNodeAsPrev()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var other = new DoublyLinkedNode<int>( 123 );

        sut.LinkPrev( other );

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeSameAs( other );
            sut.Next.Should().BeNull();
            other.Next.Should().BeSameAs( sut );
            other.Prev.Should().BeNull();
        }
    }

    [Fact]
    public void LinkNext_ShouldLinkOtherNodeAsNext()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var other = new DoublyLinkedNode<int>( 123 );

        sut.LinkNext( other );

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeSameAs( other );
            other.Next.Should().BeNull();
            other.Prev.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void UnlinkPrev_ShouldDoNothing_WhenPrevIsNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var next = new DoublyLinkedNode<int>( 123 );
        sut.LinkNext( next );

        sut.UnlinkPrev();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeSameAs( next );
            next.Prev.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void UnlinkPrev_ShouldUnlinkPrev_WhenPrevIsNotNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var prev = new DoublyLinkedNode<int>( -123 );
        var next = new DoublyLinkedNode<int>( 123 );
        sut.LinkPrev( prev );
        sut.LinkNext( next );

        sut.UnlinkPrev();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeSameAs( next );
            prev.Next.Should().BeNull();
            next.Prev.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void UnlinkNext_ShouldDoNothing_WhenNextIsNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var prev = new DoublyLinkedNode<int>( 123 );
        sut.LinkPrev( prev );

        sut.UnlinkNext();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeSameAs( prev );
            sut.Next.Should().BeNull();
            prev.Next.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void UnlinkNext_ShouldUnlinkNext_WhenNextIsNotNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var prev = new DoublyLinkedNode<int>( -123 );
        var next = new DoublyLinkedNode<int>( 123 );
        sut.LinkPrev( prev );
        sut.LinkNext( next );

        sut.UnlinkNext();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeSameAs( prev );
            sut.Next.Should().BeNull();
            prev.Next.Should().BeSameAs( sut );
            next.Prev.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenPrevAndNextAreNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );

        sut.Remove();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldUnlinkPrev_WhenPrevIsNotNullAndNextIsNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var prev = new DoublyLinkedNode<int>( 123 );
        var prevPrev = new DoublyLinkedNode<int>( 456 );
        prev.LinkPrev( prevPrev );
        sut.LinkPrev( prev );

        sut.Remove();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeNull();
            prev.Prev.Should().BeSameAs( prevPrev );
            prev.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldUnlinkNext_WhenNextIsNotNullAndPrevIsNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var next = new DoublyLinkedNode<int>( 123 );
        var nextNext = new DoublyLinkedNode<int>( 456 );
        next.LinkNext( nextNext );
        sut.LinkNext( next );

        sut.Remove();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeNull();
            next.Prev.Should().BeNull();
            next.Next.Should().BeSameAs( nextNext );
        }
    }

    [Fact]
    public void Remove_ShouldLinkPrevAndNextTogether_WhenPrevAndNextAreNotNull()
    {
        var sut = new DoublyLinkedNode<int>( 42 );
        var prev = new DoublyLinkedNode<int>( -123 );
        var prevPrev = new DoublyLinkedNode<int>( -456 );
        var next = new DoublyLinkedNode<int>( 123 );
        var nextNext = new DoublyLinkedNode<int>( 456 );
        prev.LinkPrev( prevPrev );
        next.LinkNext( nextNext );
        sut.LinkPrev( prev );
        sut.LinkNext( next );

        sut.Remove();

        using ( new AssertionScope() )
        {
            sut.Prev.Should().BeNull();
            sut.Next.Should().BeNull();
            prev.Prev.Should().BeSameAs( prevPrev );
            prev.Next.Should().BeSameAs( next );
            next.Prev.Should().BeSameAs( prev );
            next.Next.Should().BeSameAs( nextNext );
        }
    }
}
