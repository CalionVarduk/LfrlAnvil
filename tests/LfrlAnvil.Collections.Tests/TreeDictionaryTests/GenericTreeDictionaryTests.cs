using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.TreeDictionaryTests;

public abstract class GenericTreeDictionaryTests<TKey, TValue> : GenericDictionaryTestsBase<TKey, TValue>
    where TKey : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new TreeDictionary<TKey, TValue>();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( EqualityComparer<TKey>.Default ),
                sut.Root.TestNull() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );
        var sut = new TreeDictionary<TKey, TValue>( comparer );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Root.TestNull() )
            .Go();
    }

    [Fact]
    public void SetRoot_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty()
    {
        SetRoot_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty_Impl( (sut, k, v) => sut.SetRoot( k, v ) );
    }

    [Fact]
    public void SetRoot_WithNode_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty()
    {
        SetRoot_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty_Impl(
            (sut, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.SetRoot( node );
                return node;
            } );
    }

    private void SetRoot_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty_Impl(
        Func<TreeDictionary<TKey, TValue>, TKey, TValue, TreeDictionaryNode<TKey, TValue>> setRoot)
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();

        var result = setRoot( sut, key, value );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( value ),
                sut.Root.TestEquals( result ),
                result.Key.TestEquals( key ),
                result.Value.TestEquals( value ),
                result.Parent.TestNull(),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( result ) )
            .Go();
    }

    [Fact]
    public void SetRoot_ShouldAddNewNodeAsRootCorrectly_WhenDictionaryHasRoot()
    {
        SetRoot_ShouldAddNewNodeAsRootCorrectly_WhenDictionaryHasRoot_Impl( (sut, k, v) => sut.SetRoot( k, v ) );
    }

    [Fact]
    public void SetRoot_WithNode_ShouldAddNewNodeAsRootCorrectly_WhenDictionaryHasRoot()
    {
        SetRoot_ShouldAddNewNodeAsRootCorrectly_WhenDictionaryHasRoot_Impl(
            (sut, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.SetRoot( node );
                return node;
            } );
    }

    private void SetRoot_ShouldAddNewNodeAsRootCorrectly_WhenDictionaryHasRoot_Impl(
        Func<TreeDictionary<TKey, TValue>, TKey, TValue, TreeDictionaryNode<TKey, TValue>> setRoot)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var oldRoot = sut.SetRoot( keys[0], values[0] );

        var result = setRoot( sut, keys[1], values[1] );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestEquals( values[1] ),
                sut.Root.TestEquals( result ),
                result.Key.TestEquals( keys[1] ),
                result.Value.TestEquals( values[1] ),
                result.Parent.TestNull(),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( result, oldRoot ) )
            .Go();
    }

    [Fact]
    public void SetRoot_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        SetRoot_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl( (sut, k, v, _) => sut.SetRoot( k, v ) );
    }

    [Fact]
    public void SetRoot_WithNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        SetRoot_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
            (sut, k, v, i) =>
            {
                i.Node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.SetRoot( i.Node );
            } );
    }

    private void SetRoot_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
        Action<TreeDictionary<TKey, TValue>, TKey, TValue, ThrowResultInterceptor> setRoot)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( key, values[0] );
        var intercept = new ThrowResultInterceptor();

        var action = Lambda.Of( () => setRoot( sut, key, values[1], intercept ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<ArgumentException>(),
                    sut.Count.TestEquals( 1 ),
                    sut.Root.TestEquals( root ),
                    root.Value.TestEquals( values[0] ),
                    AssertNodeRelationship( root ),
                    intercept.Node is null ? Assertion.All() : AssertLackOfLinkedTree( intercept.Node ) ) )
            .Go();
    }

    [Fact]
    public void SetRoot_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsAlreadyLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var node = sut.SetRoot( key, value );

        var action = Lambda.Of( () => sut.SetRoot( node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Add_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty()
    {
        Add_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty_Impl( (sut, k, v) => sut.Add( k, v ) );
    }

    [Fact]
    public void Add_WithNode_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty()
    {
        Add_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty_Impl(
            (sut, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.Add( node );
                return node;
            } );
    }

    private void Add_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty_Impl(
        Func<TreeDictionary<TKey, TValue>, TKey, TValue, TreeDictionaryNode<TKey, TValue>> add)
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();

        var result = add( sut, key, value );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( value ),
                sut.Root.TestEquals( result ),
                result.Key.TestEquals( key ),
                result.Value.TestEquals( value ),
                result.Parent.TestNull(),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( result ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewNodeAsRootChildCorrectly_WhenDictionaryHasRoot()
    {
        Add_ShouldAddNewNodeAsRootChildCorrectly_WhenDictionaryHasRoot_Impl( (sut, k, v) => sut.Add( k, v ) );
    }

    [Fact]
    public void Add_WithNode_ShouldAddNewNodeAsRootChildCorrectly_WhenDictionaryHasRoot()
    {
        Add_ShouldAddNewNodeAsRootChildCorrectly_WhenDictionaryHasRoot_Impl(
            (sut, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.Add( node );
                return node;
            } );
    }

    private void Add_ShouldAddNewNodeAsRootChildCorrectly_WhenDictionaryHasRoot_Impl(
        Func<TreeDictionary<TKey, TValue>, TKey, TValue, TreeDictionaryNode<TKey, TValue>> add)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( keys[0], values[0] );

        var result = add( sut, keys[1], values[1] );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestEquals( values[1] ),
                sut.Root.TestEquals( root ),
                result.Key.TestEquals( keys[1] ),
                result.Value.TestEquals( values[1] ),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( root, result ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        Add_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl( (sut, k, v, _) => sut.Add( k, v ) );
    }

    [Fact]
    public void Add_WithNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        Add_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
            (sut, k, v, i) =>
            {
                i.Node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.Add( i.Node );
            } );
    }

    private void Add_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
        Action<TreeDictionary<TKey, TValue>, TKey, TValue, ThrowResultInterceptor> add)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( key, values[0] );
        var intercept = new ThrowResultInterceptor();

        var action = Lambda.Of( () => add( sut, key, values[1], intercept ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<ArgumentException>(),
                    sut.Count.TestEquals( 1 ),
                    sut.Root.TestEquals( root ),
                    root.Value.TestEquals( values[0] ),
                    AssertNodeRelationship( root ),
                    intercept.Node is null ? Assertion.All() : AssertLackOfLinkedTree( intercept.Node ) ) )
            .Go();
    }

    [Fact]
    public void Add_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsAlreadyLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var node = sut.SetRoot( key, value );

        var action = Lambda.Of( () => sut.Add( node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddTo_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren_Impl( (sut, p, k, v) => sut.AddTo( p.Key, k, v ) );
    }

    [Fact]
    public void AddTo_WithNode_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren_Impl(
            (sut, p, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.AddTo( p.Key, node );
                return node;
            } );
    }

    [Fact]
    public void AddTo_WithParentNode_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren_Impl( (sut, p, k, v) => sut.AddTo( p, k, v ) );
    }

    [Fact]
    public void AddTo_WithParentNodeAndNewNode_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren_Impl(
            (sut, p, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.AddTo( p, node );
                return node;
            } );
    }

    private void AddTo_ShouldAddNewNodeCorrectly_WhenParentDoesNotHaveAnyChildren_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TKey, TValue, TreeDictionaryNode<TKey, TValue>> addTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.Add( keys[0], values[0] );
        var parent = sut.Add( keys[1], values[1] );

        var result = addTo( sut, parent, keys[2], values[2] );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut[keys[2]].TestEquals( values[2] ),
                result.Key.TestEquals( keys[2] ),
                result.Value.TestEquals( values[2] ),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( root, parent ),
                AssertNodeRelationship( parent, result ),
                AssertNodeRelationship( result ) )
            .Go();
    }

    [Fact]
    public void AddTo_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild_Impl( (sut, p, k, v) => sut.AddTo( p.Key, k, v ) );
    }

    [Fact]
    public void AddTo_WithNode_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild_Impl(
            (sut, p, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.AddTo( p.Key, node );
                return node;
            } );
    }

    [Fact]
    public void AddTo_WithParentNode_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild_Impl( (sut, p, k, v) => sut.AddTo( p, k, v ) );
    }

    [Fact]
    public void AddTo_WithParentNodeAndNewNode_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild()
    {
        AddTo_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild_Impl(
            (sut, p, k, v) =>
            {
                var node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.AddTo( p, node );
                return node;
            } );
    }

    private void AddTo_ShouldAddNewNodeCorrectly_WhenParentHasOtherChild_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TKey, TValue, TreeDictionaryNode<TKey, TValue>> addTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 4 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.Add( keys[0], values[0] );
        var parent = sut.Add( keys[1], values[1] );
        var child = sut.AddTo( parent.Key, keys[2], values[2] );

        var result = addTo( sut, parent, keys[3], values[3] );

        Assertion.All(
                sut.Count.TestEquals( 4 ),
                sut[keys[3]].TestEquals( values[3] ),
                result.Key.TestEquals( keys[3] ),
                result.Value.TestEquals( values[3] ),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( root, parent ),
                AssertNodeRelationship( parent, child, result ),
                AssertNodeRelationship( child ),
                AssertNodeRelationship( result ) )
            .Go();
    }

    [Fact]
    public void AddTo_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        AddTo_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl( (sut, p, k, v, _) => sut.AddTo( p.Key, k, v ) );
    }

    [Fact]
    public void AddTo_WithNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        AddTo_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
            (sut, p, k, v, i) =>
            {
                i.Node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.AddTo( p.Key, i.Node );
            } );
    }

    [Fact]
    public void AddTo_WithParentNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        AddTo_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl( (sut, p, k, v, _) => sut.AddTo( p, k, v ) );
    }

    [Fact]
    public void AddTo_WithParentNodeAndNewNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        AddTo_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
            (sut, p, k, v, i) =>
            {
                i.Node = new TreeDictionaryNode<TKey, TValue>( k, v );
                sut.AddTo( p, i.Node );
            } );
    }

    private void AddTo_ShouldThrowArgumentException_WhenKeyAlreadyExists_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TKey, TValue, ThrowResultInterceptor> addTo)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( key, values[0] );
        var intercept = new ThrowResultInterceptor();

        var action = Lambda.Of( () => addTo( sut, root, key, values[1], intercept ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<ArgumentException>(),
                    sut.Count.TestEquals( 1 ),
                    sut.Root.TestEquals( root ),
                    root.Value.TestEquals( values[0] ),
                    AssertNodeRelationship( root ),
                    intercept.Node is null ? Assertion.All() : AssertLackOfLinkedTree( intercept.Node ) ) )
            .Go();
    }

    [Fact]
    public void AddTo_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();

        var action = Lambda.Of( () => sut.AddTo( keys[0], keys[1], value ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void AddTo_WithNode_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], value );

        var action = Lambda.Of( () => sut.AddTo( keys[0], node ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void AddTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsAlreadyLinkedToAnyTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var parent = sut.Add( keys[0], values[0] );
        var node = sut.Add( keys[1], values[1] );

        var action = Lambda.Of( () => sut.AddTo( parent.Key, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentBelongsToDifferentTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var other = new TreeDictionary<TKey, TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = other.SetRoot( keys[0], values[0] );

        var action = Lambda.Of( () => sut.AddTo( parent, keys[1], values[1] ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentIsNotLinkedToAnyTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = new TreeDictionaryNode<TKey, TValue>( keys[0], values[0] );

        var action = Lambda.Of( () => sut.AddTo( parent, keys[1], values[1] ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddTo_WithParentNodeAndNewNode_ShouldThrowInvalidOperationException_WhenParentBelongsToDifferentTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var other = new TreeDictionary<TKey, TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = other.SetRoot( keys[0], values[0] );
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], values[1] );

        var action = Lambda.Of( () => sut.AddTo( parent, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddTo_WithParentNodeAndNewNode_ShouldThrowInvalidOperationException_WhenParentIsNotLinkedToAnyTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = new TreeDictionaryNode<TKey, TValue>( keys[0], values[0] );
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], values[1] );

        var action = Lambda.Of( () => sut.AddTo( parent, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddTo_WithParentNodeAndNewNode_ShouldThrowInvalidOperationException_WhenNodeIsAlreadyLinkedToAnyTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var parent = sut.Add( keys[0], values[0] );
        var node = sut.Add( keys[1], values[1] );

        var action = Lambda.Of( () => sut.AddTo( parent, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddSubtree_ShouldAddNodesAsRoot_WhenDictionaryIsEmpty()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var subtree = new TreeDictionary<TKey, TValue>
        {
            { keys[0], values[0] },
            { keys[1], values[1] },
            { keys[2], values[2] }
        };

        var result = sut.AddSubtree( subtree.Root! );

        var a = sut.GetNode( keys[0] );
        var b = sut.GetNode( keys[1] );
        var c = sut.GetNode( keys[2] );

        Assertion.All(
                a.TestNotNull(),
                b.TestNotNull(),
                c.TestNotNull(),
                sut.Count.TestEquals( 3 ),
                sut.Root.TestEquals( a ),
                result.TestEquals( a ) )
            .Go();

        Assertion.All(
                a!.Parent.TestNull(),
                AssertNodeRelationship( a, b!, c! ),
                AssertNodeRelationship( b! ),
                AssertNodeRelationship( c! ) )
            .Go();
    }

    [Fact]
    public void AddSubtree_ShouldAddNodesAsChildOfRoot_WhenDictionaryHasNodes()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );
        var c = sut.Add( keys[2], values[2] );
        var subtree = new TreeDictionary<TKey, TValue>
        {
            { keys[3], values[3] },
            { keys[4], values[4] },
            { keys[5], values[5] }
        };

        var result = sut.AddSubtree( subtree.Root! );

        var d = sut.GetNode( keys[3] );
        var e = sut.GetNode( keys[4] );
        var f = sut.GetNode( keys[5] );

        Assertion.All(
                d.TestNotNull(),
                e.TestNotNull(),
                f.TestNotNull(),
                sut.Count.TestEquals( 6 ),
                sut.Root.TestEquals( a ),
                result.TestEquals( d ) )
            .Go();

        Assertion.All(
                a.Parent.TestNull(),
                AssertNodeRelationship( a, b, c, d! ),
                AssertNodeRelationship( b ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d!, e!, f! ),
                AssertNodeRelationship( e! ),
                AssertNodeRelationship( f! ) )
            .Go();
    }

    [Fact]
    public void AddSubtree_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers()
    {
        AddSubtree_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers_Impl( (sut, n) => sut.AddSubtree( n ) );
    }

    [Fact]
    public void AddSubtreeTo_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers()
    {
        AddSubtree_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers_Impl(
            (sut, n) => sut.AddSubtreeTo( sut.Root!.Key, n ) );
    }

    [Fact]
    public void AddSubtreeTo_WithParentNode_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers()
    {
        AddSubtree_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers_Impl(
            (sut, n) => sut.AddSubtreeTo( sut.Root!, n ) );
    }

    private void AddSubtree_ShouldAddNodesCorrectly_WhenSubtreeKeysAreNotUniqueDueToDifferentComparers_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> addSubtree)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 4 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );

        var comparer = EqualityComparerFactory<TKey>.Create(
            (a, b) => ((a!.Equals( keys[1] ) || a.Equals( keys[2] )) && (b!.Equals( keys[1] ) || b.Equals( keys[2] ))) || a.Equals( b ),
            a => a.Equals( keys[1] ) || a.Equals( keys[2] ) ? keys[1].GetHashCode() : a.GetHashCode() );

        var sut = new TreeDictionary<TKey, TValue>( comparer );
        var a = sut.SetRoot( keys[0], values[0] );
        var subtree = new TreeDictionary<TKey, TValue>();
        subtree.SetRoot( keys[1], values[1] );
        subtree.AddTo( keys[1], keys[2], values[2] );
        subtree.AddTo( keys[2], keys[3], values[3] );

        var result = addSubtree( sut, subtree.Root! );

        var b = sut.GetNode( keys[1] );
        var c = sut.GetNode( keys[3] );

        Assertion.All(
                b.TestNotNull(),
                c.TestNotNull(),
                sut.Count.TestEquals( 3 ),
                sut.Root.TestEquals( a ),
                result.TestEquals( b ) )
            .Go();

        Assertion.All(
                a.Parent.TestNull(),
                AssertNodeRelationship( a, b! ),
                AssertNodeRelationship( b!, c! ),
                AssertNodeRelationship( c! ) )
            .Go();
    }

    [Fact]
    public void AddSubtreeTo_ShouldAddNodesCorrectly()
    {
        AddSubtreeTo_ShouldAddNodesCorrectly_Impl( (sut, p, n) => sut.AddSubtreeTo( p.Key, n ) );
    }

    [Fact]
    public void AddSubtreeTo_WithParentNode_ShouldAddNodesCorrectly()
    {
        AddSubtreeTo_ShouldAddNodesCorrectly_Impl( (sut, p, n) => sut.AddSubtreeTo( p, n ) );
    }

    private void AddSubtreeTo_ShouldAddNodesCorrectly_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> addSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );
        var c = sut.Add( keys[2], values[2] );
        var subtree = new TreeDictionary<TKey, TValue>
        {
            { keys[3], values[3] },
            { keys[4], values[4] },
            { keys[5], values[5] }
        };

        var result = addSubtreeTo( sut, b, subtree.Root! );

        var d = sut.GetNode( keys[3] );
        var e = sut.GetNode( keys[4] );
        var f = sut.GetNode( keys[5] );

        Assertion.All(
                d.TestNotNull(),
                e.TestNotNull(),
                f.TestNotNull(),
                sut.Count.TestEquals( 6 ),
                sut.Root.TestEquals( a ),
                result.TestEquals( d ) )
            .Go();

        Assertion.All(
                a.Parent.TestNull(),
                AssertNodeRelationship( a, b, c ),
                AssertNodeRelationship( b, d! ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d!, e!, f! ),
                AssertNodeRelationship( e! ),
                AssertNodeRelationship( f! ) )
            .Go();
    }

    [Fact]
    public void AddSubtree_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree()
    {
        AddSubtree_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree_Impl( (sut, _, n) => sut.AddSubtree( n ) );
    }

    [Fact]
    public void AddSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree()
    {
        AddSubtree_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree_Impl( (sut, p, n) => sut.AddSubtreeTo( p.Key, n ) );
    }

    [Fact]
    public void AddSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree()
    {
        AddSubtree_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree_Impl( (sut, p, n) => sut.AddSubtreeTo( p, n ) );
    }

    private void AddSubtree_ShouldThrowInvalidOperationException_WhenNodeBelongsToTree_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> addSubtree)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var parent = sut.SetRoot( keys[0], values[0] );
        var node = sut.AddTo( parent, keys[1], values[1] );

        var action = Lambda.Of( () => addSubtree( sut, parent, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddSubtree_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys()
    {
        AddSubtree_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys_Impl( (sut, _, n) => sut.AddSubtree( n ) );
    }

    [Fact]
    public void AddSubtreeTo_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys()
    {
        AddSubtree_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys_Impl( (sut, p, n) => sut.AddSubtreeTo( p.Key, n ) );
    }

    [Fact]
    public void AddSubtreeTo_WithParentNode_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys()
    {
        AddSubtree_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys_Impl( (sut, p, n) => sut.AddSubtreeTo( p, n ) );
    }

    private void AddSubtree_ShouldThrowArgumentException_WhenTreeContainsAnyOfSubtreeKeys_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> addSubtree)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 5 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 5 );
        var sut = new TreeDictionary<TKey, TValue>
        {
            { keys[0], values[0] },
            { keys[1], values[1] },
            { keys[2], values[2] }
        };

        var subtree = new TreeDictionary<TKey, TValue>
        {
            { keys[3], values[3] },
            { keys[4], values[4] },
            { keys[1], values[1] }
        };

        var parent = sut.GetNode( keys[1] )!;

        var action = Lambda.Of( () => addSubtree( sut, parent, subtree.Root! ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddSubtreeTo_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var subtree = new TreeDictionary<TKey, TValue> { { keys[1], value } };

        var action = Lambda.Of( () => sut.AddSubtreeTo( keys[0], subtree.Root! ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void AddSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentBelongsToDifferentTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var other = new TreeDictionary<TKey, TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = other.SetRoot( keys[0], values[0] );
        var subtree = new TreeDictionary<TKey, TValue> { { keys[1], values[1] } };

        var action = Lambda.Of( () => sut.AddSubtreeTo( parent, subtree.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void AddSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeIsNotLinkedToAnyTree()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var parent = new TreeDictionaryNode<TKey, TValue>( keys[0], values[0] );
        var subtree = new TreeDictionary<TKey, TValue> { { keys[1], values[1] } };

        var action = Lambda.Of( () => sut.AddSubtreeTo( parent, subtree.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Remove_ShouldReturnFalseAndDoNothing_WhenKeyDoesntExist()
    {
        Remove_ShouldReturnFalseAndDoNothing_WhenKeyDoesntExist_Impl( (sut, k) => (sut.Remove( k ), default) );
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnFalseAndDoNothing_WhenKeyDoesntExist()
    {
        Remove_ShouldReturnFalseAndDoNothing_WhenKeyDoesntExist_Impl(
            (sut, k) =>
            {
                var result = sut.Remove( k, out var v );
                return (result, v);
            } );
    }

    private void Remove_ShouldReturnFalseAndDoNothing_WhenKeyDoesntExist_Impl(
        Func<TreeDictionary<TKey, TValue>, TKey, (bool Result, TValue? Removed)> remove)
    {
        var key = Fixture.Create<TKey>();
        var sut = new TreeDictionary<TKey, TValue>();

        var (result, removed) = remove( sut, key );

        Assertion.All(
                result.TestFalse(),
                removed.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveRoot_WhenDictionaryHasOneItem()
    {
        Remove_ShouldReturnTrueAndRemoveRoot_WhenDictionaryHasOneItem_Impl( (sut, n) => (sut.Remove( n.Key ), n.Value) );
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnTrueAndRemoveRoot_WhenDictionaryHasOneItem()
    {
        Remove_ShouldReturnTrueAndRemoveRoot_WhenDictionaryHasOneItem_Impl(
            (sut, n) =>
            {
                sut.Remove( n );
                return (true, n.Value);
            } );
    }

    [Fact]
    public void Remove_WithRemoved_ShouldRemoveRoot_WhenDictionaryHasOneItem()
    {
        Remove_ShouldReturnTrueAndRemoveRoot_WhenDictionaryHasOneItem_Impl(
            (sut, n) =>
            {
                var result = sut.Remove( n.Key, out var v );
                return (result, v);
            } );
    }

    private void Remove_ShouldReturnTrueAndRemoveRoot_WhenDictionaryHasOneItem_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, (bool Result, TValue? Removed)> remove)
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( key, value );

        var (result, removed) = remove( sut, root );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( value ),
                sut.Count.TestEquals( 0 ),
                sut.Root.TestNull(),
                AssertLackOfLinkedTree( root ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingRoot()
    {
        Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingRoot_Impl( (sut, n) => (sut.Remove( n.Key ), n.Value) );
    }

    [Fact]
    public void Remove_WithNode_ShouldRemoveCorrectExistingItem_WhenRemovingRoot()
    {
        Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingRoot_Impl(
            (sut, n) =>
            {
                sut.Remove( n );
                return (true, n.Value);
            } );
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingRoot()
    {
        Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingRoot_Impl(
            (sut, n) =>
            {
                var result = sut.Remove( n.Key, out var v );
                return (result, v);
            } );
    }

    private void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingRoot_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, (bool Result, TValue? Removed)> remove)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( keys[0], values[0] );
        var firstChild = sut.AddTo( root, keys[1], values[1] );
        var secondChild = sut.AddTo( root, keys[2], values[2] );

        var (result, removed) = remove( sut, root );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( values[0] ),
                sut.Count.TestEquals( 2 ),
                sut.Root.TestEquals( firstChild ),
                firstChild.Parent.TestNull(),
                AssertNodeRelationship( firstChild, secondChild ),
                AssertNodeRelationship( secondChild ),
                AssertLackOfLinkedTree( root ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingNonRootNode()
    {
        Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingNonRootNode_Impl( (sut, n) => (sut.Remove( n.Key ), n.Value) );
    }

    [Fact]
    public void Remove_WithNode_ShouldRemoveCorrectExistingItem_WhenRemovingNonRootNode()
    {
        Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingNonRootNode_Impl(
            (sut, n) =>
            {
                sut.Remove( n );
                return (true, n.Value);
            } );
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingNonRootNode()
    {
        Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingNonRootNode_Impl(
            (sut, n) =>
            {
                var result = sut.Remove( n.Key, out var v );
                return (result, v);
            } );
    }

    private void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenRemovingNonRootNode_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, (bool Result, TValue? Removed)> remove)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 5 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 5 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( keys[0], values[0] );
        var node = sut.AddTo( root, keys[1], values[1] );
        var other = sut.AddTo( root, keys[2], values[2] );
        var firstChild = sut.AddTo( node, keys[3], values[3] );
        var secondChild = sut.AddTo( node, keys[4], values[4] );

        var (result, removed) = remove( sut, node );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( values[1] ),
                sut.Count.TestEquals( 4 ),
                sut.Root.TestEquals( root ),
                root.Parent.TestNull(),
                AssertNodeRelationship( root, other, firstChild, secondChild ),
                AssertNodeRelationship( other ),
                AssertNodeRelationship( firstChild ),
                AssertNodeRelationship( secondChild ),
                AssertLackOfLinkedTree( node ) )
            .Go();
    }

    [Fact]
    public void Remove_WithNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.Remove( node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Remove_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.Remove( node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void RemoveSubtree_ShouldReturnCountAndRemoveAllNodes_WhenRemovingRoot()
    {
        RemoveSubtree_ShouldReturnCountAndRemoveAllNodes_WhenRemovingRoot_Impl( sut => sut.RemoveSubtree( sut.Root!.Key ) );
    }

    [Fact]
    public void RemoveSubtree_WithNode_ShouldReturnCountAndRemoveAllNodes_WhenRemovingRoot()
    {
        RemoveSubtree_ShouldReturnCountAndRemoveAllNodes_WhenRemovingRoot_Impl( sut => sut.RemoveSubtree( sut.Root! ) );
    }

    private void RemoveSubtree_ShouldReturnCountAndRemoveAllNodes_WhenRemovingRoot_Impl(
        Func<TreeDictionary<TKey, TValue>, int> removeSubtree)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>
        {
            { keys[0], values[0] },
            { keys[1], values[1] },
            { keys[2], values[2] }
        };

        var nodes = sut.Nodes.ToList();

        var result = removeSubtree( sut );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 0 ),
                sut.Root.TestNull(),
                nodes.TestAll( (node, _) => AssertLackOfLinkedTree( node ) ) )
            .Go();
    }

    [Fact]
    public void RemoveSubtree_ShouldReturnCorrectResultAndRemoveCorrectNodes_WhenNotRemovingRoot()
    {
        RemoveSubtree_ShouldReturnCorrectResultAndRemoveCorrectNodes_WhenNotRemovingRoot_Impl( (sut, n) => sut.RemoveSubtree( n.Key ) );
    }

    [Fact]
    public void RemoveSubtree_WithNode_ShouldReturnCorrectResultAndRemoveCorrectNodes_WhenNotRemovingRoot()
    {
        RemoveSubtree_ShouldReturnCorrectResultAndRemoveCorrectNodes_WhenNotRemovingRoot_Impl( (sut, n) => sut.RemoveSubtree( n ) );
    }

    private void RemoveSubtree_ShouldReturnCorrectResultAndRemoveCorrectNodes_WhenNotRemovingRoot_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, int> removeSubtree)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( c, keys[3], values[3] );
        var e = sut.AddTo( c, keys[4], values[4] );
        var f = sut.AddTo( e, keys[5], values[5] );

        var result = removeSubtree( sut, c );

        Assertion.All(
                result.TestEquals( 4 ),
                sut.Count.TestEquals( 2 ),
                sut.Root.TestEquals( a ),
                a.Parent.TestNull(),
                AssertNodeRelationship( a, b ),
                AssertNodeRelationship( b ),
                AssertLackOfLinkedTree( c ),
                AssertLackOfLinkedTree( d ),
                AssertLackOfLinkedTree( e ),
                AssertLackOfLinkedTree( f ) )
            .Go();
    }

    [Fact]
    public void RemoveSubtree_ShouldReturnZeroAndDoNothing_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new TreeDictionary<TKey, TValue>();

        var result = sut.RemoveSubtree( key );

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void RemoveSubtree_ShouldReturnZeroAndDoNothing_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var result = sut.RemoveSubtree( keys[1] );

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void RemoveSubtree_WithNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.RemoveSubtree( node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void RemoveSubtree_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.RemoveSubtree( node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Swap_ShouldDoNothing_WhenKeysAreEqual()
    {
        Swap_ShouldDoNothing_WhenKeysAreEqual_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldDoNothing_WhenKeysAreEqual()
    {
        Swap_ShouldDoNothing_WhenKeysAreEqual_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldDoNothing_WhenKeysAreEqual_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );

        swap( sut, a, a );

        Assertion.All(
                AssertNodeRelationship( a, b ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesFromTheSameParentCorrectly()
    {
        Swap_ShouldSwapNodesFromTheSameParentCorrectly_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesFromTheSameParentCorrectly()
    {
        Swap_ShouldSwapNodesFromTheSameParentCorrectly_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesFromTheSameParentCorrectly_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 5 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 5 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( c, keys[4], values[4] );

        swap( sut, b, c );

        Assertion.All(
                a.Parent.TestNull(),
                AssertNodeRelationship( a, c, b ),
                AssertNodeRelationship( c, d ),
                AssertNodeRelationship( b, e ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsDirectRootChild()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsDirectRootChild_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsDirectRootChild()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsDirectRootChild_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsDirectRootChild_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 4 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );

        swap( sut, a, b );

        Assertion.All(
                sut.Root.TestEquals( b ),
                b.Parent.TestNull(),
                AssertNodeRelationship( b, a, c ),
                AssertNodeRelationship( a, d ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsDirectRootChildAndSecondNodeIsRoot()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsDirectRootChildAndSecondNodeIsRoot_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstNodeIsDirectRootChildAndSecondNodeIsRoot()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsDirectRootChildAndSecondNodeIsRoot_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsDirectRootChildAndSecondNodeIsRoot_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 4 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );

        swap( sut, b, a );

        Assertion.All(
                sut.Root.TestEquals( b ),
                b.Parent.TestNull(),
                AssertNodeRelationship( b, a, c ),
                AssertNodeRelationship( a, d ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsIndirectRootDescendant()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsIndirectRootDescendant_Impl(
            (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsIndirectRootDescendant()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsIndirectRootDescendant_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsRootAndSecondNodeIsIndirectRootDescendant_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( c, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );

        swap( sut, a, c );

        Assertion.All(
                sut.Root.TestEquals( c ),
                c.Parent.TestNull(),
                AssertNodeRelationship( c, b ),
                AssertNodeRelationship( b, a, d ),
                AssertNodeRelationship( a, e, f ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsIndirectRootDescendantAndSecondNodeIsRoot()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsIndirectRootDescendantAndSecondNodeIsRoot_Impl(
            (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstNodeIsIndirectRootDescendantAndSecondNodeIsRoot()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsIndirectRootDescendantAndSecondNodeIsRoot_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstNodeIsIndirectRootDescendantAndSecondNodeIsRoot_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( c, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );

        swap( sut, c, a );

        Assertion.All(
                sut.Root.TestEquals( c ),
                c.Parent.TestNull(),
                AssertNodeRelationship( c, b ),
                AssertNodeRelationship( b, a, d ),
                AssertNodeRelationship( a, e, f ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstIsParentOfSecond()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstIsParentOfSecond_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstIsParentOfSecond()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstIsParentOfSecond_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstIsParentOfSecond_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( d, keys[5], values[5] );

        swap( sut, b, d );

        Assertion.All(
                sut.Root.TestEquals( a ),
                a.Parent.TestNull(),
                AssertNodeRelationship( a, d, c ),
                AssertNodeRelationship( d, b, e ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( b, f ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenSecondIsParentOfFirst()
    {
        Swap_ShouldSwapNodesCorrectly_WhenSecondIsParentOfFirst_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenSecondIsParentOfFirst()
    {
        Swap_ShouldSwapNodesCorrectly_WhenSecondIsParentOfFirst_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenSecondIsParentOfFirst_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( d, keys[5], values[5] );

        swap( sut, d, b );

        Assertion.All(
                sut.Root.TestEquals( a ),
                a.Parent.TestNull(),
                AssertNodeRelationship( a, d, c ),
                AssertNodeRelationship( d, b, e ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( b, f ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstIsIndirectAncestorOfSecond()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstIsIndirectAncestorOfSecond_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstIsIndirectAncestorOfSecond()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstIsIndirectAncestorOfSecond_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstIsIndirectAncestorOfSecond_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 7 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 7 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( d, keys[4], values[4] );
        var f = sut.AddTo( d, keys[5], values[5] );
        var g = sut.AddTo( e, keys[6], values[6] );

        swap( sut, b, e );

        Assertion.All(
                sut.Root.TestEquals( a ),
                a.Parent.TestNull(),
                AssertNodeRelationship( a, e, c ),
                AssertNodeRelationship( e, d ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d, b, f ),
                AssertNodeRelationship( b, g ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenSecondIsIndirectAncestorOfFirst()
    {
        Swap_ShouldSwapNodesCorrectly_WhenSecondIsIndirectAncestorOfFirst_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenSecondIsIndirectAncestorOfFirst()
    {
        Swap_ShouldSwapNodesCorrectly_WhenSecondIsIndirectAncestorOfFirst_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenSecondIsIndirectAncestorOfFirst_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 7 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 7 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( d, keys[4], values[4] );
        var f = sut.AddTo( d, keys[5], values[5] );
        var g = sut.AddTo( e, keys[6], values[6] );

        swap( sut, e, b );

        Assertion.All(
                sut.Root.TestEquals( a ),
                a.Parent.TestNull(),
                AssertNodeRelationship( a, e, c ),
                AssertNodeRelationship( e, d ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d, b, f ),
                AssertNodeRelationship( b, g ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldSwapNodesCorrectly_WhenFirstAndSecondAreNotDirectlyRelated()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstAndSecondAreNotDirectlyRelated_Impl( (sut, f, s) => sut.Swap( f.Key, s.Key ) );
    }

    [Fact]
    public void Swap_WithNodes_ShouldSwapNodesCorrectly_WhenFirstAndSecondAreNotDirectlyRelated()
    {
        Swap_ShouldSwapNodesCorrectly_WhenFirstAndSecondAreNotDirectlyRelated_Impl( (sut, f, s) => sut.Swap( f, s ) );
    }

    private void Swap_ShouldSwapNodesCorrectly_WhenFirstAndSecondAreNotDirectlyRelated_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> swap)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 9 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 9 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );
        var g = sut.AddTo( c, keys[6], values[6] );
        var h = sut.AddTo( d, keys[7], values[7] );
        var i = sut.AddTo( g, keys[8], values[8] );

        swap( sut, d, g );

        Assertion.All(
                sut.Root.TestEquals( a ),
                a.Parent.TestNull(),
                AssertNodeRelationship( a, b, c ),
                AssertNodeRelationship( b, g, e ),
                AssertNodeRelationship( c, f, d ),
                AssertNodeRelationship( g, h ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( d, i ),
                AssertNodeRelationship( h ),
                AssertNodeRelationship( i ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldThrowKeyNotFoundException_WhenFirstKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.Swap( keys[1], keys[0] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Swap_ShouldThrowKeyNotFoundException_WhenSecondKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.Swap( keys[0], keys[1] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Swap_WithNodes_ShouldThrowInvalidOperationException_WhenFirstNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.Swap( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Swap_WithNodes_ShouldThrowInvalidOperationException_WhenFirstNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.Swap( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Swap_WithNodes_ShouldThrowInvalidOperationException_WhenSecondNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.Swap( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Swap_WithNodes_ShouldThrowInvalidOperationException_WhenSecondNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.Swap( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );

        var result = moveTo( sut, a, b );

        Assertion.All(
                result.TestEquals( b ),
                AssertNodeRelationship( a, b ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsFirstRootChild_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );

        var result = moveTo( sut, b, a );

        Assertion.All(
                result.TestEquals( a ),
                sut.Root.TestEquals( b ),
                AssertNodeRelationship( b, d, e, a, c ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( a ),
                AssertNodeRelationship( c, f ),
                AssertNodeRelationship( f ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsNonFirstRootChild_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( c, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );

        var result = moveTo( sut, c, a );

        Assertion.All(
                result.TestEquals( a ),
                sut.Root.TestEquals( c ),
                AssertNodeRelationship( c, e, f, a, b ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( a ),
                AssertNodeRelationship( b, d ),
                AssertNodeRelationship( d ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 6 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( d, keys[4], values[4] );
        var f = sut.AddTo( d, keys[5], values[5] );

        var result = moveTo( sut, d, a );

        Assertion.All(
                result.TestEquals( a ),
                sut.Root.TestEquals( b ),
                AssertNodeRelationship( b, d, c ),
                AssertNodeRelationship( d, e, f, a ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( a ),
                AssertNodeRelationship( c ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesFirstChild_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 7 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 7 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( d, keys[5], values[5] );
        var g = sut.AddTo( d, keys[6], values[6] );

        var result = moveTo( sut, d, b );

        Assertion.All(
                result.TestEquals( b ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, c, d, e ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d, f, g, b ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesNonFirstChild_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 7 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 7 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( c, keys[3], values[3] );
        var e = sut.AddTo( c, keys[4], values[4] );
        var f = sut.AddTo( e, keys[5], values[5] );
        var g = sut.AddTo( e, keys[6], values[6] );

        var result = moveTo( sut, e, c );

        Assertion.All(
                result.TestEquals( c ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, b, d, e ),
                AssertNodeRelationship( b ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e, f, g, c ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g ),
                AssertNodeRelationship( c ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenParentIsNodesIndirectDescendant_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 8 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 8 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( e, keys[5], values[5] );
        var g = sut.AddTo( f, keys[6], values[6] );
        var h = sut.AddTo( f, keys[7], values[7] );

        var result = moveTo( sut, f, b );

        Assertion.All(
                result.TestEquals( b ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, c, d, e ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e, f ),
                AssertNodeRelationship( f, g, h, b ),
                AssertNodeRelationship( g ),
                AssertNodeRelationship( h ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 8 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 8 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( e, keys[5], values[5] );
        var g = sut.AddTo( f, keys[6], values[6] );
        var h = sut.AddTo( f, keys[7], values[7] );

        var result = moveTo( sut, b, f );

        Assertion.All(
                result.TestEquals( f ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, b, c ),
                AssertNodeRelationship( b, d, e, f ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e, g, h ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g ),
                AssertNodeRelationship( h ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
            (sut, a, b) =>
            {
                sut.MoveTo( a, b );
                return b;
            } );
    }

    private void MoveTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 9 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 9 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );
        var g = sut.AddTo( c, keys[6], values[6] );
        var h = sut.AddTo( d, keys[7], values[7] );
        var i = sut.AddTo( g, keys[8], values[8] );

        var result = moveTo( sut, g, d );

        Assertion.All(
                result.TestEquals( d ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, b, c ),
                AssertNodeRelationship( b, e, h ),
                AssertNodeRelationship( c, f, g ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( h ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g, i, d ),
                AssertNodeRelationship( i ),
                AssertNodeRelationship( d ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl( (sut, a, b) => sut.MoveTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithNode_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl( (sut, a, b) => sut.MoveTo( a.Key, b ) );
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl( (sut, a, b) => sut.MoveTo( a, b.Key ) );
    }

    [Fact]
    public void MoveTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl( (sut, a, b) => sut.MoveTo( a, b ) );
    }

    private void MoveTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> moveTo)
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };

        var action = Lambda.Of( () => moveTo( sut, sut.Root!, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_ShouldThrowKeyNotFoundException_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveTo( keys[0], keys[1] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveTo_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveTo( keys[1], keys[0] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithNode_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveTo( keys[1], sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveTo( node, key ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveTo( node, key ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNode_ShouldThrowKeyNotFoundException_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveTo( sut.Root!, keys[1] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenParentNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveTo( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenParentNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveTo( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveSubtreeTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl( (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveSubtreeTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl(
            (sut, a, b) =>
            {
                sut.MoveSubtreeTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveSubtreeTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl( (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode()
    {
        MoveSubtreeTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl(
            (sut, a, b) =>
            {
                sut.MoveSubtreeTo( a, b );
                return b;
            } );
    }

    private void MoveSubtreeTo_ShouldDoNothing_WhenParentIsTheCurrentParentOfTheNode_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );

        var result = moveSubtreeTo( sut, a, b );

        Assertion.All(
                result.TestEquals( b ),
                AssertNodeRelationship( a, b ),
                AssertNodeRelationship( b, c ),
                AssertNodeRelationship( c ) )
            .Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveSubtreeTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl( (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
            (sut, a, b) =>
            {
                sut.MoveSubtreeTo( a, b );
                return b;
            } );
    }

    private void MoveSubtreeTo_ShouldChangeParentCorrectly_WhenNodeIsParentsIndirectDescendant_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 9 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 9 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( e, keys[5], values[5] );
        var g = sut.AddTo( e, keys[6], values[6] );
        var h = sut.AddTo( f, keys[7], values[7] );
        var i = sut.AddTo( f, keys[8], values[8] );

        var result = moveSubtreeTo( sut, b, f );

        Assertion.All(
                result.TestEquals( f ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, b, c ),
                AssertNodeRelationship( b, d, e, f ),
                AssertNodeRelationship( c ),
                AssertNodeRelationship( d ),
                AssertNodeRelationship( e, g ),
                AssertNodeRelationship( f, h, i ),
                AssertNodeRelationship( g ),
                AssertNodeRelationship( h ),
                AssertNodeRelationship( i ) )
            .Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
            (sut, a, b) =>
            {
                sut.MoveSubtreeTo( a.Key, b );
                return b;
            } );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated()
    {
        MoveSubtreeTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
            (sut, a, b) =>
            {
                sut.MoveSubtreeTo( a, b );
                return b;
            } );
    }

    private void MoveSubtreeTo_ShouldChangeParentCorrectly_WhenParentAndNodeAreNotDirectlyRelated_Impl(
        Func<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>,
            TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 9 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 9 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( a, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( b, keys[4], values[4] );
        var f = sut.AddTo( c, keys[5], values[5] );
        var g = sut.AddTo( c, keys[6], values[6] );
        var h = sut.AddTo( d, keys[7], values[7] );
        var i = sut.AddTo( g, keys[8], values[8] );

        var result = moveSubtreeTo( sut, g, d );

        Assertion.All(
                result.TestEquals( d ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, b, c ),
                AssertNodeRelationship( b, e ),
                AssertNodeRelationship( e ),
                AssertNodeRelationship( c, f, g ),
                AssertNodeRelationship( f ),
                AssertNodeRelationship( g, i, d ),
                AssertNodeRelationship( i ),
                AssertNodeRelationship( d, h ),
                AssertNodeRelationship( h ) )
            .Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b ) );
    }

    private void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentNodeAndTargetNodeAreEqual_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };

        var action = Lambda.Of( () => moveSubtreeTo( sut, sut.Root!, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsRootChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsRootChild_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsRootChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsRootChild_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsRootChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsRootChild_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldChangeParentCorrectly_WhenNodeIsRootAndParentIsRootChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsRootChild_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b ) );
    }

    private void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsRootChild_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );

        var action = Lambda.Of( () => moveSubtreeTo( sut, b, a ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b ) );
    }

    private void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenNodeIsRootAndParentIsIndirectDescendant_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );

        var action = Lambda.Of( () => moveSubtreeTo( sut, c, a ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesChild_Impl( (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowInvalidOperationException_WhenParentIsNodesChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesChild_Impl( (sut, a, b) => sut.MoveSubtreeTo( a.Key, b ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentIsNodesChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesChild_Impl( (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldThrowInvalidOperationException_WhenParentIsNodesChild()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesChild_Impl( (sut, a, b) => sut.MoveSubtreeTo( a, b ) );
    }

    private void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesChild_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );

        var action = Lambda.Of( () => moveSubtreeTo( sut, c, b ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a.Key, b ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b.Key ) );
    }

    [Fact]
    public void MoveSubtreeTo_WithParentAndTargetNode_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant()
    {
        MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant_Impl(
            (sut, a, b) => sut.MoveSubtreeTo( a, b ) );
    }

    private void MoveSubtreeTo_ShouldThrowInvalidOperationException_WhenParentIsNodesIndirectDescendant_Impl(
        Action<TreeDictionary<TKey, TValue>, TreeDictionaryNode<TKey, TValue>, TreeDictionaryNode<TKey, TValue>> moveSubtreeTo)
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 4 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );
        var d = sut.AddTo( c, keys[3], values[3] );

        var action = Lambda.Of( () => moveSubtreeTo( sut, d, b ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowKeyNotFoundException_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveSubtreeTo( keys[0], keys[1] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveSubtreeTo( keys[1], keys[0] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithNode_ShouldThrowKeyNotFoundException_WhenParentKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveSubtreeTo( keys[1], sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( node, key ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowInvalidOperationException_WhenParentNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( node, key ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNode_ShouldThrowKeyNotFoundException_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var action = Lambda.Of( () => sut.MoveSubtreeTo( sut.Root!, keys[1] ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenParentNodeBelongsToDifferentTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var other = new TreeDictionary<TKey, TValue>();
        var node = other.SetRoot( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void MoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowInvalidOperationException_WhenParentNodeIsNotLinkedToAnyTree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        var action = Lambda.Of( () => sut.MoveSubtreeTo( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItemsAndClearAllNodes()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 5 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 5 );
        var sut = new TreeDictionary<TKey, TValue>();
        sut.SetRoot( keys[0], values[0] );
        sut.AddTo( keys[0], keys[1], values[1] );
        sut.AddTo( keys[0], keys[2], values[2] );
        sut.AddTo( keys[1], keys[3], values[3] );
        sut.AddTo( keys[1], keys[4], values[4] );

        var nodes = sut.Nodes.ToList();

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Root.TestNull(),
                nodes.TestAll( (node, _) => AssertLackOfLinkedTree( node ) ) )
            .Go();
    }

    [Fact]
    public void CreateSubtree_ShouldReturnEmpty_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], value } };

        var result = sut.CreateSubtree( keys[1] );

        Assertion.All(
                result.Count.TestEquals( 0 ),
                result.Root.TestNull(),
                result.Comparer.TestRefEquals( sut.Comparer ) )
            .Go();
    }

    [Fact]
    public void CreateSubtree_ShouldReturnCorrectResult_WhenKeyExists()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 5 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 5 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( a, keys[1], values[1] );
        var c = sut.AddTo( b, keys[2], values[2] );
        var d = sut.AddTo( b, keys[3], values[3] );
        var e = sut.AddTo( d, keys[4], values[4] );

        var result = sut.CreateSubtree( b.Key );

        var rb = result.GetNode( b.Key );
        var rc = result.GetNode( c.Key );
        var rd = result.GetNode( d.Key );
        var re = result.GetNode( e.Key );

        Assertion.All(
                rb.TestNotNull(),
                rc.TestNotNull(),
                rd.TestNotNull(),
                re.TestNotNull(),
                result.Count.TestEquals( 4 ),
                result.Root.TestEquals( rb ),
                result.Comparer.TestRefEquals( sut.Comparer ) )
            .Go();

        Assertion.All(
                rb!.Parent.TestNull(),
                AssertNodeRelationship( rb, rc!, rd! ),
                AssertNodeRelationship( rd!, re! ),
                AssertNodeRelationship( re! ) )
            .Go();
    }

    [Fact]
    public void GetNode_ShouldReturnCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var expected = sut.Add( key, value );

        var result = sut.GetNode( key );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetNode_ShouldReturnNull_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new TreeDictionary<TKey, TValue>();

        var result = sut.GetNode( key );

        result.TestNull().Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new TreeDictionary<TKey, TValue>();

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var expected = keys.Zip( values, KeyValuePair.Create ).ToList();
        var sut = new TreeDictionary<TKey, TValue>();

        foreach ( var (k, v) in expected ) sut.Add( k, v );

        var result = sut.AsEnumerable();

        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewRootNodeCorrectly_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();

        sut[key] = value;
        var result = sut.GetNode( key )!;

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( value ),
                sut.Root.TestEquals( result ),
                result.Key.TestEquals( key ),
                result.Value.TestEquals( value ),
                result.Parent.TestNull(),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( result ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewNodeAsRootChildCorrectly_WhenDictionaryHasRoot()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var root = sut.SetRoot( keys[0], values[0] );

        sut[keys[1]] = values[1];
        var result = sut.GetNode( keys[1] )!;

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestEquals( values[1] ),
                sut.Root.TestEquals( root ),
                result.Key.TestEquals( keys[1] ),
                result.Value.TestEquals( values[1] ),
                result.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( root, result ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldReplaceExistingItemCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var node = sut.SetRoot( key, values[0] );

        sut[key] = values[1];

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( values[1] ),
                sut.Root.TestEquals( node ),
                node.Key.TestEquals( key ),
                node.Value.TestEquals( values[1] ),
                node.Tree.TestRefEquals( sut ),
                AssertNodeRelationship( node ) )
            .Go();
    }

    [Fact]
    public void Keys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) ) sut.Add( k, v );

        var result = sut.Keys;

        result.TestSetEqual( keys ).Go();
    }

    [Fact]
    public void Values_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) ) sut.Add( k, v );

        var result = sut.Values;

        result.TestSetEqual( values ).Go();
    }

    [Fact]
    public void Nodes_ShouldReturnEmpty_WhenDictionaryIsEmpty()
    {
        var sut = new TreeDictionary<TKey, TValue>();
        var result = sut.Nodes;
        result.TestEmpty().Go();
    }

    [Fact]
    public void Nodes_ShouldReturnCorrectResultAccordingToBreadthFirstTraversal()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 10 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 10 );
        var sut = new TreeDictionary<TKey, TValue>();

        var a = sut.SetRoot( keys[0], values[0] );
        var b = sut.AddTo( keys[0], keys[1], values[1] );
        var e = sut.AddTo( keys[1], keys[2], values[2] );
        var f = sut.AddTo( keys[1], keys[3], values[3] );
        var c = sut.AddTo( keys[0], keys[4], values[4] );
        var d = sut.AddTo( keys[0], keys[5], values[5] );
        var i = sut.AddTo( keys[3], keys[6], values[6] );
        var j = sut.AddTo( keys[6], keys[7], values[7] );
        var g = sut.AddTo( keys[5], keys[8], values[8] );
        var h = sut.AddTo( keys[5], keys[9], values[9] );

        var result = sut.Nodes;

        result.TestSequence( [ a, b, c, d, e, f, g, h, i, j ] ).Go();
    }

    [Fact]
    public void TreeDictionaryNode_ToString_ShouldReturnCorrectResult()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var node = sut.SetRoot( key, value );
        var expected = $"{key} => {value}";

        var result = node.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyTreeDictionaryGetNode_ShouldBeEquivalentToGetNode()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var expected = sut.GetNode( key );

        var result = (( IReadOnlyTreeDictionary<TKey, TValue> )sut).GetNode( key );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyTreeDictionaryCreateSubtree_ShouldBeEquivalentToCreateSubtree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var expected = sut.CreateSubtree( key );

        var result = (( IReadOnlyTreeDictionary<TKey, TValue> )sut).CreateSubtree( key );

        result.AsEnumerable().TestSetEqual( expected ).Go();
    }

    [Fact]
    public void ITreeDictionarySetRoot_ShouldBeEquivalentToSetRoot()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();

        var result = (( ITreeDictionary<TKey, TValue> )sut).SetRoot( key, value );

        result.TestEquals( sut.Root ).Go();
    }

    [Fact]
    public void ITreeDictionarySetRoot_WithNode_ShouldBeEquivalentToSetRoot()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );

        (( ITreeDictionary<TKey, TValue> )sut).SetRoot( node );

        node.TestEquals( sut.Root ).Go();
    }

    [Fact]
    public void ITreeDictionarySetRoot_WithNode_ShouldThrowArgumentException_WhenNodeIsOfInvalidType()
    {
        var sut = new TreeDictionary<TKey, TValue>();
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).SetRoot( node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryAdd_ShouldBeEquivalentToAdd()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).Add( keys[1], values[1] );

        result.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAdd_WithNode_ShouldBeEquivalentToAdd()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], values[1] );

        (( ITreeDictionary<TKey, TValue> )sut).Add( node );

        node.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAdd_WithNode_ShouldThrowArgumentException_WhenNodeIsOfInvalidType()
    {
        var sut = new TreeDictionary<TKey, TValue>();
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).Add( node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_ShouldBeEquivalentToAddTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).AddTo( keys[0], keys[1], values[1] );

        result.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithNode_ShouldBeEquivalentToAddTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], values[1] );

        (( ITreeDictionary<TKey, TValue> )sut).AddTo( keys[0], node );

        node.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithNode_ShouldThrowArgumentException_WhenNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).AddTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithParentNode_ShouldBeEquivalentToAddTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).AddTo( sut.Root!, keys[1], values[1] );

        result.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithParentNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).AddTo( parent, keys[1], values[1] ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithParentNodeAndNewNode_ShouldBeEquivalentToAddTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], values[1] );

        (( ITreeDictionary<TKey, TValue> )sut).AddTo( sut.Root!, node );

        node.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithParentNodeAndNewNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();
        var node = new TreeDictionaryNode<TKey, TValue>( keys[1], values[1] );

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).AddTo( parent, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddTo_WithParentNodeAndNewNode_ShouldThrowArgumentException_WhenNewNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).AddTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddSubtree_ShouldBeEquivalentToAddSubtree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var subtree = new TreeDictionary<TKey, TValue> { { key, value } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).AddSubtree( subtree.Root! );

        result.TestEquals( sut.Root ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddSubtreeTo_ShouldBeEquivalentToAddSubtreeTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var subtree = new TreeDictionary<TKey, TValue> { { keys[1], values[1] } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).AddSubtreeTo( keys[0], subtree.Root! );

        result.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddSubtreeTo_WithNode_ShouldBeEquivalentToAddSubtreeTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue> { { keys[0], values[0] } };
        var subtree = new TreeDictionary<TKey, TValue> { { keys[1], values[1] } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).AddSubtreeTo( sut.Root!, subtree.Root! );

        result.TestEquals( sut.Root!.Children[0] ).Go();
    }

    [Fact]
    public void ITreeDictionaryAddSubtreeTo_WithNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue>();
        var subtree = new TreeDictionary<TKey, TValue> { { key, value } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).AddSubtreeTo( parent, subtree.Root! ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryRemove_WithNode_ShouldBeEquivalentToRemove()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };

        (( ITreeDictionary<TKey, TValue> )sut).Remove( sut.Root! );

        sut.TestEmpty().Go();
    }

    [Fact]
    public void ITreeDictionaryRemove_WithNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var sut = new TreeDictionary<TKey, TValue>();
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).Remove( node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryRemoveSubtree_WithNode_ShouldBeEquivalentToRemoveSubtree()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };

        var result = (( ITreeDictionary<TKey, TValue> )sut).RemoveSubtree( sut.Root! );

        Assertion.All(
                result.TestEquals( 1 ),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryRemoveSubtree_WithNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var sut = new TreeDictionary<TKey, TValue>();
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).RemoveSubtree( node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionarySwap_WithNodes_ShouldBeEquivalentToSwap()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );

        (( ITreeDictionary<TKey, TValue> )sut).Swap( a, b );

        Assertion.All(
                b.Parent.TestNull(),
                AssertNodeRelationship( b, a ),
                AssertNodeRelationship( a ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionarySwap_WithNodes_ShouldThrowArgumentException_WhenFirstNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).Swap( node, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionarySwap_WithNodes_ShouldThrowArgumentException_WhenSecondNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).Swap( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );

        var result = (( ITreeDictionary<TKey, TValue> )sut).MoveTo( keys[1], keys[0] );

        Assertion.All(
                result.TestEquals( a ),
                sut.Root.TestEquals( b ),
                AssertNodeRelationship( b, a ),
                AssertNodeRelationship( a ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithNode_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );

        (( ITreeDictionary<TKey, TValue> )sut).MoveTo( keys[1], a );

        Assertion.All(
                sut.Root.TestEquals( b ),
                AssertNodeRelationship( b, a ),
                AssertNodeRelationship( a ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithNode_ShouldThrowArgumentException_WhenNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithParentNode_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );

        var result = (( ITreeDictionary<TKey, TValue> )sut).MoveTo( b, keys[0] );

        Assertion.All(
                result.TestEquals( a ),
                sut.Root.TestEquals( b ),
                AssertNodeRelationship( b, a ),
                AssertNodeRelationship( a ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithParentNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveTo( parent, key ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithParentNodeAndTargetNode_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );

        (( ITreeDictionary<TKey, TValue> )sut).MoveTo( b, a );

        Assertion.All(
                sut.Root.TestEquals( b ),
                AssertNodeRelationship( b, a ),
                AssertNodeRelationship( a ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithParentNodeAndTargetNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveTo( parent, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveTo_WithParentNodeAndTargetNode_ShouldThrowArgumentException_WhenTargetNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );
        var c = sut.Add( keys[2], values[2] );

        var result = (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( keys[2], keys[1] );

        Assertion.All(
                result.TestEquals( b ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, c ),
                AssertNodeRelationship( c, b ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithNode_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );
        var c = sut.Add( keys[2], values[2] );

        (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( keys[2], b );

        Assertion.All(
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, c ),
                AssertNodeRelationship( c, b ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithNode_ShouldThrowArgumentException_WhenNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( key, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithParentNode_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );
        var c = sut.Add( keys[2], values[2] );

        var result = (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( c, keys[1] );

        Assertion.All(
                result.TestEquals( b ),
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, c ),
                AssertNodeRelationship( c, b ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithParentNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( parent, key ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithParentNodeAndTargetNode_ShouldBeEquivalentToMoveTo()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new TreeDictionary<TKey, TValue>();
        var a = sut.Add( keys[0], values[0] );
        var b = sut.Add( keys[1], values[1] );
        var c = sut.Add( keys[2], values[2] );

        (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( c, b );

        Assertion.All(
                sut.Root.TestEquals( a ),
                AssertNodeRelationship( a, c ),
                AssertNodeRelationship( c, b ),
                AssertNodeRelationship( b ) )
            .Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowArgumentException_WhenParentNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var parent = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( parent, sut.Root! ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void ITreeDictionaryMoveSubtreeTo_WithParentNodeAndTargetNode_ShouldThrowArgumentException_WhenTargetNodeIsOfInvalidType()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new TreeDictionary<TKey, TValue> { { key, value } };
        var node = Substitute.For<ITreeDictionaryNode<TKey, TValue>>();

        var action = Lambda.Of( () => (( ITreeDictionary<TKey, TValue> )sut).MoveSubtreeTo( sut.Root!, node ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    protected sealed override IDictionary<TKey, TValue> CreateEmptyDictionary()
    {
        return new TreeDictionary<TKey, TValue>();
    }

    [Pure]
    private static Assertion AssertNodeRelationship(
        TreeDictionaryNode<TKey, TValue> parent,
        params TreeDictionaryNode<TKey, TValue>[] children)
    {
        return Assertion.All(
            "NodeRelationship",
            parent.Children.Select( c => c.Key ).TestSequence( children.Select( c => c.Key ) ),
            children.Select( c => c.Parent!.Key ).TestAll( (pk, _) => pk.TestEquals( parent.Key ) ) );
    }

    [Pure]
    private static Assertion AssertLackOfLinkedTree(TreeDictionaryNode<TKey, TValue> node)
    {
        return Assertion.All(
            "LackOfLinkedTree",
            node.Tree.TestNull(),
            node.Parent.TestNull(),
            node.Children.TestEmpty() );
    }

    private sealed class ThrowResultInterceptor
    {
        public TreeDictionaryNode<TKey, TValue>? Node;
    }
}
