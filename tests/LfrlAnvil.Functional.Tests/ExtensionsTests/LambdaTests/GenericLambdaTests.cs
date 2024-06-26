﻿using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Functional.Delegates;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.LambdaTests;

public abstract class GenericLambdaTests<T1, T2, T3, T4, T5, T6, T7, TReturn> : TestsBase
    where TReturn : notnull
{
    [Fact]
    public void TryInvoke_WithAction_ShouldReturnResultWithoutError_WhenDelegateDoesntThrow()
    {
        Action action = () => { };
        var result = action.TryInvoke();
        result.IsOk.Should().BeTrue();
    }

    [Fact]
    public void TryInvoke_WithAction_ShouldReturnResultWithError_WhenDelegateThrows()
    {
        var error = new Exception();
        Action action = () => throw error;

        var result = action.TryInvoke();

        using ( new AssertionScope() )
        {
            result.HasError.Should().BeTrue();
            result.Error.Should().Be( error );
        }
    }

    [Fact]
    public void TryInvoke_WithFunc_ShouldReturnResultWithCorrectValue_WhenDelegateDoesntThrow()
    {
        var value = Fixture.Create<TReturn>();
        Func<TReturn> action = () => value;

        var result = action.TryInvoke();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeTrue();
            result.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TryInvoke_WithFunc_ShouldReturnResultWithError_WhenDelegateThrows()
    {
        var error = new Exception();
        Func<TReturn> action = () => throw error;

        var result = action.TryInvoke();

        using ( new AssertionScope() )
        {
            result.HasError.Should().BeTrue();
            result.Error.Should().Be( error );
        }
    }

    [Fact]
    public void ToFunc_0_ShouldReturnCorrectResult()
    {
        var action = Substitute.For<Action>();
        var sut = action.ToFunc();

        sut();

        action.Verify().CallCount.Should().Be( 1 );
    }

    [Fact]
    public void ToFunc_1_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var action = Substitute.For<Action<T1>>();
        var sut = action.ToFunc();

        sut( a1 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1 );
    }

    [Fact]
    public void ToFunc_2_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var action = Substitute.For<Action<T1, T2>>();
        var sut = action.ToFunc();

        sut( a1, a2 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2 );
    }

    [Fact]
    public void ToFunc_3_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var action = Substitute.For<Action<T1, T2, T3>>();
        var sut = action.ToFunc();

        sut( a1, a2, a3 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3 );
    }

    [Fact]
    public void ToFunc_4_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var action = Substitute.For<Action<T1, T2, T3, T4>>();
        var sut = action.ToFunc();

        sut( a1, a2, a3, a4 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4 );
    }

    [Fact]
    public void ToFunc_5_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var a5 = Fixture.Create<T5>();
        var action = Substitute.For<Action<T1, T2, T3, T4, T5>>();
        var sut = action.ToFunc();

        sut( a1, a2, a3, a4, a5 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4, a5 );
    }

    [Fact]
    public void ToFunc_6_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var a5 = Fixture.Create<T5>();
        var a6 = Fixture.Create<T6>();
        var action = Substitute.For<Action<T1, T2, T3, T4, T5, T6>>();
        var sut = action.ToFunc();

        sut( a1, a2, a3, a4, a5, a6 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4, a5, a6 );
    }

    [Fact]
    public void ToFunc_7_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var a5 = Fixture.Create<T5>();
        var a6 = Fixture.Create<T6>();
        var a7 = Fixture.Create<T7>();
        var action = Substitute.For<Action<T1, T2, T3, T4, T5, T6, T7>>();
        var sut = action.ToFunc();

        sut( a1, a2, a3, a4, a5, a6, a7 );

        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4, a5, a6, a7 );
    }

    [Fact]
    public void ToAction_0_ShouldReturnCorrectResult()
    {
        var func = Substitute.For<Func<Nil>>();
        var sut = func.ToAction();

        sut();

        func.Verify().CallCount.Should().Be( 1 );
    }

    [Fact]
    public void ToAction_1_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var func = Substitute.For<Func<T1, Nil>>();
        var sut = func.ToAction();

        sut( a1 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1 );
    }

    [Fact]
    public void ToAction_2_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var func = Substitute.For<Func<T1, T2, Nil>>();
        var sut = func.ToAction();

        sut( a1, a2 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2 );
    }

    [Fact]
    public void ToAction_3_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var func = Substitute.For<Func<T1, T2, T3, Nil>>();
        var sut = func.ToAction();

        sut( a1, a2, a3 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3 );
    }

    [Fact]
    public void ToAction_4_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var func = Substitute.For<Func<T1, T2, T3, T4, Nil>>();
        var sut = func.ToAction();

        sut( a1, a2, a3, a4 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4 );
    }

    [Fact]
    public void ToAction_5_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var a5 = Fixture.Create<T5>();
        var func = Substitute.For<Func<T1, T2, T3, T4, T5, Nil>>();
        var sut = func.ToAction();

        sut( a1, a2, a3, a4, a5 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4, a5 );
    }

    [Fact]
    public void ToAction_6_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var a5 = Fixture.Create<T5>();
        var a6 = Fixture.Create<T6>();
        var func = Substitute.For<Func<T1, T2, T3, T4, T5, T6, Nil>>();
        var sut = func.ToAction();

        sut( a1, a2, a3, a4, a5, a6 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4, a5, a6 );
    }

    [Fact]
    public void ToAction_7_ShouldReturnCorrectResult()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var a4 = Fixture.Create<T4>();
        var a5 = Fixture.Create<T5>();
        var a6 = Fixture.Create<T6>();
        var a7 = Fixture.Create<T7>();
        var func = Substitute.For<Func<T1, T2, T3, T4, T5, T6, T7, Nil>>();
        var sut = func.ToAction();

        sut( a1, a2, a3, a4, a5, a6, a7 );

        func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( a1, a2, a3, a4, a5, a6, a7 );
    }

    [Fact]
    public void Purify_0_ShouldReturnNone_WhenTargetReturnsFalse()
    {
        var func = Substitute.For<OutFunc<TReturn>>();
        func.Invoke( out Arg.Any<TReturn>() ).Returns( _ => false );

        var sut = func.Purify();

        var result = sut();

        using ( new AssertionScope() )
        {
            func.Verify().CallCount.Should().Be( 1 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void Purify_0_ShouldReturnWithValue_WhenTargetReturnsTrue()
    {
        var expected = Fixture.Create<TReturn>();
        var func = Substitute.For<OutFunc<TReturn>>();
        func.Invoke( out Arg.Any<TReturn>() )
            .Returns(
                c =>
                {
                    c[0] = expected;
                    return true;
                } );

        var sut = func.Purify();

        var result = sut();

        using ( new AssertionScope() )
        {
            func.Verify().CallCount.Should().Be( 1 );
            result.Value.Should().Be( expected );
        }
    }

    [Fact]
    public void Purify_1_ShouldReturnNone_WhenTargetReturnsFalse()
    {
        var a1 = Fixture.Create<T1>();
        var func = Substitute.For<OutFunc<T1, TReturn>>();
        func.Invoke( Arg.Any<T1>(), out Arg.Any<TReturn>() ).Returns( _ => false );

        var sut = func.Purify();

        var result = sut( a1 );

        using ( new AssertionScope() )
        {
            func.Verify().CallAt( 0 ).Exists().And.Arguments.SkipLast( 1 ).Should().BeSequentiallyEqualTo( a1 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void Purify_1_ShouldReturnWithValue_WhenTargetReturnsTrue()
    {
        var a1 = Fixture.Create<T1>();
        var expected = Fixture.Create<TReturn>();
        var func = Substitute.For<OutFunc<T1, TReturn>>();
        func.Invoke( Arg.Any<T1>(), out Arg.Any<TReturn>() )
            .Returns(
                c =>
                {
                    c[1] = expected;
                    return true;
                } );

        var sut = func.Purify();

        var result = sut( a1 );

        using ( new AssertionScope() )
        {
            func.Verify().CallAt( 0 ).Exists().And.Arguments.SkipLast( 1 ).Should().BeSequentiallyEqualTo( a1 );
            result.Value.Should().Be( expected );
        }
    }

    [Fact]
    public void Purify_2_ShouldReturnNone_WhenTargetReturnsFalse()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var func = Substitute.For<OutFunc<T1, T2, TReturn>>();
        func.Invoke( Arg.Any<T1>(), Arg.Any<T2>(), out Arg.Any<TReturn>() ).Returns( _ => false );

        var sut = func.Purify();

        var result = sut( a1, a2 );

        using ( new AssertionScope() )
        {
            func.Verify().CallAt( 0 ).Exists().And.Arguments.SkipLast( 1 ).Should().BeSequentiallyEqualTo( a1, a2 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void Purify_2_ShouldReturnWithValue_WhenTargetReturnsTrue()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var expected = Fixture.Create<TReturn>();
        var func = Substitute.For<OutFunc<T1, T2, TReturn>>();
        func.Invoke( Arg.Any<T1>(), Arg.Any<T2>(), out Arg.Any<TReturn>() )
            .Returns(
                c =>
                {
                    c[2] = expected;
                    return true;
                } );

        var sut = func.Purify();

        var result = sut( a1, a2 );

        using ( new AssertionScope() )
        {
            func.Verify().CallAt( 0 ).Exists().And.Arguments.SkipLast( 1 ).Should().BeSequentiallyEqualTo( a1, a2 );
            result.Value.Should().Be( expected );
        }
    }

    [Fact]
    public void Purify_3_ShouldReturnNone_WhenTargetReturnsFalse()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var func = Substitute.For<OutFunc<T1, T2, T3, TReturn>>();
        func.Invoke( Arg.Any<T1>(), Arg.Any<T2>(), Arg.Any<T3>(), out Arg.Any<TReturn>() ).Returns( _ => false );

        var sut = func.Purify();

        var result = sut( a1, a2, a3 );

        using ( new AssertionScope() )
        {
            func.Verify().CallAt( 0 ).Exists().And.Arguments.SkipLast( 1 ).Should().BeSequentiallyEqualTo( a1, a2, a3 );
            result.HasValue.Should().BeFalse();
        }
    }

    [Fact]
    public void Purify_3_ShouldReturnWithValue_WhenTargetReturnsTrue()
    {
        var a1 = Fixture.Create<T1>();
        var a2 = Fixture.Create<T2>();
        var a3 = Fixture.Create<T3>();
        var expected = Fixture.Create<TReturn>();
        var func = Substitute.For<OutFunc<T1, T2, T3, TReturn>>();
        func.Invoke( Arg.Any<T1>(), Arg.Any<T2>(), Arg.Any<T3>(), out Arg.Any<TReturn>() )
            .Returns(
                c =>
                {
                    c[3] = expected;
                    return true;
                } );

        var sut = func.Purify();

        var result = sut( a1, a2, a3 );

        using ( new AssertionScope() )
        {
            func.Verify().CallAt( 0 ).Exists().And.Arguments.SkipLast( 1 ).Should().BeSequentiallyEqualTo( a1, a2, a3 );
            result.Value.Should().Be( expected );
        }
    }

    [Fact]
    public void Identity_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T1>();
        var sut = Lambda<T1>.Identity;

        var result = sut( value );

        result.Should().Be( value );
    }

    [Fact]
    public void Of_Func0_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func1_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func2_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, T2, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func3_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, T2, T3, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func4_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, T2, T3, T4, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func5_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, T2, T3, T4, T5, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func6_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, T2, T3, T4, T5, T6, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Func7_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action0_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action1_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action2_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1, T2>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action3_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1, T2, T3>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action4_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1, T2, T3, T4>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action5_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1, T2, T3, T4, T5>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action6_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1, T2, T3, T4, T5, T6>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_Action7_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<Action<T1, T2, T3, T4, T5, T6, T7>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_OutFunc0_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<OutFunc<TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_OutFunc1_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<OutFunc<T1, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_OutFunc2_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<OutFunc<T1, T2, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Of_OutFunc3_ShouldReturnCorrectResult()
    {
        var sut = Substitute.For<OutFunc<T1, T2, T3, TReturn>>();
        var result = Lambda.Of( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func0_ShouldReturnCorrectResult()
    {
        Expression<Func<TReturn>> sut = () => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func1_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, TReturn>> sut = _ => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func2_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, T2, TReturn>> sut = (_, _) => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func3_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, T2, T3, TReturn>> sut = (_, _, _) => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func4_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, T2, T3, T4, TReturn>> sut = (_, _, _, _) => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func5_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, T2, T3, T4, T5, TReturn>> sut = (_, _, _, _, _) => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func6_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> sut = (_, _, _, _, _, _) => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void ExpressionOf_Func7_ShouldReturnCorrectResult()
    {
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> sut = (_, _, _, _, _, _, _) => Fixture.Create<TReturn>();
        var result = Lambda.ExpressionOf( sut );
        result.Should().BeSameAs( sut );
    }
}
