using System.Collections.Generic;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.ExtensionsTests.ExpressionTests;

public class ExpressionExtensionsTests : TestsBase
{
    [Fact]
    public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsField()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Field;
        var result = sut.GetMemberName();
        result.Should().Be( nameof( TestClass.Field ) );
    }

    [Fact]
    public void GetMemberName_ShouldReturnCorrectResult_WhenMemberIsProperty()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Property;
        var result = sut.GetMemberName();
        result.Should().Be( nameof( TestClass.Property ) );
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsMethodCall()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Method();
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsStaticMember()
    {
        Expression<Func<TestClass, string?>> sut = _ => TestClass.StaticProperty;
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsStaticMemberFromDifferentType()
    {
        Expression<Func<TestClass, string>> sut = _ => string.Empty;
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetMemberName_ShouldThrowArgumentException_WhenBodyIsAccessingMemberOfMember()
    {
        Expression<Func<TestClass, string?>> sut = t => t.Other!.Property;
        var action = Lambda.Of( () => sut.GetMemberName() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryGetValue_ShouldReturnCorrectResult_WhenExpressionHasValueOfTheSpecifiedType()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Expression.Constant( value );

        var result = sut.TryGetValue<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnCorrectResult_WhenExpressionHasValueAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.TryGetValue<object>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenExpressionHasValueNotAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.TryGetValue<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenExpressionHasNullValue()
    {
        var sut = Expression.Constant( null, typeof( string ) );

        var result = sut.TryGetValue<string>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnCorrectResult_WhenExpressionHasValueOfTheSpecifiedType()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Expression.Constant( value );

        var result = sut.GetValueOrDefault<int>();

        result.Should().Be( value );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnCorrectResult_WhenExpressionHasValueAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.GetValueOrDefault<object>();

        result.Should().Be( value );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDefault_WhenExpressionHasValueNotAssignableToTheSpecifiedType()
    {
        var value = Fixture.Create<string>();
        var sut = Expression.Constant( value );

        var result = sut.GetValueOrDefault<int>();

        result.Should().Be( default );
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnNull_WhenExpressionHasNullValue()
    {
        var sut = Expression.Constant( null, typeof( string ) );
        var result = sut.GetValueOrDefault<string>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetOrConvert_ShouldReturnParameter_WhenTypesAreEqual()
    {
        var sut = Expression.Constant( "foo", typeof( string ) );
        var result = sut.GetOrConvert<string>();
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void GetOrConvert_ShouldReturnConvertedParameter_WhenTypesAreNotEqual()
    {
        var sut = Expression.Constant( "foo", typeof( string ) );
        var result = sut.GetOrConvert<IEnumerable<char>>();

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( sut );
            result.NodeType.Should().Be( ExpressionType.Convert );
            if ( result is not UnaryExpression unary )
                return;

            unary.Type.Should().Be( typeof( IEnumerable<char> ) );
            unary.Operand.Should().BeSameAs( sut );
        }
    }

    [Fact]
    public void IsNullReference_ShouldReturnIsTrueNodeWithNestedReferenceEqual()
    {
        var sut = Expression.Constant( "foo", typeof( string ) );
        var result = sut.IsNullReference();

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Equal );
            result.Type.Should().Be( typeof( bool ) );
            result.Left.Should().BeSameAs( sut );
            result.Right.NodeType.Should().Be( ExpressionType.Constant );
            if ( result.Right is not ConstantExpression nullConst )
                return;

            nullConst.Type.Should().Be( sut.Type );
            nullConst.Value.Should().BeNull();
        }
    }

    [Fact]
    public void IsNotNullReference_ShouldReturnIsTrueNodeWithNestedNotReferenceEqual()
    {
        var sut = Expression.Constant( "foo", typeof( string ) );
        var result = sut.IsNotNullReference();

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.NotEqual );
            result.Type.Should().Be( typeof( bool ) );
            result.Left.Should().BeSameAs( sut );
            result.Right.NodeType.Should().Be( ExpressionType.Constant );
            if ( result.Right is not ConstantExpression nullConst )
                return;

            nullConst.Type.Should().Be( sut.Type );
            nullConst.Value.Should().BeNull();
        }
    }

    [Fact]
    public void ToForEachLoop_Create_ShouldCreateCorrectForEachLoopBlock_WhenEnumeratorIsNotDisposable()
    {
        var sut = new EnumerableWithNonDisposableEnumerator();

        var list = Expression.Variable( typeof( List<int> ) );
        var creator = Expression.Constant( sut ).ToForEachLoop();
        var loop = creator.Create(
            Expression.Block( creator.CurrentAssignment, Expression.Call( list, "Add", Type.EmptyTypes, creator.Current ) ) );

        var lambda = Expression.Lambda<Func<List<int>>>(
            Expression.Block( new[] { list }, Expression.Assign( list, Expression.New( list.Type ) ), loop, list ) );

        var @delegate = lambda.Compile();

        var result = @delegate();

        result.Should().BeSequentiallyEqualTo( 0, 1, 2 );
    }

    [Fact]
    public void ToForEachLoop_Create_ShouldCreateCorrectForEachLoopBlock_WhenEnumeratorIsDisposableValueType()
    {
        var sut = new EnumerableWithDisposableValueTypeEnumerator { ThrowException = false };

        var list = Expression.Variable( typeof( List<int> ) );
        var creator = Expression.Constant( sut ).ToForEachLoop();
        var loop = creator.Create(
            Expression.Block( creator.CurrentAssignment, Expression.Call( list, "Add", Type.EmptyTypes, creator.Current ) ) );

        var lambda = Expression.Lambda<Func<List<int>>>(
            Expression.Block( new[] { list }, Expression.Assign( list, Expression.New( list.Type ) ), loop, list ) );

        var @delegate = lambda.Compile();

        var result = @delegate();

        using ( new AssertionScope() )
        {
            sut.DisposeCalled.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( 0, 1, 2 );
        }
    }

    [Fact]
    public void ToForEachLoop_Create_ShouldCreateCorrectForEachLoopBlock_WhenEnumeratorIsDisposableValueTypeAndGetEnumeratorThrows()
    {
        var sut = new EnumerableWithDisposableValueTypeEnumerator { ThrowException = true };

        var list = Expression.Variable( typeof( List<int> ) );
        var creator = Expression.Constant( sut ).ToForEachLoop();
        var loop = creator.Create(
            Expression.Block( creator.CurrentAssignment, Expression.Call( list, "Add", Type.EmptyTypes, creator.Current ) ) );

        var lambda = Expression.Lambda<Func<List<int>>>(
            Expression.Block( new[] { list }, Expression.Assign( list, Expression.New( list.Type ) ), loop, list ) );

        var @delegate = lambda.Compile();

        var action = Lambda.Of( () => @delegate() );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<Exception>();
            sut.DisposeCalled.Should().BeFalse();
        }
    }

    [Fact]
    public void ToForEachLoop_Create_ShouldCreateCorrectForEachLoopBlock_WhenEnumeratorIsDisposableRefType()
    {
        var sut = new EnumerableWithDisposableRefTypeEnumerator { ThrowException = false };

        var list = Expression.Variable( typeof( List<int> ) );
        var creator = Expression.Constant( sut ).ToForEachLoop();
        var loop = creator.Create(
            Expression.Block( creator.CurrentAssignment, Expression.Call( list, "Add", Type.EmptyTypes, creator.Current ) ) );

        var lambda = Expression.Lambda<Func<List<int>>>(
            Expression.Block( new[] { list }, Expression.Assign( list, Expression.New( list.Type ) ), loop, list ) );

        var @delegate = lambda.Compile();

        var result = @delegate();

        using ( new AssertionScope() )
        {
            sut.DisposeCalled.Should().BeTrue();
            result.Should().BeSequentiallyEqualTo( 0, 1, 2 );
        }
    }

    [Fact]
    public void ToForEachLoop_Create_ShouldCreateCorrectForEachLoopBlock_WhenEnumeratorIsDisposableRefTypeAndGetEnumeratorThrows()
    {
        var sut = new EnumerableWithDisposableRefTypeEnumerator { ThrowException = true };

        var list = Expression.Variable( typeof( List<int> ) );
        var creator = Expression.Constant( sut ).ToForEachLoop();
        var loop = creator.Create(
            Expression.Block( creator.CurrentAssignment, Expression.Call( list, "Add", Type.EmptyTypes, creator.Current ) ) );

        var lambda = Expression.Lambda<Func<List<int>>>(
            Expression.Block( new[] { list }, Expression.Assign( list, Expression.New( list.Type ) ), loop, list ) );

        var @delegate = lambda.Compile();

        var action = Lambda.Of( () => @delegate() );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<Exception>();
            sut.DisposeCalled.Should().BeFalse();
        }
    }

    [Fact]
    public void ToForEachLoop_Create_ShouldCreateCorrectForEachLoopBlock_WhenEnumerableIsGenericArray()
    {
        var sut = new[] { 0, 1, 2 };

        var list = Expression.Variable( typeof( List<int> ) );
        var creator = Expression.Constant( sut ).ToForEachLoop();
        var loop = creator.Create(
            Expression.Block( creator.CurrentAssignment, Expression.Call( list, "Add", Type.EmptyTypes, creator.Current ) ) );

        var lambda = Expression.Lambda<Func<List<int>>>(
            Expression.Block( new[] { list }, Expression.Assign( list, Expression.New( list.Type ) ), loop, list ) );

        var @delegate = lambda.Compile();

        var result = @delegate();

        result.Should().BeSequentiallyEqualTo( 0, 1, 2 );
    }

    [Fact]
    public void ReplaceParametersByName_ShouldInjectNewExpressionsInPlaceOfSpecifiedNamedParameterExpressions()
    {
        var p1 = Expression.Parameter( typeof( int ), "p1" );
        var p2 = Expression.Parameter( typeof( int ), "p2" );
        var p3 = Expression.Parameter( typeof( int ), "p3" );
        var pNoName = Expression.Parameter( typeof( int ) );
        var c1 = Expression.Constant( 0 );

        var p1Replacement = Expression.Constant( 10 );
        var p3Replacement = Expression.Constant( 20 );

        var parametersToReplace = new Dictionary<string, Expression>
        {
            { "p1", p1Replacement },
            { "p3", p3Replacement }
        };

        var p1P2Add = Expression.Add( p1, p2 );
        var p3PNoNameAdd = Expression.Add( p3, pNoName );
        var p1P2P3PNoNameAdd = Expression.Add( p1P2Add, p3PNoNameAdd );

        // 0 + ((p1 + p2) + (p3 + pNoName))
        var sut = Expression.Add( c1, p1P2P3PNoNameAdd );

        // 0 + ((10 + p2) + (20 + pNoName))
        var result = sut.ReplaceParametersByName( parametersToReplace );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Add );
            result.Should().BeAssignableTo<BinaryExpression>();
            if ( result is not BinaryExpression newSut )
                return;

            newSut.Left.Should().BeSameAs( c1 );
            newSut.Right.NodeType.Should().Be( ExpressionType.Add );
            newSut.Right.Should().BeAssignableTo<BinaryExpression>();
            if ( newSut.Right is not BinaryExpression newP1P2P3PNoNameAdd )
                return;

            newP1P2P3PNoNameAdd.Left.NodeType.Should().Be( ExpressionType.Add );
            newP1P2P3PNoNameAdd.Right.NodeType.Should().Be( ExpressionType.Add );

            if ( newP1P2P3PNoNameAdd.Left is not BinaryExpression newP1P2Add ||
                newP1P2P3PNoNameAdd.Right is not BinaryExpression newP3PNoNameAdd )
                return;

            newP1P2Add.Left.Should().BeSameAs( p1Replacement );
            newP1P2Add.Right.Should().BeSameAs( p2 );
            newP3PNoNameAdd.Left.Should().BeSameAs( p3Replacement );
            newP3PNoNameAdd.Right.Should().BeSameAs( pNoName );
        }
    }

    [Fact]
    public void ReplaceParameters_ShouldInjectNewExpressionsInPlaceOfSpecifiedParameterExpressions()
    {
        var p1 = Expression.Parameter( typeof( int ), "p" );
        var p2 = Expression.Parameter( typeof( int ), "p" );
        var p3 = Expression.Parameter( typeof( int ), "q" );
        var pNoName = Expression.Parameter( typeof( int ) );
        var c1 = Expression.Constant( 0 );

        var p1Replacement = Expression.Constant( 10 );
        var pNoNameReplacement = Expression.Constant( 20 );

        var parametersToReplace = new[] { p1, pNoName };
        var replacements = new Expression[] { p1Replacement, pNoNameReplacement };

        var p1P2Add = Expression.Add( p1, p2 );
        var p3PNoNameAdd = Expression.Add( p3, pNoName );
        var p1P2P3PNoNameAdd = Expression.Add( p1P2Add, p3PNoNameAdd );

        // 0 + ((p1 + p2) + (p3 + pNoName))
        var sut = Expression.Add( c1, p1P2P3PNoNameAdd );

        // 0 + ((10 + p2) + (p3 + 20))
        var result = sut.ReplaceParameters( parametersToReplace, replacements );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Add );
            result.Should().BeAssignableTo<BinaryExpression>();
            if ( result is not BinaryExpression newSut )
                return;

            newSut.Left.Should().BeSameAs( c1 );
            newSut.Right.NodeType.Should().Be( ExpressionType.Add );
            newSut.Right.Should().BeAssignableTo<BinaryExpression>();
            if ( newSut.Right is not BinaryExpression newP1P2P3PNoNameAdd )
                return;

            newP1P2P3PNoNameAdd.Left.NodeType.Should().Be( ExpressionType.Add );
            newP1P2P3PNoNameAdd.Right.NodeType.Should().Be( ExpressionType.Add );

            if ( newP1P2P3PNoNameAdd.Left is not BinaryExpression newP1P2Add ||
                newP1P2P3PNoNameAdd.Right is not BinaryExpression newP3PNoNameAdd )
                return;

            newP1P2Add.Left.Should().BeSameAs( p1Replacement );
            newP1P2Add.Right.Should().BeSameAs( p2 );
            newP3PNoNameAdd.Left.Should().BeSameAs( p3 );
            newP3PNoNameAdd.Right.Should().BeSameAs( pNoNameReplacement );
        }
    }

    private sealed class EnumerableWithNonDisposableEnumerator
    {
        public Enumerator GetEnumerator()
        {
            return new Enumerator();
        }

        public sealed class Enumerator
        {
            public int Current { get; private set; } = -1;

            public bool MoveNext()
            {
                return ++Current < 3;
            }
        }
    }

    private sealed class EnumerableWithDisposableValueTypeEnumerator
    {
        public bool ThrowException { get; init; }
        public bool DisposeCalled { get; set; } = false;

        public Enumerator GetEnumerator()
        {
            if ( ThrowException )
                throw new Exception();

            return new Enumerator( this );
        }

        public struct Enumerator
        {
            private readonly EnumerableWithDisposableValueTypeEnumerator? _source;
            public int Current { get; private set; }

            internal Enumerator(EnumerableWithDisposableValueTypeEnumerator source)
            {
                _source = source;
                Current = -1;
            }

            public bool MoveNext()
            {
                return ++Current < 3;
            }

            public void Dispose()
            {
                if ( _source is not null )
                    _source.DisposeCalled = true;
            }
        }
    }

    private sealed class EnumerableWithDisposableRefTypeEnumerator
    {
        public bool ThrowException { get; init; }
        public bool DisposeCalled { get; set; } = false;

        public Enumerator GetEnumerator()
        {
            if ( ThrowException )
                throw new Exception();

            return new Enumerator( this );
        }

        public sealed class Enumerator
        {
            private readonly EnumerableWithDisposableRefTypeEnumerator _source;
            public int Current { get; private set; }

            internal Enumerator(EnumerableWithDisposableRefTypeEnumerator source)
            {
                _source = source;
                Current = -1;
            }

            public bool MoveNext()
            {
                return ++Current < 3;
            }

            public void Dispose()
            {
                _source.DisposeCalled = true;
            }
        }
    }

    private class TestClass
    {
        public static string? StaticProperty { get; set; }

        public string? Property { get; set; }
        public string? Field;
        public TestClass? Other { get; set; }

        public string? Method()
        {
            return Property;
        }
    }
}
