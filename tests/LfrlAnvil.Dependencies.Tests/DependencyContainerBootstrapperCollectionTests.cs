using LfrlAnvil.Dependencies.Bootstrapping;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Dependencies.Tests;

public class DependencyContainerBootstrapperCollectionTests : TestsBase
{
    [Fact]
    public void Add_ShouldAddInnerBootstrapper()
    {
        var first = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var second = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var third = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();

        var sut = new DependencyContainerBootstrapperCollection();

        var result = sut.Add( first ).Add( second ).Add( third );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.Count.TestEquals( 3 ),
                result[0].TestRefEquals( first ),
                result[1].TestRefEquals( second ),
                result[2].TestRefEquals( third ),
                result.TestSequence( [ first, second, third ] ) )
            .Go();
    }

    [Fact]
    public void Bootstrap_ShouldCallBootstrapOnEachInnerElementOnce()
    {
        var first = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var second = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var third = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var builder = new DependencyContainerBuilder();

        var sut = new DependencyContainerBootstrapperCollection().Add( first ).Add( second ).Add( third );

        sut.Bootstrap( builder );

        Assertion.All(
                first.TestReceivedCalls( x => x.Bootstrap( builder ), count: 1 ),
                second.TestReceivedCalls( x => x.Bootstrap( builder ), count: 1 ),
                third.TestReceivedCalls( x => x.Bootstrap( builder ), count: 1 ),
                Assertion.CallOrder( () =>
                {
                    first.Bootstrap( builder );
                    second.Bootstrap( builder );
                    third.Bootstrap( builder );
                } ) )
            .Go();
    }

    [Fact]
    public void Bootstrap_ShouldThrowInvalidOperationException_WhenPreviousBootstrapWasNotFinished()
    {
        var builder = new DependencyContainerBuilder();
        var sut = new DependencyContainerBootstrapperCollection();
        sut.Add( sut );

        var action = Lambda.Of( () => sut.Bootstrap( builder ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }
}
