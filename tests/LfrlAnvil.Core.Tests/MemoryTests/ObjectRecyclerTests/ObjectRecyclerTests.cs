using LfrlAnvil.Functional;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests;

public class ObjectRecyclerTests : TestsBase
{
    [Fact]
    public void Rent_ShouldCreateFirstObjectCorrectly()
    {
        var sut = new RecyclerMock();
        var result = sut.Rent();

        Assertion.All(
                sut.ObjectCount.TestEquals( 1 ),
                sut.ActiveObjectCount.TestEquals( 1 ),
                result.Owner.TestRefEquals( sut ),
                result.GetObject().Freed.TestFalse(),
                result.ToString()
                    .TestEquals( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Obj]" ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldCreateSecondObjectCorrectly()
    {
        var sut = new RecyclerMock { CreateDisposable = true };
        var first = sut.Rent();

        var result = sut.Rent();

        Assertion.All(
                sut.ObjectCount.TestEquals( 2 ),
                sut.ActiveObjectCount.TestEquals( 2 ),
                result.Owner.TestRefEquals( sut ),
                result.GetObject().Freed.TestFalse(),
                result.GetObject().TestNotRefEquals( first.GetObject() ),
                result.ToString()
                    .TestEquals(
                        "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Disposable]" ) )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 2 ),
                sut.ActiveObjectCount.TestEquals( 2 ),
                result.Owner.TestRefEquals( sut ),
                result.GetObject().Freed.TestTrue(),
                result.GetObject().TestRefEquals( obj ),
                result.GetObject().TestNotRefEquals( second.GetObject() ),
                result.ToString()
                    .TestEquals( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Obj]" ) )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 3 ),
                sut.ActiveObjectCount.TestEquals( 2 ),
                result.Owner.TestRefEquals( sut ),
                result.GetObject().Freed.TestTrue(),
                result.GetObject().TestRefEquals( obj2 ),
                result.GetObject().TestNotRefEquals( obj1 ),
                result.GetObject().TestNotRefEquals( third.GetObject() ),
                result.ToString()
                    .TestEquals( "(active) [LfrlAnvil.Tests.MemoryTests.ObjectRecyclerTests.ObjectRecyclerTests+RecyclerMock+Obj]" ) )
            .Go();
    }

    [Fact]
    public void Rent_ShouldThrowObjectDisposedException_WhenRecyclerIsDisposed()
    {
        var sut = new RecyclerMock();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Rent() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenNoObjectIsCached()
    {
        var sut = new RecyclerMock();
        sut.Dispose();

        Assertion.All(
                sut.ObjectCount.TestEquals( 0 ),
                sut.ActiveObjectCount.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 0 ),
                sut.ActiveObjectCount.TestEquals( 0 ),
                first.ToString().TestEquals( "(disposed)" ),
                second.ToString().TestEquals( "(disposed)" ),
                third.ToString().TestEquals( "(disposed)" ),
                fourth.ToString().TestEquals( "(disposed)" ),
                obj1.IsDisposed.TestTrue(),
                obj2.IsDisposed.TestTrue(),
                obj3.IsDisposed.TestTrue() )
            .Go();
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

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<AggregateException>(),
                    sut.ObjectCount.TestEquals( 0 ),
                    sut.ActiveObjectCount.TestEquals( 0 ),
                    first.ToString().TestEquals( "(disposed)" ),
                    second.ToString().TestEquals( "(disposed)" ),
                    third.ToString().TestEquals( "(disposed)" ),
                    fourth.ToString().TestEquals( "(disposed)" ),
                    obj1.IsDisposed.TestTrue(),
                    obj2.IsDisposed.TestTrue(),
                    obj3.IsDisposed.TestTrue(),
                    obj4.IsDisposed.TestTrue() ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenCalledSecondTime()
    {
        var sut = new RecyclerMock();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void TrimExcess_ShouldDoNothing_WhenRecyclerIsEmpty()
    {
        var sut = new RecyclerMock();
        sut.TrimExcess();

        Assertion.All(
                sut.ObjectCount.TestEquals( 0 ),
                sut.ActiveObjectCount.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 3 ),
                sut.ActiveObjectCount.TestEquals( 3 ),
                objA.Freed.TestFalse(),
                objA.IsDisposed.TestFalse(),
                objB.Freed.TestFalse(),
                objB.IsDisposed.TestFalse(),
                objC.Freed.TestFalse(),
                objC.IsDisposed.TestFalse() )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 0 ),
                sut.ActiveObjectCount.TestEquals( 0 ),
                objA.IsDisposed.TestTrue(),
                objB.IsDisposed.TestTrue(),
                objC.IsDisposed.TestTrue() )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 5 ),
                sut.ActiveObjectCount.TestEquals( 2 ),
                objA.Freed.TestTrue(),
                objA.IsDisposed.TestFalse(),
                objB.Freed.TestTrue(),
                objB.IsDisposed.TestFalse(),
                objC.Freed.TestFalse(),
                objC.IsDisposed.TestFalse(),
                objD.Freed.TestTrue(),
                objD.IsDisposed.TestFalse(),
                objE.Freed.TestFalse(),
                objE.IsDisposed.TestFalse() )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 5 ),
                sut.ActiveObjectCount.TestEquals( 2 ),
                objA.Freed.TestTrue(),
                objA.IsDisposed.TestFalse(),
                objB.Freed.TestTrue(),
                objB.IsDisposed.TestFalse(),
                objC.Freed.TestFalse(),
                objC.IsDisposed.TestFalse(),
                objD.Freed.TestTrue(),
                objD.IsDisposed.TestFalse(),
                objE.Freed.TestFalse(),
                objE.IsDisposed.TestFalse(),
                objF.Freed.TestTrue(),
                objF.IsDisposed.TestTrue(),
                objG.Freed.TestTrue(),
                objG.IsDisposed.TestTrue() )
            .Go();
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

        Assertion.All(
                sut.ObjectCount.TestEquals( 2 ),
                sut.ActiveObjectCount.TestEquals( 2 ),
                objA.Freed.TestFalse(),
                objA.IsDisposed.TestFalse(),
                objB.Freed.TestFalse(),
                objB.IsDisposed.TestFalse(),
                objC.Freed.TestTrue(),
                objC.IsDisposed.TestTrue(),
                objD.Freed.TestTrue(),
                objD.IsDisposed.TestTrue(),
                objE.Freed.TestTrue(),
                objE.IsDisposed.TestTrue() )
            .Go();
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

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<AggregateException>(),
                    sut.ObjectCount.TestEquals( 2 ),
                    sut.ActiveObjectCount.TestEquals( 2 ),
                    objA.Freed.TestFalse(),
                    objA.IsDisposed.TestFalse(),
                    objB.Freed.TestFalse(),
                    objB.IsDisposed.TestFalse(),
                    objC.Freed.TestTrue(),
                    objC.IsDisposed.TestTrue(),
                    objD.Freed.TestTrue(),
                    objD.IsDisposed.TestTrue() ) )
            .Go();
    }

    [Fact]
    public void TokenDispose_ShouldFreeUnderlyingObject()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        var obj = sut.GetObject();

        sut.Dispose();

        Assertion.All(
                recycler.ObjectCount.TestEquals( 1 ),
                recycler.ActiveObjectCount.TestEquals( 0 ),
                obj.Freed.TestTrue(),
                sut.ToString().TestEquals( "(disposed)" ) )
            .Go();
    }

    [Fact]
    public void TokenDispose_ShouldFreeUnderlyingObject_WithMultipleActiveTokens()
    {
        var recycler = new RecyclerMock();
        _ = recycler.Rent();
        var sut = recycler.Rent();
        var obj = sut.GetObject();

        sut.Dispose();

        Assertion.All(
                recycler.ObjectCount.TestEquals( 2 ),
                recycler.ActiveObjectCount.TestEquals( 1 ),
                obj.Freed.TestTrue(),
                sut.ToString().TestEquals( "(disposed)" ) )
            .Go();
    }

    [Fact]
    public void TokenDispose_ShouldDoNothing_WhenCalledSecondTime()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void TokenDispose_ShouldDoNothing_WhenTokenIsNotValid()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        sut.Dispose();
        recycler.TrimExcess();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void TokenGetObject_ShouldThrowObjectDisposedException_ForDefault()
    {
        var sut = default( ObjectRecyclerToken<object> );
        var action = Lambda.Of( () => sut.GetObject() );
        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void TokenGetObject_ShouldThrowObjectDisposedException_ForFreedToken()
    {
        var recycler = new RecyclerMock();
        var sut = recycler.Rent();
        sut.Dispose();

        var action = Lambda.Of( () => sut.GetObject() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
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
