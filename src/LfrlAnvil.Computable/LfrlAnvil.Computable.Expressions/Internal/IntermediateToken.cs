using System.Diagnostics;
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
        Debug.Assert( symbol.Equals( TokenConstants.OpenedParenthesis ), "symbol is OpenedParenthesis" );
        return new IntermediateToken( IntermediateTokenType.OpenedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedParenthesis(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.ClosedParenthesis ), "symbol is ClosedParenthesis" );
        return new IntermediateToken( IntermediateTokenType.ClosedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateInlineFunctionSeparator(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.InlineFunctionSeparator ), "symbol is InlineFunctionSeparator" );
        return new IntermediateToken( IntermediateTokenType.InlineFunctionSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateFunctionParameterSeparator(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.FunctionParameterSeparator ), "symbol is FunctionParameterSeparator" );
        return new IntermediateToken( IntermediateTokenType.FunctionParameterSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMemberAccess(StringSlice symbol)
    {
        Debug.Assert( symbol.Equals( TokenConstants.MemberAccess ), "symbol is MemberAccess" );
        return new IntermediateToken( IntermediateTokenType.MemberAccess, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateStringConstant(StringSlice symbol)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        return new IntermediateToken( IntermediateTokenType.StringConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateNumberConstant(StringSlice symbol)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        var firstChar = symbol[0];
        Debug.Assert( char.IsDigit( firstChar ), "first symbol character is a digit" );

        return new IntermediateToken( IntermediateTokenType.NumberConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateBooleanConstant(StringSlice symbol)
    {
        Debug.Assert( TokenConstants.IsBooleanTrue( symbol ) || TokenConstants.IsBooleanFalse( symbol ), "symbol is boolean constant" );
        return new IntermediateToken( IntermediateTokenType.BooleanConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateConstructs(StringSlice symbol, ConstructTokenDefinition constructs)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        return new IntermediateToken( IntermediateTokenType.Constructs, symbol, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateArgument(StringSlice symbol)
    {
        Debug.Assert( symbol.Length > 0, "symbol.Length > 0" );
        return new IntermediateToken( IntermediateTokenType.Argument, symbol );
    }
}
