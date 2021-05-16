using LfrlSoft.NET.Common.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Common
{
    public static class Assert
    {
        public const string DefaultParamName = "param";

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNull<T>(T param, string paramName = DefaultParamName)
            where T : class
        {
            if ( !(param is null) )
                throw Exceptions.NotNull( param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNull<T>(T? param, string paramName = DefaultParamName)
            where T : struct
        {
            if ( param.HasValue )
                throw Exceptions.NotNull( param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNull<T>(T param, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( !Generic<T>.IsReferenceType && !Generic<T>.IsNullableType )
                throw Exceptions.NotNull( param, paramName );

            if ( !comparer.Equals( param, default ) )
                throw Exceptions.NotNull( param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNull<T>(T param, string paramName = DefaultParamName)
            where T : class
        {
            if ( param is null )
                throw Exceptions.Null( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNull<T>(T? param, string paramName = DefaultParamName)
            where T : struct
        {
            if ( !param.HasValue )
                throw Exceptions.Null( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNull<T>(T param, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( !Generic<T>.IsReferenceType && !Generic<T>.IsNullableType )
                return;

            if ( comparer.Equals( param, default ) )
                throw Exceptions.Null( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsDefault<T>(T param, string paramName = DefaultParamName)
        {
            if ( Generic<T>.IsNotDefault( param ) )
                throw Exceptions.NotDefault( param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotDefault<T>(T param, string paramName = DefaultParamName)
        {
            if ( Generic<T>.IsDefault( param ) )
                throw Exceptions.Default( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsOfType<T>(object param, string paramName = DefaultParamName)
        {
            IsOfType( typeof( T ), param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsOfType<T>(Type type, T param, string paramName = DefaultParamName)
        {
            if ( type != param.GetType() )
                throw Exceptions.NotOfType( type, param.GetType(), paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotOfType<T>(object param, string paramName = DefaultParamName)
        {
            IsNotOfType( typeof( T ), param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotOfType<T>(Type type, T param, string paramName = DefaultParamName)
        {
            if ( type == param.GetType() )
                throw Exceptions.OfType( type, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsAssignableToType<T>(object param, string paramName = DefaultParamName)
        {
            IsAssignableToType( param, typeof( T ), paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsAssignableToType<T>(T param, Type type, string paramName = DefaultParamName)
        {
            if ( !type.IsAssignableFrom( param.GetType() ) )
                throw Exceptions.NotAssignableToType( type, param.GetType(), paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotAssignableToType<T>(object param, string paramName = DefaultParamName)
        {
            IsNotAssignableToType( param, typeof( T ), paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotAssignableToType<T>(T param, Type type, string paramName = DefaultParamName)
        {
            if ( type.IsAssignableFrom( param.GetType() ) )
                throw Exceptions.AssignableToType( type, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Equals<T>(T param, T value, string paramName = DefaultParamName)
            where T : IEquatable<T>
        {
            if ( !param.Equals( value ) )
                throw Exceptions.NotEqualTo( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Equals<T>(T param, T value, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( !comparer.Equals( param, value ) )
                throw Exceptions.NotEqualTo( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotEquals<T>(T param, T value, string paramName = DefaultParamName)
            where T : IEquatable<T>
        {
            if ( param.Equals( value ) )
                throw Exceptions.EqualTo( value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotEquals<T>(T param, T value, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Equals( param, value ) )
                throw Exceptions.EqualTo( value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsGreaterThan<T>(T param, T value, string paramName = DefaultParamName)
            where T : IComparable<T>
        {
            if ( param.CompareTo( value ) <= 0 )
                throw Exceptions.NotGreaterThan( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsGreaterThan<T>(T param, T value, IComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Compare( param, value ) <= 0 )
                throw Exceptions.NotGreaterThan( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsGreaterThanOrEqualTo<T>(T param, T value, string paramName = DefaultParamName)
            where T : IComparable<T>
        {
            if ( param.CompareTo( value ) < 0 )
                throw Exceptions.NotGreaterThanOrEqual( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsGreaterThanOrEqualTo<T>(T param, T value, IComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Compare( param, value ) < 0 )
                throw Exceptions.NotGreaterThanOrEqual( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsLessThan<T>(T param, T value, string paramName = DefaultParamName)
            where T : IComparable<T>
        {
            if ( param.CompareTo( value ) >= 0 )
                throw Exceptions.NotLessThan( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsLessThan<T>(T param, T value, IComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Compare( param, value ) >= 0 )
                throw Exceptions.NotLessThan( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsLessThanOrEqualTo<T>(T param, T value, string paramName = DefaultParamName)
            where T : IComparable<T>
        {
            if ( param.CompareTo( value ) > 0 )
                throw Exceptions.NotLessThanOrEqual( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsLessThanOrEqualTo<T>(T param, T value, IComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Compare( param, value ) > 0 )
                throw Exceptions.NotLessThanOrEqual( param, value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsBetween<T>(T param, T min, T max, string paramName = DefaultParamName)
            where T : IComparable<T>
        {
            if ( param.CompareTo( min ) < 0 || param.CompareTo( max ) > 0 )
                throw Exceptions.NotBetween( param, min, max, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsBetween<T>(T param, T min, T max, IComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Compare( param, min ) < 0 || comparer.Compare( param, max ) > 0 )
                throw Exceptions.NotBetween( param, min, max, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotBetween<T>(T param, T min, T max, string paramName = DefaultParamName)
            where T : IComparable<T>
        {
            if ( param.CompareTo( min ) >= 0 && param.CompareTo( max ) <= 0 )
                throw Exceptions.Between( param, min, max, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotBetween<T>(T param, T min, T max, IComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( comparer.Compare( param, min ) >= 0 && comparer.Compare( param, max ) <= 0 )
                throw Exceptions.Between( param, min, max, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsEmpty<T>(IEnumerable<T> param, string paramName = DefaultParamName)
        {
            if ( param.Any() )
                throw Exceptions.NotEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsEmpty<T>(IReadOnlyCollection<T> param, string paramName = DefaultParamName)
        {
            if ( param.Count > 0 )
                throw Exceptions.NotEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsEmpty(string param, string paramName = DefaultParamName)
        {
            if ( param.Length > 0 )
                throw Exceptions.NotEmpty( param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotEmpty<T>(IEnumerable<T> param, string paramName = DefaultParamName)
        {
            if ( !param.Any() )
                throw Exceptions.Empty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotEmpty<T>(IReadOnlyCollection<T> param, string paramName = DefaultParamName)
        {
            if ( param.Count == 0 )
                throw Exceptions.Empty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotEmpty(string param, string paramName = DefaultParamName)
        {
            if ( param.Length == 0 )
                throw Exceptions.Empty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNullOrEmpty<T>(IEnumerable<T> param, string paramName = DefaultParamName)
        {
            if ( param?.Any() == true )
                throw Exceptions.NotNullOrEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNullOrEmpty<T>(IReadOnlyCollection<T> param, string paramName = DefaultParamName)
        {
            if ( param?.Count > 0 )
                throw Exceptions.NotNullOrEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNullOrEmpty(string param, string paramName = DefaultParamName)
        {
            if ( !string.IsNullOrEmpty( param ) )
                throw Exceptions.NotNullOrEmpty( param, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNullOrEmpty<T>(IEnumerable<T> param, string paramName = DefaultParamName)
        {
            if ( param?.Any() != true )
                throw Exceptions.NullOrEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNullOrEmpty<T>(IReadOnlyCollection<T> param, string paramName = DefaultParamName)
        {
            if ( (param?.Count ?? 0) == 0 )
                throw Exceptions.NullOrEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNullOrEmpty(string param, string paramName = DefaultParamName)
        {
            if ( (param?.Length ?? 0) == 0 )
                throw Exceptions.NullOrEmpty( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IsNotNullOrWhiteSpace(string param, string paramName = DefaultParamName)
        {
            if ( string.IsNullOrWhiteSpace( param ) )
                throw Exceptions.NullOrWhiteSpace( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ContainsNull<T>(IEnumerable<T> param, string paramName = DefaultParamName)
            where T : class
        {
            if ( param.All( e => !(e is null) ) )
                throw Exceptions.NotContainsNull( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ContainsNull<T>(IEnumerable<T?> param, string paramName = DefaultParamName)
            where T : struct
        {
            if ( param.All( e => e.HasValue ) )
                throw Exceptions.NotContainsNull( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ContainsNull<T>(IEnumerable<T> param, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( !Generic<T>.IsReferenceType && !Generic<T>.IsNullableType )
                throw Exceptions.NotContainsNull( paramName );

            if ( param.All( e => !comparer.Equals( e, default ) ) )
                throw Exceptions.NotContainsNull( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotContainsNull<T>(IEnumerable<T> param, string paramName = DefaultParamName)
            where T : class
        {
            if ( param.Any( e => e is null ) )
                throw Exceptions.ContainsNull( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotContainsNull<T>(IEnumerable<T?> param, string paramName = DefaultParamName)
            where T : struct
        {
            if ( param.Any( e => !e.HasValue ) )
                throw Exceptions.ContainsNull( paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotContainsNull<T>(IEnumerable<T> param, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( !Generic<T>.IsReferenceType && !Generic<T>.IsNullableType )
                return;

            if ( param.Any( e => comparer.Equals( e, default ) ) )
                throw Exceptions.ContainsNull( paramName );
        }

        // TODO: other collection assertions
        // HasExactly, HasAtLeast, HasAtMost, HasBetween

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Contains<T>(IEnumerable<T> param, T value, string paramName = DefaultParamName)
            where T : IEquatable<T>
        {
            if ( !param.Any( e => e.Equals( value ) ) )
                throw Exceptions.NotContains( value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Contains<T>(IEnumerable<T> param, T value, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( !param.Any( e => comparer.Equals( e, value ) ) )
                throw Exceptions.NotContains( value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotContains<T>(IEnumerable<T> param, T value, string paramName = DefaultParamName)
            where T : IEquatable<T>
        {
            if ( param.Any( e => e.Equals( value ) ) )
                throw Exceptions.Contains( value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotContains<T>(IEnumerable<T> param, T value, IEqualityComparer<T> comparer, string paramName = DefaultParamName)
        {
            if ( param.Any( e => comparer.Equals( e, value ) ) )
                throw Exceptions.Contains( value, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ForAny<T>(IEnumerable<T> param, Func<T, bool> predicate, string description = null, string paramName = DefaultParamName)
        {
            if ( !param.Any( predicate ) )
                throw Exceptions.NotAny( description, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ForAny<T>(IEnumerable<T> param, Func<T, bool> predicate, Func<string> descriptionProvider, string paramName = DefaultParamName)
        {
            if ( !param.Any( predicate ) )
                throw Exceptions.NotAny( descriptionProvider?.Invoke(), paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ForAll<T>(IEnumerable<T> param, Func<T, bool> predicate, string description = null, string paramName = DefaultParamName)
        {
            if ( !param.All( predicate ) )
                throw Exceptions.NotAll( description, paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ForAll<T>(IEnumerable<T> param, Func<T, bool> predicate, Func<string> descriptionProvider, string paramName = DefaultParamName)
        {
            if ( !param.All( predicate ) )
                throw Exceptions.NotAll( descriptionProvider?.Invoke(), paramName );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void True(bool condition, string description = null)
        {
            if ( !condition )
                throw Exceptions.False( description );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void True(bool condition, Func<string> descriptionProvider)
        {
            if ( !condition )
                throw Exceptions.False( descriptionProvider?.Invoke() );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void False(bool condition, string description = null)
        {
            if ( condition )
                throw Exceptions.True( description );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void False(bool condition, Func<string> descriptionProvider)
        {
            if ( condition )
                throw Exceptions.True( descriptionProvider?.Invoke() );
        }

        private static class Exceptions
        {
            public static ArgumentException NotNull<T>(T param, string paramName) =>
                new ArgumentException( $"expected {paramName} to be null but found {param}", paramName );

            public static ArgumentNullException Null(string paramName) =>
                new ArgumentNullException( paramName );

            public static ArgumentException NotDefault<T>(T param, string paramName) =>
                new ArgumentException( $"expected {paramName} to be default but found {param}", paramName );

            public static ArgumentException Default(string paramName) =>
                new ArgumentException( $"expected {paramName} to not be default", paramName );

            public static ArgumentException NotOfType(Type type, Type actualType, string paramName) =>
                new ArgumentException( $"expected {paramName} to be of type {type.FullName} but found {actualType.FullName}", paramName );

            public static ArgumentException OfType(Type type, string paramName) =>
                new ArgumentException( $"expected {paramName} to not be of type {type.FullName}", paramName );

            public static ArgumentException NotAssignableToType(Type type, Type actualType, string paramName) =>
                new ArgumentException( $"expected {paramName} to be assignable to type {type.FullName} but found {actualType.FullName}", paramName );

            public static ArgumentException AssignableToType(Type type, string paramName) =>
                new ArgumentException( $"expected {paramName} to not be assignable to type {type.FullName}", paramName );

            public static ArgumentException NotEqualTo<T>(T param, T expectedValue, string paramName) =>
                new ArgumentException( $"expected {paramName} to be equal to {expectedValue} but found {param}", paramName );

            public static ArgumentException EqualTo<T>(T expectedValue, string paramName) =>
                new ArgumentException( $"expected {paramName} to not be equal to {expectedValue}", paramName );

            public static ArgumentException NotGreaterThan<T>(T param, T expectedValue, string paramName) =>
                new ArgumentException( $"expected {paramName} to be greater than {expectedValue} but found {param}", paramName );

            public static ArgumentException NotGreaterThanOrEqual<T>(T param, T expectedValue, string paramName) =>
                new ArgumentException( $"expected {paramName} to be greater than or equal to {expectedValue} but found {param}", paramName );

            public static ArgumentException NotLessThan<T>(T param, T expectedValue, string paramName) =>
                new ArgumentException( $"expected {paramName} to be less than {expectedValue} but found {param}", paramName );

            public static ArgumentException NotLessThanOrEqual<T>(T param, T expectedValue, string paramName) =>
                new ArgumentException( $"expected {paramName} to be less than or equal to {expectedValue} but found {param}", paramName );

            public static ArgumentException Between<T>(T param, T min, T max, string paramName) =>
                new ArgumentException( $"expected {paramName} to not be between {min} and {max} but found {param}", paramName );

            public static ArgumentException NotBetween<T>(T param, T min, T max, string paramName) =>
                new ArgumentException( $"expected {paramName} to be between {min} and {max} but found {param}", paramName );

            public static ArgumentException NotEmpty(string paramName) =>
                new ArgumentException( $"expected {paramName} to be empty", paramName );

            public static ArgumentException NotEmpty(string param, string paramName) =>
                new ArgumentException( $"expected {paramName} to be empty but found {param}", paramName );

            public static ArgumentException Empty(string paramName) =>
                new ArgumentException( $"expected {paramName} to not be empty", paramName );

            public static ArgumentException NotNullOrEmpty(string paramName) =>
                new ArgumentException( $"expected {paramName} to be null or empty", paramName );

            public static ArgumentException NotNullOrEmpty(string param, string paramName) =>
                new ArgumentException( $"expected {paramName} to be null or empty but found {param}", paramName );

            public static ArgumentException NullOrEmpty(string paramName) =>
                new ArgumentException( $"expected {paramName} to not be null or empty", paramName );

            public static ArgumentException NullOrWhiteSpace(string paramName) =>
                new ArgumentException( $"expected {paramName} to not be null or white space", paramName );

            public static ArgumentException NotContainsNull(string paramName) =>
                new ArgumentException( $"expected {paramName} to contain null", paramName );

            public static ArgumentException ContainsNull(string paramName) =>
                new ArgumentException( $"expected {paramName} to not contain null", paramName );

            public static ArgumentException NotContains<T>(T value, string paramName) =>
                new ArgumentException( $"expected {paramName} to contain {value}", paramName );

            public static ArgumentException Contains<T>(T value, string paramName) =>
                new ArgumentException( $"expected {paramName} to not contain {value}", paramName );

            public static ArgumentException NotAny(string description, string paramName) =>
                new ArgumentException( description ?? $"expected any {paramName} element to pass the predicate", paramName );

            public static ArgumentException NotAll(string description, string paramName) =>
                new ArgumentException( description ?? $"expected all {paramName} elements to pass the predicate", paramName );

            // TODO: other collection assertion exceptions

            public static ArgumentException False(string description) =>
                new ArgumentException( description ?? "expected condition to be true" );

            public static ArgumentException True(string description) =>
                new ArgumentException( description ?? "expected condition to be false" );
        }
    }
}
