namespace LfrlAnvil.Sql.Expressions;

public enum SqlCompoundQueryOperator : byte
{
    Union = 0,
    UnionAll = 1,
    Intersect = 2,
    Except = 3
}
