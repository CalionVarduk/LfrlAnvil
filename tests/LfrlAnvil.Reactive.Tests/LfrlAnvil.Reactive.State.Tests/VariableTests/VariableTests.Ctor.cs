using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.VariableTests;

public partial class VariableTests
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = new Variable<int, string>( initialValue, value, comparer, errorsValidator, warningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( value ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                (( IReadOnlyVariable<int, string> )sut).OnValidate.TestRefEquals( sut.OnValidate ),
                (( IReadOnlyVariable<int> )sut).OnChange.TestRefEquals( sut.OnChange ),
                (( IReadOnlyVariable )sut).ValueType.TestEquals( typeof( int ) ),
                (( IReadOnlyVariable )sut).ValidationResultType.TestEquals( typeof( string ) ),
                (( IReadOnlyVariable )sut).InitialValue.TestEquals( sut.InitialValue ),
                (( IReadOnlyVariable )sut).Value.TestEquals( sut.Value ),
                (( IReadOnlyVariable )sut).Errors.Cast<object>().TestSetEqual( sut.Errors ),
                (( IReadOnlyVariable )sut).Warnings.Cast<object>().TestSetEqual( sut.Warnings ),
                (( IReadOnlyVariable )sut).OnValidate.TestEquals( sut.OnValidate ),
                (( IReadOnlyVariable )sut).OnChange.TestEquals( sut.OnChange ),
                (( IVariableNode )sut).OnValidate.TestEquals( sut.OnValidate ),
                (( IVariableNode )sut).OnChange.TestEquals( sut.OnChange ),
                (( IVariableNode )sut).GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var (initialValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new Variable<int, string>( initialValue, value );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( value ),
                sut.Comparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.WarningsValidator.TestType().AssignableTo<PassingValidator<int, string>>() )
            .Go();
    }

    [Fact]
    public void Ctor_WithInitialValueOnly_ShouldReturnCorrectResult()
    {
        var initialValue = Fixture.Create<int>();
        var comparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<int, string>>();
        var warningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = new Variable<int, string>( initialValue, comparer, errorsValidator, warningsValidator );

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

    [Fact]
    public void Ctor_WithInitialValueOnly_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var initialValue = Fixture.Create<int>();
        var sut = new Variable<int, string>( initialValue );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialValue.TestEquals( initialValue ),
                sut.Value.TestEquals( initialValue ),
                sut.Comparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.WarningsValidator.TestType().AssignableTo<PassingValidator<int, string>>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldSetStateToDefault_WhenInitialValueEqualsValue()
    {
        var value = Fixture.Create<int>();
        var sut = Variable.WithoutValidators<string>.Create( value );
        sut.State.TestEquals( VariableState.Default ).Go();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeErrorsValidator()
    {
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );

        var sut = Variable.Create( value, errorsValidator: errorsValidator );

        sut.Errors.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeWarningsValidator()
    {
        var value = Fixture.Create<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );

        var sut = Variable.Create( value, warningsValidator: warningsValidator );

        sut.Warnings.TestEmpty().Go();
    }
}
