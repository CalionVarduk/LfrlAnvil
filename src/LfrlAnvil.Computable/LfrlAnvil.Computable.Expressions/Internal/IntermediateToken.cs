using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

// TODO: if StringSlice were to be extracted to Core, then this could possibly be made public
// & a new factory method for input tokenization only could be implemented
// also, a new factory method for parsing enumerable of tokens into expression
// + functionality to join an enumerable of tokens together into one input string
// this potentially would allow the user to arrange tokens themselves whichever way they want, using them like little building blocks
// note: there should probably be made a simplified, public-safe struct for 'external' tokens
// since this struct actually contains a construct token definition instance
// be careful with this since this could open the flood gates of a multitude of functionalities, which could require a lot of refactoring
// the simplified structure could simply hold a bitmask of construct types, giving a bit more info than just 'construct'
// maybe some helper methods for checking if a token can be followed by some other token
// or methods that give some context bitmasks, when a specific token combination could be considered correct (e.g. delegate invocation)
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
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.OpenedParenthesis ),
            "Assumed symbol to be " + nameof( TokenConstants.OpenedParenthesis ) + "." );

        return new IntermediateToken( IntermediateTokenType.OpenedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedParenthesis(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.ClosedParenthesis ),
            "Assumed symbol to be " + nameof( TokenConstants.ClosedParenthesis ) + "." );

        return new IntermediateToken( IntermediateTokenType.ClosedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateOpenedSquareBracket(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.OpenedSquareBracket ),
            "Assumed symbol to be " + nameof( TokenConstants.OpenedSquareBracket ) + "." );

        return new IntermediateToken( IntermediateTokenType.OpenedSquareBracket, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedSquareBracket(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.ClosedSquareBracket ),
            "Assumed symbol to be " + nameof( TokenConstants.ClosedSquareBracket ) + "." );

        return new IntermediateToken( IntermediateTokenType.ClosedSquareBracket, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateInlineFunctionSeparator(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.InlineFunctionSeparator ),
            "Assumed symbol to be " + nameof( TokenConstants.InlineFunctionSeparator ) + "." );

        return new IntermediateToken( IntermediateTokenType.InlineFunctionSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateElementSeparator(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.ElementSeparator ),
            "Assumed symbol to be " + nameof( TokenConstants.ElementSeparator ) + "." );

        return new IntermediateToken( IntermediateTokenType.ElementSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMemberAccess(StringSlice symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.MemberAccess ),
            "Assumed symbol to be " + nameof( TokenConstants.MemberAccess ) + "." );

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
