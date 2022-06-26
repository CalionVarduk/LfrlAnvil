namespace LfrlAnvil.Mathematical.Expressions.Internal;

public interface ITokenizerConfiguration
{
    char DecimalPoint { get; }
    char IntegerDigitSeparator { get; }
    string ScientificNotationExponents { get; }
    bool AllowScientificNotation { get; }
    bool AllowNonIntegerValues { get; }
    char StringDelimiter { get; }
}
