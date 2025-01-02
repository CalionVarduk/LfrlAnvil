// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using LfrlAnvil.Functional;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests;

public class ObjectRecyclerTests : TestsBase
{
    [Fact]
    public void Rent_ShouldCreateFirstObjectCorrectly()
    {
        var sut = new RecyclerMock();
        var result = sut.Rent();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 1 );
            sut.ActiveObjectCount.Should().Be( 1 );
            result.Owner.Should().BeSameAs( sut );
            result.GetObject().Freed.Should().BeFalse();
            result.ToString()
                .Should()
                .Be( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Obj]" );
        }
    }

    [Fact]
    public void Rent_ShouldCreateSecondObjectCorrectly()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var first = sut.Rent();

        var result = sut.Rent();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 2 );
            sut.ActiveObjectCount.Should().Be( 2 );
            result.Owner.Should().BeSameAs( sut );
            result.GetObject().Freed.Should().BeFalse();
            result.GetObject().Should().NotBeSameAs( first.GetObject() );
            result.ToString()
                .Should()
                .Be( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Disposable]" );
        }
    }

    [Fact]
    public void Rent_ShouldReuseFreedObjectCorrectly()
    {
        var sut = new RecyclerMock();
        var first = sut.Rent();
        var second = sut.Rent();
        var obj = first.GetObject();
        first.Dispose();

        var result = sut.Rent();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 2 );
            sut.ActiveObjectCount.Should().Be( 2 );
            result.Owner.Should().BeSameAs( sut );
            result.GetObject().Freed.Should().BeTrue();
            result.GetObject().Should().BeSameAs( obj );
            result.GetObject().Should().NotBeSameAs( second.GetObject() );
            result.ToString()
                .Should()
                .Be( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Obj]" );
        }
    }

    [Fact]
    public void Rent_ShouldReuseFreedObjectCorrectly_WithMoreThanOneFreeObject()
    {
        var sut = new RecyclerMock();
        var first = sut.Rent();
        var second = sut.Rent();
        var third = sut.Rent();
        var obj1 = first.GetObject();
        var obj2 = second.GetObject();
        first.Dispose();
        second.Dispose();

        var result = sut.Rent();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 3 );
            sut.ActiveObjectCount.Should().Be( 2 );
            result.Owner.Should().BeSameAs( sut );
            result.GetObject().Freed.Should().BeTrue();
            result.GetObject().Should().BeSameAs( obj2 );
            result.GetObject().Should().NotBeSameAs( obj1 );
            result.GetObject().Should().NotBeSameAs( third.GetObject() );
            result.ToString()
                .Should()
                .Be( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Obj]" );
        }
    }

    [Fact]
    public void Rent_ShouldThrowObjectDisposedException_WhenRecyclerIsDisposed()
    {
        var sut = new RecyclerMock();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Rent() );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenNoObjectIsCached()
    {
        var sut = new RecyclerMock();
        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 0 );
            sut.ActiveObjectCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeAllTokens()
    {
        var sut = new RecyclerMock();
        var first = sut.Rent();
        sut.CreateDisposable = true;
        var second = sut.Rent();
        var third = sut.Rent();
        var fourth = sut.Rent();
        var obj1 = ( RecyclerMock.Disposable )second.GetObject();
        var obj2 = ( RecyclerMock.Disposable )third.GetObject();
        var obj3 = ( RecyclerMock.Disposable )fourth.GetObject();
        fourth.Dispose();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 0 );
            sut.ActiveObjectCount.Should().Be( 0 );
            first.ToString().Should().Be( "(disposed)" );
            second.ToString().Should().Be( "(disposed)" );
            third.ToString().Should().Be( "(disposed)" );
            fourth.ToString().Should().Be( "(disposed)" );
            obj1.IsDisposed.Should().BeTrue();
            obj2.IsDisposed.Should().BeTrue();
            obj3.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeAllTokens_EvenWhenSomeObjectDisposalThrows()
    {
        var exc1 = new Exception( "foo" );
        var exc2 = new Exception( "bar" );
        var sut = new RecyclerMock { CreateDisposable = true };
        var first = sut.Rent();
        var second = sut.Rent();
        var third = sut.Rent();
        var fourth = sut.Rent();
        var obj1 = ( RecyclerMock.Disposable )first.GetObject();
        var obj2 = ( RecyclerMock.Disposable )second.GetObject();
        var obj3 = ( RecyclerMock.Disposable )third.GetObject();
        var obj4 = ( RecyclerMock.Disposable )fourth.GetObject();
        obj2.DisposeException = exc1;
        obj3.DisposeException = exc2;
        third.Dispose();
        fourth.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<AggregateException>().And.InnerExceptions.Should().BeSequentiallyEqualTo( exc1, exc2 );
            sut.ObjectCount.Should().Be( 0 );
            sut.ActiveObjectCount.Should().Be( 0 );
            first.ToString().Should().Be( "(disposed)" );
            second.ToString().Should().Be( "(disposed)" );
            third.ToString().Should().Be( "(disposed)" );
            fourth.ToString().Should().Be( "(disposed)" );
            obj1.IsDisposed.Should().BeTrue();
            obj2.IsDisposed.Should().BeTrue();
            obj3.IsDisposed.Should().BeTrue();
            obj4.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenCalledSecondTime()
    {
        var sut = new RecyclerMock();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public void TrimExcess_ShouldDoNothing_WhenRecyclerIsEmpty()
    {
        var sut = new RecyclerMock();
        sut.TrimExcess();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 0 );
            sut.ActiveObjectCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void TrimExcess_ShouldDoNothing_WhenRecyclerContainsOnlyActiveObjects()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var a = sut.Rent();
        var b = sut.Rent();
        var c = sut.Rent();
        var objA = ( RecyclerMock.Disposable )a.GetObject();
        var objB = ( RecyclerMock.Disposable )b.GetObject();
        var objC = ( RecyclerMock.Disposable )c.GetObject();

        sut.TrimExcess();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 3 );
            sut.ActiveObjectCount.Should().Be( 3 );
            objA.Freed.Should().BeFalse();
            objA.IsDisposed.Should().BeFalse();
            objB.Freed.Should().BeFalse();
            objB.IsDisposed.Should().BeFalse();
            objC.Freed.Should().BeFalse();
            objC.IsDisposed.Should().BeFalse();
        }
    }

    [Fact]
    public void TrimExcess_ShouldDisposeAllObjects_WhenRecyclerContainsOnlyFreeObjects()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var a = sut.Rent();
        var b = sut.Rent();
        var c = sut.Rent();
        var objA = ( RecyclerMock.Disposable )a.GetObject();
        var objB = ( RecyclerMock.Disposable )b.GetObject();
        var objC = ( RecyclerMock.Disposable )c.GetObject();
        b.Dispose();
        c.Dispose();
        a.Dispose();

        sut.TrimExcess();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 0 );
            sut.ActiveObjectCount.Should().Be( 0 );
            objA.IsDisposed.Should().BeTrue();
            objB.IsDisposed.Should().BeTrue();
            objC.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void TrimExcess_ShouldDoNothing_WhenRecyclerContainsSomeFreeObjects_NoneAtTail()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var a = sut.Rent();
        var b = sut.Rent();
        var c = sut.Rent();
        var d = sut.Rent();
        var e = sut.Rent();
        var objA = ( RecyclerMock.Disposable )a.GetObject();
        var objB = ( RecyclerMock.Disposable )b.GetObject();
        var objC = ( RecyclerMock.Disposable )c.GetObject();
        var objD = ( RecyclerMock.Disposable )d.GetObject();
        var objE = ( RecyclerMock.Disposable )e.GetObject();
        d.Dispose();
        b.Dispose();
        a.Dispose();

        sut.TrimExcess();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 5 );
            sut.ActiveObjectCount.Should().Be( 2 );
            objA.Freed.Should().BeTrue();
            objA.IsDisposed.Should().BeFalse();
            objB.Freed.Should().BeTrue();
            objB.IsDisposed.Should().BeFalse();
            objC.Freed.Should().BeFalse();
            objC.IsDisposed.Should().BeFalse();
            objD.Freed.Should().BeTrue();
            objD.IsDisposed.Should().BeFalse();
            objE.Freed.Should().BeFalse();
            objE.IsDisposed.Should().BeFalse();
        }
    }

    [Fact]
    public void TrimExcess_ShouldDisposeFreeObjectsAtTail_WhenRecyclerContainsSomeFreeObjects_SomeAtTail()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var a = sut.Rent();
        var b = sut.Rent();
        var c = sut.Rent();
        var d = sut.Rent();
        var e = sut.Rent();
        var f = sut.Rent();
        var g = sut.Rent();
        var objA = ( RecyclerMock.Disposable )a.GetObject();
        var objB = ( RecyclerMock.Disposable )b.GetObject();
        var objC = ( RecyclerMock.Disposable )c.GetObject();
        var objD = ( RecyclerMock.Disposable )d.GetObject();
        var objE = ( RecyclerMock.Disposable )e.GetObject();
        var objF = ( RecyclerMock.Disposable )f.GetObject();
        var objG = ( RecyclerMock.Disposable )g.GetObject();
        f.Dispose();
        g.Dispose();
        b.Dispose();
        a.Dispose();
        d.Dispose();

        sut.TrimExcess();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 5 );
            sut.ActiveObjectCount.Should().Be( 2 );
            objA.Freed.Should().BeTrue();
            objA.IsDisposed.Should().BeFalse();
            objB.Freed.Should().BeTrue();
            objB.IsDisposed.Should().BeFalse();
            objC.Freed.Should().BeFalse();
            objC.IsDisposed.Should().BeFalse();
            objD.Freed.Should().BeTrue();
            objD.IsDisposed.Should().BeFalse();
            objE.Freed.Should().BeFalse();
            objE.IsDisposed.Should().BeFalse();
            objF.Freed.Should().BeTrue();
            objF.IsDisposed.Should().BeTrue();
            objG.Freed.Should().BeTrue();
            objG.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void TrimExcess_ShouldDisposeAllFreeObjects_WhenRecyclerContainsSomeFreeObjects_AllAtTail()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var a = sut.Rent();
        var b = sut.Rent();
        var c = sut.Rent();
        var d = sut.Rent();
        var e = sut.Rent();
        var objA = ( RecyclerMock.Disposable )a.GetObject();
        var objB = ( RecyclerMock.Disposable )b.GetObject();
        var objC = ( RecyclerMock.Disposable )c.GetObject();
        var objD = ( RecyclerMock.Disposable )d.GetObject();
        var objE = ( RecyclerMock.Disposable )e.GetObject();
        d.Dispose();
        c.Dispose();
        e.Dispose();

        sut.TrimExcess();

        using ( new AssertionScope() )
        {
            sut.ObjectCount.Should().Be( 2 );
            sut.ActiveObjectCount.Should().Be( 2 );
            objA.Freed.Should().BeFalse();
            objA.IsDisposed.Should().BeFalse();
            objB.Freed.Should().BeFalse();
            objB.IsDisposed.Should().BeFalse();
            objC.Freed.Should().BeTrue();
            objC.IsDisposed.Should().BeTrue();
            objD.Freed.Should().BeTrue();
            objD.IsDisposed.Should().BeTrue();
            objE.Freed.Should().BeTrue();
            objE.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void TrimExcess_ShouldDisposeFreeObjects_EvenWhenSomeObjectDisposalThrows()
    {
        var exc1 = new Exception( "foo" );
        var exc2 = new Exception( "bar" );
        var sut = new RecyclerMock { CreateDisposable = true };
        var a = sut.Rent();
        var b = sut.Rent();
        var c = sut.Rent();
        var d = sut.Rent();
        var objA = ( RecyclerMock.Disposable )a.GetObject();
        var objB = ( RecyclerMock.Disposable )b.GetObject();
        var objC = ( RecyclerMock.Disposable )c.GetObject();
        var objD = ( RecyclerMock.Disposable )d.GetObject();
        objC.DisposeException = exc1;
        objD.DisposeException = exc2;
        c.Dispose();
        d.Dispose();

        var action = Lambda.Of( () => sut.TrimExcess() );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<AggregateException>().And.InnerExceptions.Should().BeSequentiallyEqualTo( exc1, exc2 );
            sut.ObjectCount.Should().Be( 2 );
            sut.ActiveObjectCount.Should().Be( 2 );
            objA.Freed.Should().BeFalse();
            objA.IsDisposed.Should().BeFalse();
            objB.Freed.Should().BeFalse();
            objB.IsDisposed.Should().BeFalse();
            objC.Freed.Should().BeTrue();
            objC.IsDisposed.Should().BeTrue();
            objD.Freed.Should().BeTrue();
            objD.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void TokenDispose_ShouldFreeUnderlyingObject()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        var obj = sut.GetObject();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            recycler.ObjectCount.Should().Be( 1 );
            recycler.ActiveObjectCount.Should().Be( 0 );
            obj.Freed.Should().BeTrue();
            sut.ToString().Should().Be( "(disposed)" );
        }
    }

    [Fact]
    public void TokenDispose_ShouldFreeUnderlyingObject_WithMultipleActiveTokens()
    {
        var recycler = new RecyclerMock();
        _ = recycler.Rent();
        var sut = recycler.Rent();
        var obj = sut.GetObject();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            recycler.ObjectCount.Should().Be( 2 );
            recycler.ActiveObjectCount.Should().Be( 1 );
            obj.Freed.Should().BeTrue();
            sut.ToString().Should().Be( "(disposed)" );
        }
    }

    [Fact]
    public void TokenDispose_ShouldDoNothing_WhenCalledSecondTime()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public void TokenDispose_ShouldDoNothing_WhenTokenIsNotValid()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        sut.Dispose();
        recycler.TrimExcess();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public void TokenGetObject_ShouldThrowObjectDisposedException_ForDefault()
    {
        var sut = default( ObjectRecyclerToken<object> );
        var action = Lambda.Of( () => sut.GetObject() );
        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void TokenGetObject_ShouldThrowObjectDisposedException_ForFreedToken()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        sut.Dispose();

        var action = Lambda.Of( () => sut.GetObject() );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    public sealed class RecyclerMock : ObjectRecycler<RecyclerMock.Obj>
    {
        public bool CreateDisposable { get; set; }

        public class Obj
        {
            public bool Freed { get; set; }
        }

        public sealed class Disposable : Obj, IDisposable
        {
            public bool IsDisposed { get; private set; }
            public Exception? DisposeException { get; set; }

            public void Dispose()
            {
                Ensure.False( IsDisposed );
                IsDisposed = true;

                if ( DisposeException is not null )
                    throw DisposeException;
            }
        }

        protected override Obj Create()
        {
            return CreateDisposable ? new Disposable() : new Obj();
        }

        protected override void Free(Obj obj)
        {
            base.Free( obj );
            obj.Freed = true;
        }
    }
}
