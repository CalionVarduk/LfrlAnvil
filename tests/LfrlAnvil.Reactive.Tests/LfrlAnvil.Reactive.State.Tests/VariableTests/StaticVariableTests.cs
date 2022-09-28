using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public class StaticVariableTests : TestsBase
{
    [Fact]
    public void Create_WithInitialValue_ShouldReturnVariableWithValueEqualToInitialValueAndDefaultComparer()
    {
        var initialValue = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( initialValue );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndComparer_ShouldReturnVariableWithValueEqualToInitialValue()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, comparer );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndValue_ShouldReturnVariableWithDefaultComparer()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndValueAndComparer_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value, comparer );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndErrorsValidator_ShouldReturnVariableWithValueEqualToInitialValueAndDefaultComparer()
    {
        var initialValue = Fixture.Create<int>();
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndComparerAndErrorsValidator_ShouldReturnVariableWithValueEqualToInitialValue()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, comparer, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndValueAndErrorsValidator_ShouldReturnVariableWithDefaultComparer()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, value, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndValidators_ShouldReturnVariableWithValueEqualToInitialValueAndDefaultComparer()
    {
        var initialValue = Fixture.Create<int>();
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( initialValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndValueAndComparerAndErrorsValidator_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, value, comparer, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndComparerAndValidators_ShouldReturnVariableWithValueEqualToInitialValue()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
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
    public void Create_WithInitialValueAndValueAndValidators_ShouldReturnVariableWithDefaultComparer()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, value, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.InitialValue.Should().Be( initialValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
        }
    }

    [Fact]
    public void Create_WithInitialValueAndValueAndComparerAndValidators_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, value, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
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
}
