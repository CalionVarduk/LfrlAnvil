namespace LfrlAnvil;

public sealed class DoublyLinkedNode<T>
{
    private T _value;

    public DoublyLinkedNode(T value)
    {
        _value = value;
    }

    public DoublyLinkedNode<T>? Prev { get; private set; }
    public DoublyLinkedNode<T>? Next { get; private set; }

    public T Value
    {
        get => _value;
        set => _value = value;
    }

    public ref T ValueRef => ref _value;

    public void LinkPrev(DoublyLinkedNode<T> other)
    {
        Ensure.IsNull( Prev );
        Ensure.IsNull( other.Next );
        Prev = other;
        other.Next = this;
    }

    public void LinkNext(DoublyLinkedNode<T> other)
    {
        Ensure.IsNull( Next );
        Ensure.IsNull( other.Prev );
        Next = other;
        other.Prev = this;
    }

    public void UnlinkPrev()
    {
        if ( Prev is null )
            return;

        Prev.Next = null;
        Prev = null;
    }

    public void UnlinkNext()
    {
        if ( Next is null )
            return;

        Next.Prev = null;
        Next = null;
    }

    public void Remove()
    {
        if ( Prev is null )
        {
            if ( Next is not null )
            {
                Next.Prev = null;
                Next = null;
            }

            return;
        }

        if ( Next is null )
        {
            Prev.Next = null;
            Prev = null;
            return;
        }

        Prev.Next = Next;
        Next.Prev = Prev;
        Next = null;
        Prev = null;
    }
}
