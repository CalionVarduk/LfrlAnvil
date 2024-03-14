using System.Collections.Generic;
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
    private ReadOnlyArray<SqlColumnBuilder> _referencedComputationColumns;
    private SqlColumnBuilderNode? _node;

    protected SqlColumnBuilder(SqlTableBuilder table, string name, SqlColumnTypeDefinition typeDefinition)
        : base( table.Database, SqlObjectType.Column, name )
    {
        _referencedComputationColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Table = table;
        TypeDefinition = typeDefinition;
        IsNullable = false;
        DefaultValue = null;
        Computation = null;
    }

    public SqlTableBuilder Table { get; }
    public SqlColumnTypeDefinition TypeDefinition { get; private set; }
    public bool IsNullable { get; private set; }
    public SqlExpressionNode? DefaultValue { get; private set; }
    public SqlColumnComputation? Computation { get; private set; }

    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedComputationColumns =>
        SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedComputationColumns );

    public SqlColumnBuilderNode Node => _node ??= Table.Node[Name];

    ISqlTableBuilder ISqlColumnBuilder.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilder.TypeDefinition => TypeDefinition;

    IReadOnlyCollection<ISqlColumnBuilder> ISqlColumnBuilder.ReferencedComputationColumns =>
        _referencedComputationColumns.GetUnderlyingArray();

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

    public SqlColumnBuilder SetComputation(SqlColumnComputation? computation)
    {
        ThrowIfRemoved();
        var change = BeforeComputationChange( computation );
        if ( change.IsCancelled )
            return this;

        var originalValue = Computation;
        Computation = change.NewValue;
        AfterComputationChange( originalValue );
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
        {
            ThrowIfCannotHaveDefaultValue();
            ValidateDefaultValueExpression( newValue );
        }

        return newValue;
    }

    protected virtual void AfterDefaultValueChange(SqlExpressionNode? originalValue)
    {
        AddDefaultValueChange( this, originalValue );
    }

    protected virtual SqlPropertyChange<SqlColumnComputation?> BeforeComputationChange(SqlColumnComputation? newValue)
    {
        if ( newValue is null )
        {
            if ( Computation is null )
                return SqlPropertyChange.Cancel<SqlColumnComputation?>();

            ThrowIfReferenced();
            ClearComputationColumnReferences();
            return newValue;
        }

        if ( Computation is null )
        {
            ThrowIfReferenced();
            SetComputationColumnReferences( ValidateComputationExpression( newValue.Value.Expression ) );
            SetDefaultValue( null );
            return newValue;
        }

        if ( ReferenceEquals( Computation.Value.Expression, newValue.Value.Expression ) )
            return Computation.Value.Storage == newValue.Value.Storage ? SqlPropertyChange.Cancel<SqlColumnComputation?>() : newValue;

        ThrowIfReferenced();
        var computationColumns = ValidateComputationExpression( newValue.Value.Expression );
        ClearComputationColumnReferences();
        SetComputationColumnReferences( computationColumns );
        return newValue;
    }

    protected virtual void AfterComputationChange(SqlColumnComputation? originalValue)
    {
        AddComputationChange( this, originalValue );
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

        if ( Computation is null )
            return;

        ClearComputationColumnReferences();
        var computation = Computation;
        Computation = null;
        AfterComputationChange( computation );
    }

    protected override void AfterRemove()
    {
        AddRemoval( Table, this );
    }

    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedComputationColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Computation = null;
    }

    protected void ValidateDefaultValueExpression(SqlExpressionNode node)
    {
        var validator = CreateDefaultValueExpressionValidator();
        validator.Visit( node );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    [Pure]
    protected virtual SqlConstantExpressionValidator CreateDefaultValueExpressionValidator()
    {
        return new SqlConstantExpressionValidator();
    }

    [Pure]
    protected ReadOnlyArray<SqlColumnBuilder> ValidateComputationExpression(SqlExpressionNode expression)
    {
        var validator = CreateComputationExpressionValidator();
        validator.Visit( expression );

        var errors = validator.GetErrors();
        var result = validator.GetReferencedColumns();
        foreach ( var column in result )
        {
            if ( ReferenceEquals( column, this ) )
            {
                errors = errors.Extend( ExceptionResources.GeneratedColumnCannotReferenceSelf );
                break;
            }
        }

        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );

        return result;
    }

    [Pure]
    protected virtual SqlTableScopeExpressionValidator CreateComputationExpressionValidator()
    {
        return new SqlTableScopeExpressionValidator( Table );
    }

    protected void ThrowIfCannotHaveDefaultValue()
    {
        if ( Computation is not null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.GeneratedColumnCannotHaveDefaultValue ) );
    }

    protected void ThrowIfTypeDefinitionIsUnrecognized(SqlColumnTypeDefinition definition)
    {
        if ( ! Database.TypeDefinitions.Contains( definition ) )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.UnrecognizedTypeDefinition( definition ) ) );
    }

    protected void SetComputationColumnReferences(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedComputationColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Computation ) );
        foreach ( var column in _referencedComputationColumns )
            AddReference( column, refSource );
    }

    protected void ClearComputationColumnReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Computation ) );
        foreach ( var column in _referencedComputationColumns )
            RemoveReference( column, refSource );

        _referencedComputationColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void SetDefaultValueBasedOnDataType()
    {
        Assume.IsNull( Computation );
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

    ISqlColumnBuilder ISqlColumnBuilder.SetComputation(SqlColumnComputation? computation)
    {
        return SetComputation( computation );
    }
}
