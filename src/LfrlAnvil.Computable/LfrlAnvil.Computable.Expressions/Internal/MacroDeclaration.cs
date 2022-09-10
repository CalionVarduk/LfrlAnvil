using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class MacroDeclaration
{
    private readonly List<IntermediateToken> _tokens;

    internal MacroDeclaration(StringSlice name)
    {
        Name = name;
        _tokens = new List<IntermediateToken>();
    }

    // TODO: unused
    internal StringSlice Name { get; }
    internal IReadOnlyList<IntermediateToken> Tokens => _tokens;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddToken(IntermediateToken token)
    {
        _tokens.Add( token );
    }
}
