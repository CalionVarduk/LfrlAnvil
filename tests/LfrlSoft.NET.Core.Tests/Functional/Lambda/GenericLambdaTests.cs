using System;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Functional.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Lambda
{
    public abstract class GenericLambdaTests<T1, T2, T3, T4, T5, T6, T7, TReturn> : TestsBase
    {
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

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1 );
        }

        [Fact]
        public void ToFunc_2_ShouldReturnCorrectResult()
        {
            var a1 = Fixture.Create<T1>();
            var a2 = Fixture.Create<T2>();
            var action = Substitute.For<Action<T1, T2>>();
            var sut = action.ToFunc();

            sut( a1, a2 );

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2 );
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

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3 );
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

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4 );
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

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4, a5 );
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

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4, a5, a6 );
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

            action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4, a5, a6, a7 );
        }

        [Fact]
        public void ToAction_0_ShouldReturnCorrectResult()
        {
            var func = Substitute.For<Func<Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut();

            func.Verify().CallCount.Should().Be( 1 );
        }

        [Fact]
        public void ToAction_1_ShouldReturnCorrectResult()
        {
            var a1 = Fixture.Create<T1>();
            var func = Substitute.For<Func<T1, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1 );
        }

        [Fact]
        public void ToAction_2_ShouldReturnCorrectResult()
        {
            var a1 = Fixture.Create<T1>();
            var a2 = Fixture.Create<T2>();
            var func = Substitute.For<Func<T1, T2, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1, a2 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2 );
        }

        [Fact]
        public void ToAction_3_ShouldReturnCorrectResult()
        {
            var a1 = Fixture.Create<T1>();
            var a2 = Fixture.Create<T2>();
            var a3 = Fixture.Create<T3>();
            var func = Substitute.For<Func<T1, T2, T3, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1, a2, a3 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3 );
        }

        [Fact]
        public void ToAction_4_ShouldReturnCorrectResult()
        {
            var a1 = Fixture.Create<T1>();
            var a2 = Fixture.Create<T2>();
            var a3 = Fixture.Create<T3>();
            var a4 = Fixture.Create<T4>();
            var func = Substitute.For<Func<T1, T2, T3, T4, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1, a2, a3, a4 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4 );
        }

        [Fact]
        public void ToAction_5_ShouldReturnCorrectResult()
        {
            var a1 = Fixture.Create<T1>();
            var a2 = Fixture.Create<T2>();
            var a3 = Fixture.Create<T3>();
            var a4 = Fixture.Create<T4>();
            var a5 = Fixture.Create<T5>();
            var func = Substitute.For<Func<T1, T2, T3, T4, T5, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1, a2, a3, a4, a5 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4, a5 );
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
            var func = Substitute.For<Func<T1, T2, T3, T4, T5, T6, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1, a2, a3, a4, a5, a6 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4, a5, a6 );
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
            var func = Substitute.For<Func<T1, T2, T3, T4, T5, T6, T7, Core.Functional.Nil>>();
            var sut = func.ToAction();

            sut( a1, a2, a3, a4, a5, a6, a7 );

            func.Verify().CallAt( 0 ).Exists().And.Arguments.Should().ContainInOrder( a1, a2, a3, a4, a5, a6, a7 );
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
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func1_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func2_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, T2, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func3_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, T2, T3, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func4_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, T2, T3, T4, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func5_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, T2, T3, T4, T5, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func6_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, T2, T3, T4, T5, T6, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Func7_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action0_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action1_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action2_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1, T2>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action3_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1, T2, T3>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action4_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1, T2, T3, T4>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action5_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1, T2, T3, T4, T5>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action6_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1, T2, T3, T4, T5, T6>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Of_Action7_ShouldReturnCorrectResult()
        {
            var sut = Substitute.For<Action<T1, T2, T3, T4, T5, T6, T7>>();
            var result = Core.Functional.Lambda.Of( sut );
            result.Should().BeSameAs( sut );
        }
    }
}
