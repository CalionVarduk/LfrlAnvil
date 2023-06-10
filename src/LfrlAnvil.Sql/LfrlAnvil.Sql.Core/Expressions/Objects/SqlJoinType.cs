namespace LfrlAnvil.Sql.Expressions.Objects;

public enum SqlJoinType : byte
{
    Inner = 0,
    Left = 1,
    Right = 2,
    Full = 3,
    Cross = 4
}
