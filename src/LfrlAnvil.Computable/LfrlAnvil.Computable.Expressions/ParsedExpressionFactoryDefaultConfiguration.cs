namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents an <see cref="IParsedExpressionFactory"/> configuration with default values.
/// </summary>
public class ParsedExpressionFactoryDefaultConfiguration : IParsedExpressionFactoryConfiguration
{
    /// <inheritdoc />
    public virtual char DecimalPoint => '.';

    /// <inheritdoc />
    public virtual char IntegerDigitSeparator => '_';

    /// <inheritdoc />
    public virtual string ScientificNotationExponents => "eE";

    /// <inheritdoc />
    public virtual bool AllowNonIntegerNumbers => true;

    /// <inheritdoc />
    public virtual bool AllowScientificNotation => true;

    /// <inheritdoc />
    public virtual char StringDelimiter => '\'';

    /// <inheritdoc />
    public virtual bool ConvertResultToOutputTypeAutomatically => true;

    /// <inheritdoc />
    public virtual bool AllowNonPublicMemberAccess => false;

    /// <inheritdoc />
    public virtual bool IgnoreMemberNameCase => false;

    /// <inheritdoc />
    public virtual bool PostponeStaticInlineDelegateCompilation => false;

    /// <inheritdoc />
    public virtual bool DiscardUnusedArguments => true;
}
