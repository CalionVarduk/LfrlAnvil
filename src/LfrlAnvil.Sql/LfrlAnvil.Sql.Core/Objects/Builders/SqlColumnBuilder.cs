using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlColumnBuilder : SqlObjectBuilder, ISqlColumnBuilder
{
    private SqlColumnBuilderNode? _node;

    protected SqlColumnBuilder(SqlTableBuilder table, string name, SqlColumnTypeDefinition typeDefinition)
        : base( table.Database, SqlObjectType.Column, name )
    {
        Table = table;
        TypeDefinition = typeDefinition;
        IsNullable = false;
        DefaultValue = null;
    }

    public SqlTableBuilder Table { get; }
    public SqlColumnTypeDefinition TypeDefinition { get; private set; }
    public bool IsNullable { get; private set; }
    public SqlExpressionNode? DefaultValue { get; private set; }
    public SqlColumnBuilderNode Node => _node ??= Table.Node[Name];

    ISqlTableBuilder ISqlColumnBuilder.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilder.TypeDefinition => TypeDefinition;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    public new SqlColumnBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public SqlColumnBuilder SetType(SqlColumnTypeDefinition definition)
    {
        ThrowIfRemoved();
        var change = BeforeTypeDefinitionChange( definition );
        if ( change.IsCancelled )
            return this;

        var originalValue = TypeDefinition;
        TypeDefinition = change.NewValue;
        AfterTypeDefinitionChange( originalValue );
        return this;
    }

    public SqlColumnBuilder MarkAsNullable(bool enabled = true)
    {
        ThrowIfRemoved();
        var change = BeforeIsNullableChange( enabled );
        if ( change.IsCancelled )
            return this;

        var originalValue = IsNullable;
        IsNullable = change.NewValue;
        AfterIsNullableChange( originalValue );
        return this;
    }

    public SqlColumnBuilder SetDefaultValue(SqlExpressionNode? value)
    {
        ThrowIfRemoved();
        var change = BeforeDefaultValueChange( value );
        if ( change.IsCancelled )
            return this;

        var originalValue = DefaultValue;
        DefaultValue = change.NewValue;
        AfterDefaultValueChange( originalValue );
        return this;
    }

    [Pure]
    public SqlOrderByNode Asc()
    {
        ThrowIfRemoved();
        return SqlNode.OrderByAsc( Node );
    }

    [Pure]
    public SqlOrderByNode Desc()
    {
        ThrowIfRemoved();
        return SqlNode.OrderByDesc( Node );
    }

    protected virtual SqlPropertyChange<SqlColumnTypeDefinition> BeforeTypeDefinitionChange(SqlColumnTypeDefinition newValue)
    {
        if ( ReferenceEquals( TypeDefinition, newValue ) )
            return SqlPropertyChange.Cancel<SqlColumnTypeDefinition>();

        ThrowIfReferenced();
        ThrowIfTypeDefinitionIsUnrecognized( newValue );
        SetDefaultValue( null );
        return newValue;
    }

    protected virtual void AfterTypeDefinitionChange(SqlColumnTypeDefinition originalValue)
    {
        AddTypeDefinitionChange( this, originalValue );
    }

    protected virtual SqlPropertyChange<bool> BeforeIsNullableChange(bool newValue)
    {
        if ( IsNullable == newValue )
            return SqlPropertyChange.Cancel<bool>();

        ThrowIfReferenced();
        return newValue;
    }

    protected virtual void AfterIsNullableChange(bool originalValue)
    {
        AddIsNullableChange( this, originalValue );
    }

    protected virtual SqlPropertyChange<SqlExpressionNode?> BeforeDefaultValueChange(SqlExpressionNode? newValue)
    {
        if ( ReferenceEquals( DefaultValue, newValue ) )
            return SqlPropertyChange.Cancel<SqlExpressionNode?>();

        if ( newValue is not null )
            ValidateDefaultValueExpression( newValue );

        return newValue;
    }

    protected virtual void AfterDefaultValueChange(SqlExpressionNode? originalValue)
    {
        AddDefaultValueChange( this, originalValue );
    }

    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ThrowIfReferenced();
        ChangeNameInCollection( Table.Columns, this, newValue );
        return change;
    }

    protected override void AfterNameChange(string originalValue)
    {
        AddNameChange( Table, this, originalValue );
    }

    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveFromCollection( Table.Columns, this );
    }

    protected override void AfterRemove()
    {
        AddRemoval( Table, this );
    }

    protected void ValidateDefaultValueExpression(SqlExpressionNode node)
    {
        // TODO:
        // move to configurable db builder interface (low priority, later)
        var validator = new SqlConstantExpressionValidator();
        validator.Visit( node );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    protected void ThrowIfTypeDefinitionIsUnrecognized(SqlColumnTypeDefinition definition)
    {
        if ( ! Database.TypeDefinitions.Contains( definition ) )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.UnrecognizedTypeDefinition( definition ) ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void SetDefaultValueBasedOnDataType()
    {
        DefaultValue = TypeDefinition.DefaultValue;
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetType(ISqlColumnTypeDefinition definition)
    {
        return SetType( SqlHelpers.CastOrThrow<SqlColumnTypeDefinition>( Database, definition ) );
    }

    ISqlColumnBuilder ISqlColumnBuilder.MarkAsNullable(bool enabled)
    {
        return MarkAsNullable( enabled );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetDefaultValue(SqlExpressionNode? value)
    {
        return SetDefaultValue( value );
    }
}
