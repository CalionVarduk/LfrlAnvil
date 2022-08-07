using System.Linq;
using FluentAssertions.Primitives;

namespace LfrlAnvil.TestExtensions.FluentAssertions;

public class NSubstituteDelegateMockAssertions<T> : ObjectAssertions
    where T : Delegate
{
    internal NSubstituteDelegateMockAssertions(T @delegate)
        : base( @delegate ) { }

    public new T Subject => (T)base.Subject;
    public int CallCount => Subject.ReceivedCalls().Count();

    public NSubstituteDelegateMockCallAssertions<T> CallAt(int callIndex)
    {
        return new NSubstituteDelegateMockCallAssertions<T>( this, callIndex );
    }
}