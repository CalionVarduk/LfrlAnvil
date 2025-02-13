using LfrlAnvil.Reactive.State.Extensions;
using LfrlAnvil.Reactive.State.Tests.VariableRootTests;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.ExtensionsTests.VariableNodeCollectionTests;

public class VariableNodeCollectionExtensionsTests : TestsBase
{
    [Fact]
    public void FindAllInvalid_ShouldReturnNodesThatAreMarkedAsInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var node1 = Variable.Create( Fixture.Create<string>(), errorsValidator: validator );
        var node2 = Variable.Create( Fixture.Create<string>(), errorsValidator: validator, warningsValidator: validator );
        var node3 = Variable.Create( Fixture.Create<string>(), warningsValidator: validator );
        node1.Change( Fixture.Create<string>() );
        node2.Change( Fixture.Create<string>() );
        node3.RefreshValidation();

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), node1 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node2 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node3 );

        var result = sut.Nodes.FindAllInvalid();

        result.TestSetEqual( [ node1, node2 ] ).Go();
    }

    [Fact]
    public void FindAllWarning_ShouldReturnNodesThatAreMarkedAsWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var node1 = Variable.Create( Fixture.Create<string>(), errorsValidator: validator );
        var node2 = Variable.Create( Fixture.Create<string>(), errorsValidator: validator, warningsValidator: validator );
        var node3 = Variable.Create( Fixture.Create<string>(), warningsValidator: validator );
        node1.Change( Fixture.Create<string>() );
        node2.Change( Fixture.Create<string>() );
        node3.RefreshValidation();

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), node1 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node2 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node3 );

        var result = sut.Nodes.FindAllWarning();

        result.TestSetEqual( [ node2, node3 ] ).Go();
    }

    [Fact]
    public void FindAllChanged_ShouldReturnNodesThatAreMarkedAsChanged()
    {
        var node1 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        var node2 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        var node3 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        node1.Change( Fixture.Create<string>() );
        node2.Change( Fixture.Create<string>() );
        node3.Refresh();

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), node1 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node2 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node3 );

        var result = sut.Nodes.FindAllChanged();

        result.TestSetEqual( [ node1, node2 ] ).Go();
    }

    [Fact]
    public void FindAllReadOnly_ShouldReturnNodesThatAreMarkedAsReadOnly()
    {
        var node1 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        var node2 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        var node3 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        node1.SetReadOnly( true );
        node2.SetReadOnly( true );

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), node1 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node2 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node3 );

        var result = sut.Nodes.FindAllReadOnly();

        result.TestSetEqual( [ node1, node2 ] ).Go();
    }

    [Fact]
    public void FindAllDirty_ShouldReturnNodesThatAreMarkedAsDirty()
    {
        var node1 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        var node2 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        var node3 = Variable.WithoutValidators<string>.Create( Fixture.Create<string>() );
        node1.Refresh();
        node2.Refresh();

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), node1 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node2 );
        sut.ExposedRegisterNode( Fixture.Create<string>(), node3 );

        var result = sut.Nodes.FindAllDirty();

        result.TestSetEqual( [ node1, node2 ] ).Go();
    }
}
