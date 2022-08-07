using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct IntermediateToken
{
    private IntermediateToken(IntermediateTokenType type, StringSlice symbol, ConstructTokenDefinition? constructs = null)
    {
        Type = type;
        Symbol = symbol;
        Constructs = constructs;
    }

    public IntermediateTokenType Type { get; }
    public StringSlice Symbol { get; }
    public ConstructTokenDefinition? Constructs { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] \"{Symbol}\" at {Symbol.StartIndex}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateOpenedParenthesis(StringSlice symbol)
    {
        Assume.True( symbol.Equals( TokenConstants.OpenedParenthesis ), "Assumed symbol to be OpenedParenthesis." );
        return new IntermediateToken( IntermediateTokenType.OpenedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedParenthesis(StringSlice symbol)
    {
        Assume.True( symbol.Equals( TokenConstants.ClosedParenthesis ), "Assumed symbol to be ClosedParenthesis." );
        return new IntermediateToken( IntermediateTokenType.ClosedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateInlineFunctionSeparator(StringSlice symbol)
    {
        Assume.True( symbol.Equals( TokenConstants.InlineFunctionSeparator ), "Assumed symbol to be InlineFunctionSeparator." );
        return new IntermediateToken( IntermediateTokenType.InlineFunctionSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateFunctionParameterSeparator(StringSlice symbol)
    {
        Assume.True( symbol.Equals( TokenConstants.FunctionParameterSeparator ), "Assumed symbol to be FunctionParameterSeparator." );
        return new IntermediateToken( IntermediateTokenType.FunctionParameterSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMemberAccess(StringSlice symbol)
    {
        Assume.True( symbol.Equals( TokenConstants.MemberAccess ), "Assumed symbol to be MemberAccess." );
        return new IntermediateToken( IntermediateTokenType.MemberAccess, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateStringConstant(StringSlice symbol)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        return new IntermediateToken( IntermediateTokenType.StringConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateNumberConstant(StringSlice symbol)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        var firstChar = symbol[0];
        Assume.True( char.IsDigit( firstChar ), "Assumed first symbol character to be a digit." );

        return new IntermediateToken( IntermediateTokenType.NumberConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateBooleanConstant(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.IsBooleanTrue( symbol ) || TokenConstants.IsBooleanFalse( symbol ),
            "Assumed symbol to be a boolean constant." );

        return new IntermediateToken( IntermediateTokenType.BooleanConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateConstructs(StringSlice symbol, ConstructTokenDefinition constructs)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        return new IntermediateToken( IntermediateTokenType.Constructs, symbol, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateArgument(StringSlice symbol)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        return new IntermediateToken( IntermediateTokenType.Argument, symbol );
    }
}
