using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public class StaticVariableTests : TestsBase
{
    [Fact]
    public void Create_WithoutValidators_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, value, comparer );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( value ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.WarningsValidator.TestType().AssignableTo<PassingValidator<int, string>>() )
            .Go();
    }

    [Fact]
    public void Create_WithoutValidatorsAndWithInitialValueOnly_ShouldReturnCorrectVariable()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = Variable.WithoutValidators<string>.Create( initialValue, comparer );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( initialValue ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.WarningsValidator.TestType().AssignableTo<PassingValidator<int, string>>() )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnCorrectVariable()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, value, comparer, errorsValidator, warningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( value ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ) )
            .Go();
    }

    [Fact]
    public void Create_WithInitialValueOnly_ShouldReturnCorrectVariable()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = Variable.Create( initialValue, comparer, errorsValidator, warningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( initialValue ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ) )
            .Go();
    }
}
