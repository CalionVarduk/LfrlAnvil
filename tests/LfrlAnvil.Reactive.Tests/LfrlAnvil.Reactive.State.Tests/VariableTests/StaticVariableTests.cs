using FluentAssertions.Execution;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public class StaticVariableTests : TestsBase
{
    [Fact]
    public void Create_WithoutValidators_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value, comparer );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.WarningsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
        }
    }

    [Fact]
    public void Create_WithoutValidatorsAndWithInitialValueOnly_ShouldReturnCorrectVariable()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, comparer );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.WarningsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, value, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueOnly_ShouldReturnCorrectVariable()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
        }
    }
}
