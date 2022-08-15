namespace LfrlAnvil.Computable.Expressions;

public class ParsedExpressionFactoryDefaultConfiguration : IParsedExpressionFactoryConfiguration
{
    public virtual char DecimalPoint => '.';
    public virtual char IntegerDigitSeparator => '_';
    public virtual string ScientificNotationExponents => "eE";
    public virtual bool AllowNonIntegerNumbers => true;
    public virtual bool AllowScientificNotation => true;
    public virtual char StringDelimiter => '\'';
    public virtual bool ConvertResultToOutputTypeAutomatically => true;
    public virtual bool AllowNonPublicMemberAccess => false;
    public virtual bool IgnoreMemberNameCase => false;
}
