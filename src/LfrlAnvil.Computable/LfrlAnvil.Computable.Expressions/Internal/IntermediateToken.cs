using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal readonly struct IntermediateToken
{
    private IntermediateToken(IntermediateTokenType type, StringSegment symbol, ConstructTokenDefinition? constructs = null)
    {
        Type = type;
        Symbol = symbol;
        Constructs = constructs;
    }

    public IntermediateTokenType Type { get; }
    public StringSegment Symbol { get; }
    public ConstructTokenDefinition? Constructs { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] \"{Symbol}\" at {Symbol.StartIndex}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateOpenedParenthesis(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.OpenedParenthesis ),
            "Assumed symbol to be " + nameof( TokenConstants.OpenedParenthesis ) + "." );

        return new IntermediateToken( IntermediateTokenType.OpenedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedParenthesis(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.ClosedParenthesis ),
            "Assumed symbol to be " + nameof( TokenConstants.ClosedParenthesis ) + "." );

        return new IntermediateToken( IntermediateTokenType.ClosedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateOpenedSquareBracket(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.OpenedSquareBracket ),
            "Assumed symbol to be " + nameof( TokenConstants.OpenedSquareBracket ) + "." );

        return new IntermediateToken( IntermediateTokenType.OpenedSquareBracket, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedSquareBracket(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.ClosedSquareBracket ),
            "Assumed symbol to be " + nameof( TokenConstants.ClosedSquareBracket ) + "." );

        return new IntermediateToken( IntermediateTokenType.ClosedSquareBracket, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateLineSeparator(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.LineSeparator ),
            "Assumed symbol to be " + nameof( TokenConstants.LineSeparator ) + "." );

        return new IntermediateToken( IntermediateTokenType.LineSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateElementSeparator(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.ElementSeparator ),
            "Assumed symbol to be " + nameof( TokenConstants.ElementSeparator ) + "." );

        return new IntermediateToken( IntermediateTokenType.ElementSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMemberAccess(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.MemberAccess ),
            "Assumed symbol to be " + nameof( TokenConstants.MemberAccess ) + "." );

        return new IntermediateToken( IntermediateTokenType.MemberAccess, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateStringConstant(StringSegment symbol)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        return new IntermediateToken( IntermediateTokenType.StringConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateNumberConstant(StringSegment symbol)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        var firstChar = symbol[0];
        Assume.True( char.IsDigit( firstChar ), "Assumed first symbol character to be a digit." );

        return new IntermediateToken( IntermediateTokenType.NumberConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateBooleanConstant(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.IsBooleanTrue( symbol ) || TokenConstants.IsBooleanFalse( symbol ),
            "Assumed symbol to be a boolean constant." );

        return new IntermediateToken( IntermediateTokenType.BooleanConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateVariableDeclaration(StringSegment symbol)
    {
        Assume.True( TokenConstants.IsVariableDeclaration( symbol ), "Assumed symbol to be a variable declaration." );
        return new IntermediateToken( IntermediateTokenType.VariableDeclaration, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMacroDeclaration(StringSegment symbol)
    {
        Assume.True( TokenConstants.IsMacroDeclaration( symbol ), "Assumed symbol to be a macro declaration." );
        return new IntermediateToken( IntermediateTokenType.MacroDeclaration, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateConstructs(StringSegment symbol, ConstructTokenDefinition constructs)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        return new IntermediateToken( IntermediateTokenType.Constructs, symbol, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateAssignment(StringSegment symbol)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.Assignment ),
            "Assumed symbol to be " + nameof( TokenConstants.Assignment ) + "." );

        return new IntermediateToken( IntermediateTokenType.Assignment, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateAssignmentWithConstructs(StringSegment symbol, ConstructTokenDefinition constructs)
    {
        Assume.True(
            TokenConstants.AreEqual( symbol, TokenConstants.Assignment ),
            "Assumed symbol to be " + nameof( TokenConstants.Assignment ) + "." );

        return new IntermediateToken( IntermediateTokenType.Assignment, symbol, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateArgument(StringSegment symbol)
    {
        Assume.IsNotEmpty( symbol, nameof( symbol ) );
        return new IntermediateToken( IntermediateTokenType.Argument, symbol );
    }
}
