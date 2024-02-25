using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class MacroDeclaration
{
    private readonly List<Token> _tokens;
    private readonly Dictionary<StringSegment, int>? _parameterIndexes;

    internal MacroDeclaration(Dictionary<StringSegment, int>? parameterIndexes)
    {
        _tokens = new List<Token>();
        _parameterIndexes = parameterIndexes;
    }

    internal bool IsEmpty => _tokens.Count == 0;
    internal int ParameterCount => _parameterIndexes?.Count ?? 0;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddToken(IntermediateToken token)
    {
        var result = token.Type == IntermediateTokenType.Argument &&
            _parameterIndexes is not null &&
            _parameterIndexes.TryGetValue( token.Symbol, out var index )
                ? Token.CreateFromParameter( token, index )
                : Token.Create( token );

        _tokens.Add( result );
    }

    internal Chain<ParsedExpressionBuilderError> Process(
        ExpressionBuilderRootState state,
        IntermediateToken token,
        IReadOnlyList<IReadOnlyList<IntermediateToken>> parameterTokens)
    {
        Assume.ContainsExactly( parameterTokens, ParameterCount );

        foreach ( var macroToken in _tokens )
        {
            var errors = macroToken.IsFromParameter
                ? HandleParameterTokens( macroToken, state, parameterTokens[macroToken.ParameterIndex] )
                : state.HandleToken( macroToken.Value );

            if ( errors.Count == 0 )
                continue;

            errors = Chain.Create( ParsedExpressionBuilderError.CreateMacroResolutionFailure( token, errors ) );
            return errors;
        }

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Chain<ParsedExpressionBuilderError> HandleParameterTokens(
        Token token,
        ExpressionBuilderRootState state,
        IReadOnlyList<IntermediateToken> parameterTokens)
    {
        Assume.True( token.IsFromParameter );

        foreach ( var parameterToken in parameterTokens )
        {
            var errors = state.HandleToken( parameterToken );
            if ( errors.Count > 0 )
                return Chain.Create( ParsedExpressionBuilderError.CreateMacroParameterResolutionFailure( token.Value, errors ) );
        }

        return Chain<ParsedExpressionBuilderError>.Empty;
    }

    private readonly struct Token
    {
        internal readonly IntermediateToken Value;
        internal readonly int ParameterIndex;

        private Token(IntermediateToken value, int parameterIndex)
        {
            Value = value;
            ParameterIndex = parameterIndex;
        }

        internal bool IsFromParameter => ParameterIndex >= 0;

        [Pure]
        public override string ToString()
        {
            return IsFromParameter ? $"{nameof( ParameterIndex )}: {ParameterIndex}" : $"{nameof( Value )}: {Value}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Token CreateFromParameter(IntermediateToken value, int parameterIndex)
        {
            Assume.IsGreaterThanOrEqualTo( parameterIndex, 0 );
            return new Token( value, parameterIndex );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static Token Create(IntermediateToken value)
        {
            return new Token( value, -1 );
        }
    }
}
