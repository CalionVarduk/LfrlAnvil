using LfrlAnvil.Reactive.State.Extensions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.ExtensionsTests.VariableNodeTests;

public class VariableNodeExtensionsTests : TestsBase
{
    [Fact]
    public void IsChanged_ShouldReturnTrue_WhenStateHasChangedFlag()
    {
        var (value, newValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var node = Variable.WithoutValidators<string>.Create( value, newValue );

        var result = node.IsChanged();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsChanged_ShouldReturnFalse_WhenStateDoesNotHaveChangedFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        var result = node.IsChanged();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInvalid_ShouldReturnTrue_WhenStateHasInvalidFlag()
    {
        var validator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var node = Variable.Create( Fixture.Create<int>(), errorsValidator: validator );
        node.RefreshValidation();

        var result = node.IsInvalid();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInvalid_ShouldReturnFalse_WhenStateDoesNotHaveInvalidFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        var result = node.IsInvalid();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWarning_ShouldReturnTrue_WhenStateHasWarningFlag()
    {
        var validator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var node = Variable.Create( Fixture.Create<int>(), warningsValidator: validator );
        node.RefreshValidation();

        var result = node.IsWarning();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWarning_ShouldReturnFalse_WhenStateDoesNotHaveWarningFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        var result = node.IsWarning();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsReadOnly_ShouldReturnTrue_WhenStateHasReadOnlyFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        node.SetReadOnly( true );

        var result = node.IsReadOnly();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsReadOnly_ShouldReturnFalse_WhenStateDoesNotHaveReadOnlyFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        var result = node.IsReadOnly();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDisposed_ShouldReturnTrue_WhenStateHasDisposedFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        node.Dispose();

        var result = node.IsDisposed();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsDisposed_ShouldReturnFalse_WhenStateDoesNotHaveDisposedFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        var result = node.IsDisposed();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDirty_ShouldReturnTrue_WhenStateHasDirtyFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        node.Refresh();

        var result = node.IsDirty();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_ShouldReturnFalse_WhenStateDoesNotHaveDirtyFlag()
    {
        var node = Variable.WithoutValidators<string>.Create( Fixture.Create<int>() );
        var result = node.IsDirty();
        result.Should().BeFalse();
    }
}
