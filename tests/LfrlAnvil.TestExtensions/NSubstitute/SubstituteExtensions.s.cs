using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.Core;

namespace LfrlAnvil.TestExtensions.NSubstitute
{
    public static class SubstituteExtensions
    {
        public static ConfiguredCall Returns<T>(this T substitute, IReadOnlyCollection<T> values)
        {
            return substitute.Returns( values.First(), values.Skip( 1 ).ToArray() );
        }
    }
}
