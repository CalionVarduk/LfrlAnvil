using LfrlAnvil.Computable.Automata.Extensions;

namespace LfrlAnvil.Computable.Automata.Tests.ExtensionsTests.StateMachineNodeTests;

public class StateMachineNodeExtensionsTests : TestsBase
{
    [Fact]
    public void IsAccept_ShouldReturnTrue_WhenNodeIsMarkedAsAcceptAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( a )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsAccept();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccept_ShouldReturnFalse_WhenNodeIsMarkedAsDeadAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsAccept();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccept_ShouldReturnTrue_WhenNodeIsMarkedAsAccept()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsAccept();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAccept_ShouldReturnFalse_WhenNodeIsMarkedAsInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsAccept();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccept_ShouldReturnFalse_WhenNodeIsMarkedAsDefault()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsAccept();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAccept_ShouldReturnFalse_WhenNodeIsMarkedAsDead()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .AddTransition( a, b, 0 )
            .MarkAsInitial( a )
            .MarkAsAccept( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsAccept();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInitial_ShouldReturnTrue_WhenNodeIsMarkedAsAcceptAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( a )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsInitial();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInitial_ShouldReturnTrue_WhenNodeIsMarkedAsDeadAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsInitial();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInitial_ShouldReturnFalse_WhenNodeIsMarkedAsAccept()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsInitial();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInitial_ShouldReturnTrue_WhenNodeIsMarkedAsInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsInitial();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInitial_ShouldReturnFalse_WhenNodeIsMarkedAsDefault()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsInitial();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInitial_ShouldReturnFalse_WhenNodeIsMarkedAsDead()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .AddTransition( a, b, 0 )
            .MarkAsAccept( a )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsInitial();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDead_ShouldReturnFalse_WhenNodeIsMarkedAsAcceptAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( a )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsDead();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDead_ShouldReturnTrue_WhenNodeIsMarkedAsDeadAndInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsDead();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsDead_ShouldReturnFalse_WhenNodeIsMarkedAsAccept()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsAccept( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsDead();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDead_ShouldReturnFalse_WhenNodeIsMarkedAsInitial()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.IsDead();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDead_ShouldReturnFalse_WhenNodeIsMarkedAsDefault()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsDefault( b )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsDead();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDead_ShouldReturnTrue_WhenNodeIsMarkedAsDead()
    {
        var (a, b) = ("a", "b");
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .SetOptimization( StateMachineOptimizationParams<string>.Minimize( (s1, s2) => s1 + s2 ) )
            .AddTransition( a, b, 0 )
            .MarkAsAccept( a )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[b];

        var result = sut.IsDead();

        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransition_ShouldReturnTrue_WhenNodeHasAnyTransition()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.CanTransition();

        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransition_ShouldReturnFalse_WhenNodeDoesNotHaveAnyTransitions()
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.CanTransition();

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, true )]
    [InlineData( 1, true )]
    [InlineData( 2, false )]
    public void CanTransition_WithInput_ShouldReturnCorrectResult(int input, bool expected)
    {
        var a = "a";
        var builder = new StateMachineBuilder<string, int, string>( Fixture.Create<string>() )
            .AddTransition( a, 0 )
            .AddTransition( a, 1 )
            .MarkAsInitial( a );

        var machine = builder.Build();
        var sut = machine.States[a];

        var result = sut.CanTransition( input );

        result.Should().Be( expected );
    }
}
