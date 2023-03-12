using System.Collections.Generic;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = new Variable<int, string>( initialValue, value, comparer, errorsValidator, warningsValidator );

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

            ((IReadOnlyVariable<int, string>)sut).OnValidate.Should().BeSameAs( sut.OnValidate );
            ((IReadOnlyVariable<int>)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IReadOnlyVariable)sut).ValueType.Should().Be( typeof( int ) );
            ((IReadOnlyVariable)sut).ValidationResultType.Should().Be( typeof( string ) );
            ((IReadOnlyVariable)sut).InitialValue.Should().Be( sut.InitialValue );
            ((IReadOnlyVariable)sut).Value.Should().Be( sut.Value );
            ((IReadOnlyVariable)sut).Errors.Should().BeEquivalentTo( sut.Errors );
            ((IReadOnlyVariable)sut).Warnings.Should().BeEquivalentTo( sut.Warnings );
            ((IReadOnlyVariable)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IReadOnlyVariable)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IVariableNode)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).GetChildren().Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new Variable<int, string>( initialValue, value );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.WarningsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
        }
    }

    [Fact]
    public void Ctor_WithInitialValueOnly_ShouldReturnCorrectResult()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = new Variable<int, string>( initialValue, comparer, errorsValidator, warningsValidator );

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

    [Fact]
    public void Ctor_WithInitialValueOnly_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var initialValue = Fixture.Create<int>();
        var sut = new Variable<int, string>( initialValue );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.WarningsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
        }
    }

    [Fact]
    public void Ctor_ShouldSetStateToDefault_WhenInitialValueEqualsValue()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.State.Should().Be( VariableState.Default );
    }

    [Fact]
    public void Ctor_ShouldNotInvokeErrorsValidator()
    {
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );

        var sut = Variable.Create( value, errorsValidator: errorsValidator );

        sut.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeWarningsValidator()
    {
        var value = Fixture.Create<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );

        var sut = Variable.Create( value, warningsValidator: warningsValidator );

        sut.Warnings.Should().BeEmpty();
    }
}
