using FluentAssertions.Execution;
using LfrlAnvil.Dependencies.Bootstrapping;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Dependencies.Tests.DependencyContainerBootstrappingTests;

public class DependencyContainerBootstrapperCollectionTests : TestsBase
{
    [Fact]
    public void Add_ShouldAddInnerBootstrapper()
    {
        var first = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var second = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var third = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();

        var sut = new DependencyContainerBootstrapperCollection();

        var result = sut
            .Add( first )
            .Add( second )
            .Add( third );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Count.Should().Be( 3 );
            result[0].Should().BeSameAs( first );
            result[1].Should().BeSameAs( second );
            result[2].Should().BeSameAs( third );
            result.Should().BeSequentiallyEqualTo( first, second, third );
        }
    }

    [Fact]
    public void Bootstrap_ShouldCallBootstrapOnEachInnerElementOnce()
    {
        var first = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var second = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var third = Substitute.For<IDependencyContainerBootstrapper<DependencyContainerBuilder>>();
        var builder = new DependencyContainerBuilder();

        var sut = new DependencyContainerBootstrapperCollection()
            .Add( first )
            .Add( second )
            .Add( third );

        sut.Bootstrap( builder );

        using ( new AssertionScope() )
        {
            first.VerifyCalls().Received( x => x.Bootstrap( builder ), 1 );
            second.VerifyCalls().Received( x => x.Bootstrap( builder ), 1 );
            third.VerifyCalls().Received( x => x.Bootstrap( builder ), 1 );
            Verify.CallOrder(
                () =>
                {
                    first.Bootstrap( builder );
                    second.Bootstrap( builder );
                    third.Bootstrap( builder );
                } );
        }
    }

    [Fact]
    public void Bootstrap_ShouldThrowInvalidOperationException_WhenPreviousBootstrapWasNotFinished()
    {
        var builder = new DependencyContainerBuilder();
        var sut = new DependencyContainerBootstrapperCollection();
        sut.Add( sut );

        var action = Lambda.Of( () => sut.Bootstrap( builder ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }
}
