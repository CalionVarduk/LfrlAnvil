using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public class StaticVariableTests : TestsBase
{
    [Fact]
    public void Create_WithOriginalValue_ShouldReturnVariableWithValueEqualToOriginalValueAndDefaultComparer()
    {
        var originalValue = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( originalValue );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( originalValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndComparer_ShouldReturnVariableWithValueEqualToOriginalValue()
    {
        var originalValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( originalValue, comparer );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( originalValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndValue_ShouldReturnVariableWithDefaultComparer()
    {
        var (originalValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = Variable.WithoutValidators<string>.Create( originalValue, value );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndValueAndComparer_ShouldReturnCorrectVariable()
    {
        var (originalValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( originalValue, value, comparer );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndErrorsValidator_ShouldReturnVariableWithValueEqualToOriginalValueAndDefaultComparer()
    {
        var originalValue = Fixture.Create<int>();
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( originalValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndComparerAndErrorsValidator_ShouldReturnVariableWithValueEqualToOriginalValue()
    {
        var originalValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, comparer, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( originalValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndValueAndErrorsValidator_ShouldReturnVariableWithDefaultComparer()
    {
        var (originalValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, value, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndValidators_ShouldReturnVariableWithValueEqualToOriginalValueAndDefaultComparer()
    {
        var originalValue = Fixture.Create<int>();
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( originalValue );
            sut.Comparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndValueAndComparerAndErrorsValidator_ShouldReturnCorrectVariable()
    {
        var (originalValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, value, comparer, errorsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( value );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndComparerAndValidators_ShouldReturnVariableWithValueEqualToOriginalValue()
    {
        var originalValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
            sut.Value.Should().Be( originalValue );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
        }
    }

    [Fact]
    public void Create_WithOriginalValueAndValueAndValidators_ShouldReturnVariableWithDefaultComparer()
    {
        var (originalValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, value, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
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
    public void Create_WithOriginalValueAndValueAndComparerAndValidators_ShouldReturnCorrectVariable()
    {
        var (originalValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( originalValue, value, comparer, errorsValidator, warningsValidator );

        using ( new AssertionScope() )
        {
            sut.OriginalValue.Should().Be( originalValue );
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
