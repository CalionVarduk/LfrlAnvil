using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal.Delegates;

internal readonly struct ClosureExpressionFactory
{
    private readonly Segment[] _segments;
    private readonly ParameterMapping[] _parameterMappings;

    internal ClosureExpressionFactory(
        ParameterExpression parameter,
        ExpressionReplacement[] capturedParameters,
        Segment[] segments,
        ParameterMapping[] parameterMappings)
    {
        Parameter = parameter;
        CapturedParameters = capturedParameters;
        _segments = segments;
        _parameterMappings = parameterMappings;
    }

    internal ParameterExpression Parameter { get; }
    internal ExpressionReplacement[] CapturedParameters { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal NewExpression CreateCtorCallForRootDelegate()
    {
        Assume.IsNotEmpty( _parameterMappings, nameof( _parameterMappings ) );

        var result = CreateLastRootSegmentExpression();
        for ( var i = _segments.Length - 2; i >= 0; --i )
            result = CreateRootSegmentExpression( i, result );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal NewExpression CreateCtorCallForNestedDelegate(ClosureExpressionFactory parentFactory)
    {
        Assume.IsNotEmpty( _parameterMappings, nameof( _parameterMappings ) );

        var result = CreateLastNestedSegmentExpression( parentFactory );
        for ( var i = _segments.Length - 2; i >= 0; --i )
            result = CreateNestedClosureSegmentExpression( parentFactory, i, result );

        return result;
    }

    [Pure]
    private NewExpression CreateLastRootSegmentExpression()
    {
        var segment = _segments[^1];
        var ctorParameters = new Expression[segment.ParameterCount];

        for ( var i = 0; i < segment.ParameterCount; ++i )
            ctorParameters[i] = GetRootCtorParameter( i + segment.FirstParameterIndex );

        return Expression.New( segment.Ctor, ctorParameters );
    }

    [Pure]
    private NewExpression CreateRootSegmentExpression(int segmentIndex, NewExpression tail)
    {
        var segment = _segments[segmentIndex];
        var ctorParameters = new Expression[segment.ParameterCount + 1];
        ctorParameters[^1] = tail;

        for ( var i = 0; i < segment.ParameterCount; ++i )
            ctorParameters[i] = GetRootCtorParameter( i + segment.FirstParameterIndex );

        return Expression.New( segment.Ctor, ctorParameters );
    }

    [Pure]
    private NewExpression CreateLastNestedSegmentExpression(ClosureExpressionFactory parentFactory)
    {
        var segment = _segments[^1];
        var ctorParameters = new Expression[segment.ParameterCount];

        for ( var i = 0; i < segment.ParameterCount; ++i )
            ctorParameters[i] = GetNestedCtorParameter( i + segment.FirstParameterIndex, parentFactory );

        return Expression.New( segment.Ctor, ctorParameters );
    }

    [Pure]
    private NewExpression CreateNestedClosureSegmentExpression(ClosureExpressionFactory parentFactory, int segmentIndex, NewExpression tail)
    {
        var segment = _segments[segmentIndex];
        var ctorParameters = new Expression[segment.ParameterCount + 1];
        ctorParameters[^1] = tail;

        for ( var i = 0; i < segment.ParameterCount; ++i )
            ctorParameters[i] = GetNestedCtorParameter( i + segment.FirstParameterIndex, parentFactory );

        return Expression.New( segment.Ctor, ctorParameters );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression GetRootCtorParameter(int index)
    {
        var parameter = _parameterMappings[index].Parameter;
        Assume.IsNotNull( parameter, nameof( parameter ) );
        return parameter;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression GetNestedCtorParameter(int index, ClosureExpressionFactory parentClosure)
    {
        var mapping = _parameterMappings[index];
        if ( mapping.IsFromParentParameter )
            return mapping.Parameter;

        var parentMemberAccess = parentClosure.CapturedParameters[mapping.ParentClosureIndex].Replacement;
        return parentMemberAccess;
    }

    internal readonly struct Segment
    {
        internal readonly ConstructorInfo Ctor;
        internal readonly int FirstParameterIndex;
        internal readonly int ParameterCount;

        internal Segment(ConstructorInfo ctor, int firstParameterIndex, int parameterCount)
        {
            Ctor = ctor;
            FirstParameterIndex = firstParameterIndex;
            ParameterCount = parameterCount;
        }
    }

    internal readonly struct ParameterMapping
    {
        private ParameterMapping(int parentClosureIndex, ParameterExpression? parameter)
        {
            ParentClosureIndex = parentClosureIndex;
            Parameter = parameter;
        }

        internal readonly int ParentClosureIndex;
        internal readonly ParameterExpression? Parameter;

        [MemberNotNullWhen( true, nameof( Parameter ) )]
        internal bool IsFromParentParameter => Parameter is not null;

        [Pure]
        public override string ToString()
        {
            return IsFromParentParameter ? $"{nameof( Parameter )}: {Parameter}" : $"{nameof( ParentClosureIndex )}: {ParentClosureIndex}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ParameterMapping CreateFromParentParameter(ParameterExpression parameter)
        {
            return new ParameterMapping( -1, parameter );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ParameterMapping CreateFromNestedClosure(int parentClosureIndex)
        {
            Assume.IsGreaterThanOrEqualTo( parentClosureIndex, 0, nameof( parentClosureIndex ) );
            return new ParameterMapping( parentClosureIndex, null );
        }
    }
}
