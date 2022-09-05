namespace LfrlAnvil.Computable.Expressions;

public interface IParsedExpressionFactoryConfiguration
{
    char DecimalPoint { get; }
    char IntegerDigitSeparator { get; }
    string ScientificNotationExponents { get; }
    bool AllowNonIntegerNumbers { get; }
    bool AllowScientificNotation { get; }
    char StringDelimiter { get; }
    bool ConvertResultToOutputTypeAutomatically { get; }
    bool AllowNonPublicMemberAccess { get; }
    bool IgnoreMemberNameCase { get; }
    bool PostponeStaticInlineDelegateCompilation { get; }
    bool DiscardUnusedArguments { get; }
}
