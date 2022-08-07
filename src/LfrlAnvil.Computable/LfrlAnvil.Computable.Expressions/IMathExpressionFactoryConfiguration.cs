namespace LfrlAnvil.Computable.Expressions;

public interface IMathExpressionFactoryConfiguration
{
    char DecimalPoint { get; }
    char IntegerDigitSeparator { get; }
    string ScientificNotationExponents { get; }
    bool AllowNonIntegerNumbers { get; }
    bool AllowScientificNotation { get; }
    char StringDelimiter { get; }
    bool ConvertResultToOutputTypeAutomatically { get; }
}
