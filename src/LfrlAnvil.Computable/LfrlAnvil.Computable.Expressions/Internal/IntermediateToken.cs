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
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.OpenedParenthesis ) );
        return new IntermediateToken( IntermediateTokenType.OpenedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedParenthesis(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.ClosedParenthesis ) );
        return new IntermediateToken( IntermediateTokenType.ClosedParenthesis, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateOpenedSquareBracket(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.OpenedSquareBracket ) );
        return new IntermediateToken( IntermediateTokenType.OpenedSquareBracket, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateClosedSquareBracket(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.ClosedSquareBracket ) );
        return new IntermediateToken( IntermediateTokenType.ClosedSquareBracket, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateLineSeparator(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.LineSeparator ) );
        return new IntermediateToken( IntermediateTokenType.LineSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateElementSeparator(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.ElementSeparator ) );
        return new IntermediateToken( IntermediateTokenType.ElementSeparator, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMemberAccess(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.MemberAccess ) );
        return new IntermediateToken( IntermediateTokenType.MemberAccess, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateStringConstant(StringSegment symbol)
    {
        Assume.IsNotEmpty( symbol );
        return new IntermediateToken( IntermediateTokenType.StringConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateNumberConstant(StringSegment symbol)
    {
        Assume.IsNotEmpty( symbol );
        var firstChar = symbol[0];
        Assume.True( char.IsDigit( firstChar ) );

        return new IntermediateToken( IntermediateTokenType.NumberConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateBooleanConstant(StringSegment symbol)
    {
        Assume.True( TokenConstants.IsBooleanTrue( symbol ) || TokenConstants.IsBooleanFalse( symbol ) );
        return new IntermediateToken( IntermediateTokenType.BooleanConstant, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateVariableDeclaration(StringSegment symbol)
    {
        Assume.True( TokenConstants.IsVariableDeclaration( symbol ) );
        return new IntermediateToken( IntermediateTokenType.VariableDeclaration, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateMacroDeclaration(StringSegment symbol)
    {
        Assume.True( TokenConstants.IsMacroDeclaration( symbol ) );
        return new IntermediateToken( IntermediateTokenType.MacroDeclaration, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateConstructs(StringSegment symbol, ConstructTokenDefinition constructs)
    {
        Assume.IsNotEmpty( symbol );
        return new IntermediateToken( IntermediateTokenType.Constructs, symbol, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateAssignment(StringSegment symbol)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.Assignment ) );
        return new IntermediateToken( IntermediateTokenType.Assignment, symbol );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateAssignmentWithConstructs(StringSegment symbol, ConstructTokenDefinition constructs)
    {
        Assume.True( TokenConstants.AreEqual( symbol, TokenConstants.Assignment ) );
        return new IntermediateToken( IntermediateTokenType.Assignment, symbol, constructs );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static IntermediateToken CreateArgument(StringSegment symbol)
    {
        Assume.IsNotEmpty( symbol );
        return new IntermediateToken( IntermediateTokenType.Argument, symbol );
    }
}
