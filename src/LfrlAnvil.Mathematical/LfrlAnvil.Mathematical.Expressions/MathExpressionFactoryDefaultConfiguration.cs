namespace LfrlAnvil.Mathematical.Expressions;

public class MathExpressionFactoryDefaultConfiguration : IMathExpressionFactoryConfiguration
{
    public virtual char DecimalPoint => '.';
    public virtual char IntegerDigitSeparator => '_';
    public virtual string ScientificNotationExponents => "eE";
    public virtual bool AllowNonIntegerNumbers => true;
    public virtual bool AllowScientificNotation => true;
    public virtual char StringDelimiter => '\'';
    public virtual bool ConvertResultToOutputTypeAutomatically => true;
}
