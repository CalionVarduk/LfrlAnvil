using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace LfrlAnvil.TestExtensions;

public sealed class Fixture
{
    private readonly Dictionary<Type, Delegate> _factories;

    public Fixture(bool allowNegativeSignedIntegers = false)
    {
        _factories = new Dictionary<Type, Delegate>
        {
            { typeof( object ), (Fixture _) => new object() },
            { typeof( bool ), (Fixture _) => Random.Shared.Next( 0, 2 ) != 0 },
            { typeof( byte ), (Fixture _) => ( byte )Random.Shared.Next( 0, byte.MaxValue + 1 ) },
            { typeof( ushort ), (Fixture _) => ( ushort )Random.Shared.Next( 0, ushort.MaxValue + 1 ) },
            { typeof( uint ), (Fixture _) => unchecked( ( uint )Random.Shared.Next( int.MinValue, int.MaxValue ) ) },
            { typeof( ulong ), (Fixture _) => unchecked( ( ulong )Random.Shared.NextInt64( long.MinValue, long.MaxValue ) ) },
            { typeof( Half ), (Fixture _) => ( Half )Random.Shared.NextSingle() },
            { typeof( float ), (Fixture _) => Random.Shared.NextSingle() },
            { typeof( double ), (Fixture _) => Random.Shared.NextDouble() },
            { typeof( decimal ), (Fixture _) => Random.Shared.Next() + ( decimal )Random.Shared.NextDouble() },
            { typeof( char ), (Fixture _) => ( char )Random.Shared.Next( 32, 127 ) },
            { typeof( string ), (Fixture _) => Guid.NewGuid().ToString() },
            { typeof( Guid ), (Fixture _) => Guid.NewGuid() },
            {
                typeof( TimeSpan ),
                (Fixture _) => TimeSpan.FromTicks( Random.Shared.NextInt64( TimeSpan.TicksPerDay * -30, TimeSpan.TicksPerDay * 30 ) )
            },
            {
                typeof( DateTime ), (Fixture _) =>
                {
                    var offset = Random.Shared.NextInt64( TimeSpan.TicksPerDay * -30, TimeSpan.TicksPerDay * 30 );
                    return DateTime.SpecifyKind( DateTime.UtcNow, DateTimeKind.Unspecified ).Add( TimeSpan.FromTicks( offset ) );
                }
            },
            {
                typeof( DateOnly ), (Fixture _) =>
                {
                    var offset = Random.Shared.Next( -365, 365 );
                    return DateOnly.FromDateTime( DateTime.UtcNow ).AddDays( offset );
                }
            },
            { typeof( TimeOnly ), (Fixture _) => new TimeOnly( Random.Shared.NextInt64( TimeSpan.TicksPerDay ) ) }
        };

        if ( allowNegativeSignedIntegers )
        {
            _factories.Add( typeof( sbyte ), (Fixture _) => ( sbyte )Random.Shared.Next( sbyte.MinValue, sbyte.MaxValue + 1 ) );
            _factories.Add( typeof( short ), (Fixture _) => ( short )Random.Shared.Next( short.MinValue, short.MaxValue + 1 ) );
            _factories.Add( typeof( int ), (Fixture _) => Random.Shared.Next( int.MinValue, int.MaxValue ) );
            _factories.Add( typeof( long ), (Fixture _) => Random.Shared.NextInt64( long.MinValue, long.MaxValue ) );
            _factories.Add( typeof( BigInteger ), (Fixture _) => ( BigInteger )Random.Shared.NextInt64( long.MinValue, long.MaxValue ) );
        }
        else
        {
            _factories.Add( typeof( sbyte ), (Fixture _) => ( sbyte )Random.Shared.Next( 0, sbyte.MaxValue + 1 ) );
            _factories.Add( typeof( short ), (Fixture _) => ( short )Random.Shared.Next( 0, short.MaxValue + 1 ) );
            _factories.Add( typeof( int ), (Fixture _) => Random.Shared.Next() );
            _factories.Add( typeof( long ), (Fixture _) => Random.Shared.NextInt64() );
            _factories.Add( typeof( BigInteger ), (Fixture _) => ( BigInteger )Random.Shared.NextInt64() );
        }
    }

    public Fixture Customize<T>(Func<Fixture, Func<Fixture, T>?, Func<Fixture, T>> customization)
    {
        var factory = customization( this, ( Func<Fixture, T>? )_factories.GetValueOrDefault( typeof( T ) ) );
        _factories[typeof( T )] = factory;
        return this;
    }

    [Pure]
    public T Create<T>()
    {
        var @delegate = GetDelegate<T>();
        return @delegate( this );
    }

    [Pure]
    public T Create<T>(Func<T, bool> predicate)
    {
        return GetGenerator<T>().First( predicate );
    }

    [Pure]
    public IEnumerable<T> CreateMany<T>(int count = 3)
    {
        var @delegate = GetDelegate<T>();
        return Enumerable.Range( 0, count ).Select( _ => @delegate( this ) ).ToList();
    }

    [Pure]
    public IEnumerable<T> CreateMany<T>(Func<T, bool> predicate, int count = 3)
    {
        var @delegate = GetDelegate<T>();
        return Enumerable.Range( 0, count ).Select( _ => CreateGenerator( @delegate ).First( predicate ) ).ToList();
    }

    [Pure]
    public T[] CreateManyDistinct<T>(int count = 3)
    {
        var @delegate = GetDelegate<T>();
        var result = new HashSet<T>();
        while ( result.Count < count )
            result.Add( @delegate( this ) );

        return result.ToArray();
    }

    [Pure]
    public T[] CreateManySorted<T>(int count = 3)
    {
        return CreateMany<T>( count ).Order().ToArray();
    }

    [Pure]
    public T[] CreateManyDistinctSorted<T>(int count = 3)
    {
        var @delegate = GetDelegate<T>();
        var result = new HashSet<T>();
        while ( result.Count < count )
            result.Add( @delegate( this ) );

        return result.Order().ToArray();
    }

    [Pure]
    public IEnumerable<T> GetGenerator<T>()
    {
        var @delegate = GetDelegate<T>();
        return CreateGenerator( @delegate );
    }

    [Pure]
    public T? CreateDefault<T>()
    {
        return default;
    }

    [Pure]
    public T CreateNotDefault<T>()
    {
        return Create<T>( static x => ! EqualityComparer<T>.Default.Equals( x, default ) );
    }

    private IEnumerable<T> CreateGenerator<T>(Func<Fixture, T> @delegate)
    {
        while ( true )
            yield return @delegate( this );
    }

    [Pure]
    private Func<Fixture, T> GetDelegate<T>()
    {
        if ( _factories.TryGetValue( typeof( T ), out var result ) )
            return ( Func<Fixture, T> )result;

        if ( typeof( T ).IsEnum )
            return CustomizeEnum<T>();

        var underlyingType = Nullable.GetUnderlyingType( typeof( T ) );
        if ( underlyingType is null )
            return ( Func<Fixture, T> )_factories[typeof( T )];

        if ( ! _factories.TryGetValue( underlyingType, out result ) )
        {
            if ( ! underlyingType.IsEnum )
                return ( Func<Fixture, T> )_factories[typeof( T )];

            var genericEnumMethod = typeof( Fixture ).GetMethod(
                nameof( CustomizeEnum ),
                BindingFlags.Instance | BindingFlags.NonPublic );

            Debug.Assert( genericEnumMethod is not null );
            var enumMethod = genericEnumMethod.MakeGenericMethod( underlyingType );
            result = ( Delegate )enumMethod.Invoke( this, null )!;
        }

        var genericNullableMethod = typeof( Fixture ).GetMethod(
            nameof( CustomizeNullable ),
            BindingFlags.Instance | BindingFlags.NonPublic );

        Debug.Assert( genericNullableMethod is not null );
        var nullableMethod = genericNullableMethod.MakeGenericMethod( underlyingType );
        return ( Func<Fixture, T> )nullableMethod.Invoke( this, [ result ] )!;
    }

    private Func<Fixture, T> CustomizeEnum<T>()
    {
        Debug.Assert( typeof( T ).IsEnum );
        var values = Enum.GetValues( typeof( T ) ).Cast<T>().ToArray();

        Func<Fixture, T> result = values.Length == 0
            ? _ => default!
            : _ => values[Random.Shared.Next( 0, values.Length )];

        _factories[typeof( T )] = result;
        return result;
    }

    private Func<Fixture, T?> CustomizeNullable<T>(Func<Fixture, T> @delegate)
        where T : struct
    {
        var result = (Fixture f) => ( T? )@delegate( f );
        _factories.Add( typeof( T? ), result );
        return result;
    }
}
