using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Internal.Delegates;

internal static class ClosureHelpers
{
    internal const int MaxSegmentLength = 8;

    private static readonly MethodInfo[] OpenGenericBindMethods = typeof( ClosureHelpers )
        .GetMethods( BindingFlags.Static | BindingFlags.NonPublic )
        .Where( m => m.IsGenericMethodDefinition && m.Name == nameof( BindClosure ) )
        .OrderBy( m => m.GetGenericArguments().Length )
        .ToArray();

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Type FindDelegateWithCaptureType(Expression body, ParameterExpression[] parameters)
    {
        Assume.IsNotEmpty( parameters, nameof( parameters ) );
        Assume.IsNull( parameters[0], nameof( parameters ) + "[0]" );

        var bindMethodIndex = parameters.Length - 1;
        if ( bindMethodIndex >= OpenGenericBindMethods.Length )
            throw new UnsupportedDelegateParameterCountException( bindMethodIndex );

        var genericArgs = new Type[parameters.Length];
        for ( var i = 1; i < parameters.Length; ++i )
            genericArgs[i - 1] = parameters[i].Type;

        genericArgs[^1] = body.Type;

        var method = OpenGenericBindMethods[bindMethodIndex];
        var openFuncType = method.ReturnType.GetGenericTypeDefinition();
        var result = openFuncType.MakeGenericType( genericArgs );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MethodInfo FindBindClosureMethod(Expression body, ParameterExpression[] parameters)
    {
        Assume.ContainsInRange( parameters, 1, 16, nameof( parameters ) );

        var genericArgs = new Type[parameters.Length + 1];
        for ( var i = 0; i < parameters.Length; ++i )
            genericArgs[i] = parameters[i].Type;

        genericArgs[^1] = body.Type;

        var bindMethodIndex = parameters.Length - 1;
        var method = OpenGenericBindMethods[bindMethodIndex];
        var result = method.MakeGenericMethod( genericArgs );
        return result;
    }

    [Pure]
    internal static ClosureExpressionFactory CreateClosureTypeFactory(
        InlineDelegateCollectionState.ClosureInfo capturedParameters,
        InlineDelegateCollectionState.ClosureInfo? parentCapturedParameters)
    {
        Assume.IsNotEmpty( capturedParameters.Parameters, nameof( capturedParameters ) );
        Assume.Conditional(
            parentCapturedParameters is not null,
            () => Assume.IsNotEmpty( parentCapturedParameters!.Value.Parameters, nameof( parentCapturedParameters ) ) );

        var parameters = new ExpressionReplacement[capturedParameters.Parameters.Count];
        for ( var i = 0; i < parameters.Length; ++i )
            parameters[i] = new ExpressionReplacement( capturedParameters.Parameters[i].Expression );

        var segments = CreateClosureTypeSegments( parameters );
        var captureParameter = Expression.Parameter( segments[0].Ctor.DeclaringType!, $"__C{capturedParameters.StateId}" );
        var parameterMappings = CreateClosureParameterMappings( capturedParameters, parentCapturedParameters );
        PopulateCapturedParameterReplacements( captureParameter, parameters, segments );

        return new ClosureExpressionFactory( captureParameter, parameters, segments, parameterMappings );
    }

    [Pure]
    private static ClosureExpressionFactory.Segment[] CreateClosureTypeSegments(ExpressionReplacement[] parameters)
    {
        var (segmentCount, lastSegmentLength) = GetClosureTypeSegmentsCount( parameters.Length );
        var segments = new ClosureExpressionFactory.Segment[segmentCount];

        var firstParameterIndex = parameters.Length - lastSegmentLength;
        var openClosureType = GetOpenGenericClosureType( lastSegmentLength );
        var segmentParameters = parameters.AsSpan( firstParameterIndex, lastSegmentLength );
        var closureTypeCtor = GetClosureTypeCtor( openClosureType, segmentParameters, tailType: null );
        segments[^1] = new ClosureExpressionFactory.Segment( closureTypeCtor, firstParameterIndex, lastSegmentLength );

        for ( var i = segments.Length - 2; i >= 0; --i )
        {
            firstParameterIndex -= MaxSegmentLength - 1;
            openClosureType = GetOpenGenericClosureType( MaxSegmentLength );
            segmentParameters = parameters.AsSpan( firstParameterIndex, MaxSegmentLength - 1 );
            closureTypeCtor = GetClosureTypeCtor( openClosureType, segmentParameters, tailType: segments[i + 1].Ctor.DeclaringType! );
            segments[i] = new ClosureExpressionFactory.Segment( closureTypeCtor, firstParameterIndex, MaxSegmentLength - 1 );
        }

        return segments;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static (int SegmentCount, int LastSegmentLength) GetClosureTypeSegmentsCount(int parameterCount)
    {
        Assume.IsGreaterThan( parameterCount, 0, nameof( parameterCount ) );

        var segmentCount = parameterCount / (MaxSegmentLength - 1) + 1;
        var lastSegmentLength = parameterCount % (MaxSegmentLength - 1);

        if ( lastSegmentLength == 0 )
        {
            lastSegmentLength = MaxSegmentLength - 1;
            --segmentCount;
        }
        else if ( lastSegmentLength == 1 && segmentCount > 1 )
        {
            lastSegmentLength = MaxSegmentLength;
            --segmentCount;
        }

        return (segmentCount, lastSegmentLength);
    }

    private static void PopulateCapturedParameterReplacements(
        Expression captureParameter,
        ExpressionReplacement[] parameters,
        ClosureExpressionFactory.Segment[] segments)
    {
        var segmentParameterCount = segments[0].ParameterCount;
        for ( var i = 0; i < segmentParameterCount; ++i )
            parameters[i] = parameters[i].SetReplacement( CreateClosureParameterMemberAccess( captureParameter, i ) );

        if ( segments.Length > 1 )
        {
            var tailMemberAccess = CreateClosureParameterMemberAccess( captureParameter, MaxSegmentLength - 1 );
            for ( var i = segmentParameterCount; i < parameters.Length; ++i )
                parameters[i] = parameters[i].SetReplacement( tailMemberAccess );

            var segmentIndex = 1;
            while ( true )
            {
                var firstParameterIndex = segments[segmentIndex].FirstParameterIndex;
                segmentParameterCount = segments[segmentIndex].ParameterCount;

                for ( var i = 0; i < segmentParameterCount; ++i )
                {
                    var index = i + firstParameterIndex;
                    parameters[index] = parameters[index]
                        .SetReplacement( CreateClosureParameterMemberAccess( parameters[index].Replacement, i ) );
                }

                if ( ++segmentIndex == segments.Length )
                    break;

                var tailIndex = firstParameterIndex + segmentParameterCount;
                tailMemberAccess = CreateClosureParameterMemberAccess( parameters[tailIndex].Replacement, MaxSegmentLength - 1 );
                for ( var i = tailIndex; i < parameters.Length; ++i )
                    parameters[i] = parameters[i].SetReplacement( tailMemberAccess );
            }
        }
    }

    [Pure]
    private static MemberExpression CreateClosureParameterMemberAccess(Expression target, int parameterIndex)
    {
        var memberName = GetParameterMemberAccessName( parameterIndex );
        var field = target.Type.GetField( memberName )!;
        var result = Expression.MakeMemberAccess( target, field );
        return result;
    }

    private static ClosureExpressionFactory.ParameterMapping[] CreateClosureParameterMappings(
        InlineDelegateCollectionState.ClosureInfo capturedParameters,
        InlineDelegateCollectionState.ClosureInfo? parentCapturedParameters)
    {
        var parameterMappings = new ClosureExpressionFactory.ParameterMapping[capturedParameters.Parameters.Count];
        if ( parentCapturedParameters is null )
        {
            for ( var i = 0; i < parameterMappings.Length; ++i )
            {
                parameterMappings[i] = ClosureExpressionFactory.ParameterMapping.CreateFromParentParameter(
                    capturedParameters.Parameters[i].Expression );
            }

            return parameterMappings;
        }

        for ( var i = 0; i < parameterMappings.Length; ++i )
        {
            var capture = capturedParameters.Parameters[i];
            if ( capture.OwnerStateId == parentCapturedParameters.Value.StateId )
            {
                parameterMappings[i] = ClosureExpressionFactory.ParameterMapping.CreateFromParentParameter( capture.Expression );
                continue;
            }

            var parentClosureIndex = parentCapturedParameters.Value.FindIndex( capture.Expression );
            parameterMappings[i] = ClosureExpressionFactory.ParameterMapping.CreateFromNestedClosure( parentClosureIndex );
        }

        return parameterMappings;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ConstructorInfo GetClosureTypeCtor(Type openClosureType, ReadOnlySpan<ExpressionReplacement> parameters, Type? tailType)
    {
        var types = CreateClosureArgumentTypeBuffer( parameters.Length, tailType );
        for ( var i = 0; i < parameters.Length; ++i )
            types[i] = parameters[i].Original.Type;

        var closedClosureType = openClosureType.MakeGenericType( types );
        var ctor = closedClosureType.GetConstructor( BindingFlags.Instance | BindingFlags.NonPublic, types )!;
        return ctor;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type[] CreateClosureArgumentTypeBuffer(int parameterCount, Type? tailType)
    {
        if ( tailType is null )
            return new Type[parameterCount];

        var result = new Type[parameterCount + 1];
        result[^1] = tailType;
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type GetOpenGenericClosureType(int parameterCount)
    {
        Assume.IsGreaterThan( parameterCount, 0, nameof( parameterCount ) );

        return parameterCount switch
        {
            1 => typeof( LambdaClosure<> ),
            2 => typeof( LambdaClosure<,> ),
            3 => typeof( LambdaClosure<,,> ),
            4 => typeof( LambdaClosure<,,,> ),
            5 => typeof( LambdaClosure<,,,,> ),
            6 => typeof( LambdaClosure<,,,,,> ),
            7 => typeof( LambdaClosure<,,,,,,> ),
            _ => typeof( LambdaClosure<,,,,,,,> )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetParameterMemberAccessName(int parameterIndex)
    {
        Assume.IsInRange( parameterIndex, 0, MaxSegmentLength - 1, nameof( parameterIndex ) );

        return parameterIndex switch
        {
            0 => nameof( LambdaClosure<int>._1 ),
            1 => nameof( LambdaClosure<int, int>._2 ),
            2 => nameof( LambdaClosure<int, int, int>._3 ),
            3 => nameof( LambdaClosure<int, int, int, int>._4 ),
            4 => nameof( LambdaClosure<int, int, int, int, int>._5 ),
            5 => nameof( LambdaClosure<int, int, int, int, int, int>._6 ),
            6 => nameof( LambdaClosure<int, int, int, int, int, int, int>._7 ),
            _ => nameof( LambdaClosure<int, int, int, int, int, int, int, int>._8 ),
        };
    }

    private sealed class LambdaClosure<T1>
    {
        public readonly T1 _1;

        private LambdaClosure(T1 _1)
        {
            this._1 = _1;
        }
    }

    private sealed class LambdaClosure<T1, T2>
    {
        public readonly T1 _1;
        public readonly T2 _2;

        private LambdaClosure(T1 _1, T2 _2)
        {
            this._1 = _1;
            this._2 = _2;
        }
    }

    private sealed class LambdaClosure<T1, T2, T3>
    {
        public readonly T1 _1;
        public readonly T2 _2;
        public readonly T3 _3;

        private LambdaClosure(T1 _1, T2 _2, T3 _3)
        {
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
        }
    }

    private sealed class LambdaClosure<T1, T2, T3, T4>
    {
        public readonly T1 _1;
        public readonly T2 _2;
        public readonly T3 _3;
        public readonly T4 _4;

        private LambdaClosure(T1 _1, T2 _2, T3 _3, T4 _4)
        {
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
        }
    }

    private sealed class LambdaClosure<T1, T2, T3, T4, T5>
    {
        public readonly T1 _1;
        public readonly T2 _2;
        public readonly T3 _3;
        public readonly T4 _4;
        public readonly T5 _5;

        private LambdaClosure(T1 _1, T2 _2, T3 _3, T4 _4, T5 _5)
        {
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
            this._5 = _5;
        }
    }

    private sealed class LambdaClosure<T1, T2, T3, T4, T5, T6>
    {
        public readonly T1 _1;
        public readonly T2 _2;
        public readonly T3 _3;
        public readonly T4 _4;
        public readonly T5 _5;
        public readonly T6 _6;

        private LambdaClosure(T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6)
        {
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
            this._5 = _5;
            this._6 = _6;
        }
    }

    private sealed class LambdaClosure<T1, T2, T3, T4, T5, T6, T7>
    {
        public readonly T1 _1;
        public readonly T2 _2;
        public readonly T3 _3;
        public readonly T4 _4;
        public readonly T5 _5;
        public readonly T6 _6;
        public readonly T7 _7;

        private LambdaClosure(T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7)
        {
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
            this._5 = _5;
            this._6 = _6;
            this._7 = _7;
        }
    }

    private sealed class LambdaClosure<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public readonly T1 _1;
        public readonly T2 _2;
        public readonly T3 _3;
        public readonly T4 _4;
        public readonly T5 _5;
        public readonly T6 _6;
        public readonly T7 _7;
        public readonly T8 _8;

        private LambdaClosure(T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8)
        {
            this._1 = _1;
            this._2 = _2;
            this._3 = _3;
            this._4 = _4;
            this._5 = _5;
            this._6 = _6;
            this._7 = _7;
            this._8 = _8;
        }
    }

    [Pure]
    private static Func<TReturn> BindClosure<TClosure, TReturn>(TClosure closure, Func<TClosure, TReturn> lambda)
    {
        return () => lambda( closure );
    }

    [Pure]
    private static Func<T1, TReturn> BindClosure<TClosure, T1, TReturn>(TClosure closure, Func<TClosure, T1, TReturn> lambda)
    {
        return t1 => lambda( closure, t1 );
    }

    [Pure]
    private static Func<T1, T2, TReturn> BindClosure<TClosure, T1, T2, TReturn>(TClosure closure, Func<TClosure, T1, T2, TReturn> lambda)
    {
        return (t1, t2) => lambda( closure, t1, t2 );
    }

    [Pure]
    private static Func<T1, T2, T3, TReturn> BindClosure<TClosure, T1, T2, T3, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, TReturn> lambda)
    {
        return (t1, t2, t3) => lambda( closure, t1, t2, t3 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, TReturn> BindClosure<TClosure, T1, T2, T3, T4, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, TReturn> lambda)
    {
        return (t1, t2, t3, t4) => lambda( closure, t1, t2, t3, t4 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, TReturn> BindClosure<TClosure, T1, T2, T3, T4, T5, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5) => lambda( closure, t1, t2, t3, t4, t5 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, TReturn> BindClosure<TClosure, T1, T2, T3, T4, T5, T6, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6) => lambda( closure, t1, t2, t3, t4, t5, t6 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, TReturn> BindClosure<TClosure, T1, T2, T3, T4, T5, T6, T7, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7) => lambda( closure, t1, t2, t3, t4, t5, t6, t7 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> BindClosure<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8) => lambda( closure, t1, t2, t3, t4, t5, t6, t7, t8 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> BindClosure<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9) => lambda( closure, t1, t2, t3, t4, t5, t6, t7, t8, t9 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> BindClosure<
        TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10) => lambda( closure, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> BindClosure<
        TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11) => lambda( closure, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> BindClosure<
        TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12) => lambda( closure, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> BindClosure<
        TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13) => lambda(
            closure,
            t1,
            t2,
            t3,
            t4,
            t5,
            t6,
            t7,
            t8,
            t9,
            t10,
            t11,
            t12,
            t13 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> BindClosure<
        TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14) => lambda(
            closure,
            t1,
            t2,
            t3,
            t4,
            t5,
            t6,
            t7,
            t8,
            t9,
            t10,
            t11,
            t12,
            t13,
            t14 );
    }

    [Pure]
    private static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> BindClosure<
        TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>(
        TClosure closure,
        Func<TClosure, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> lambda)
    {
        return (t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15) => lambda(
            closure,
            t1,
            t2,
            t3,
            t4,
            t5,
            t6,
            t7,
            t8,
            t9,
            t10,
            t11,
            t12,
            t13,
            t14,
            t15 );
    }
}
