using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlColumnBuilder" />
public abstract class SqlColumnBuilder : SqlObjectBuilder, ISqlColumnBuilder
{
    private ReadOnlyArray<SqlColumnBuilder> _referencedComputationColumns;
    private SqlColumnBuilderNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlColumnBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this column belongs to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="typeDefinition"><see cref="ISqlColumnTypeDefinition"/> instance that defines the data type of this column.</param>
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

    /// <inheritdoc cref="ISqlColumnBuilder.Table" />
    public SqlTableBuilder Table { get; }

    /// <inheritdoc cref="ISqlColumnBuilder.TypeDefinition" />
    public SqlColumnTypeDefinition TypeDefinition { get; private set; }

    /// <inheritdoc />
    public bool IsNullable { get; private set; }

    /// <inheritdoc />
    public SqlExpressionNode? DefaultValue { get; private set; }

    /// <inheritdoc />
    public SqlColumnComputation? Computation { get; private set; }

    /// <inheritdoc cref="ISqlColumnBuilder.ReferencedComputationColumns" />
    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedComputationColumns =>
        SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedComputationColumns );

    /// <inheritdoc />
    public SqlColumnBuilderNode Node => _node ??= Table.Node[Name];

    ISqlTableBuilder ISqlColumnBuilder.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilder.TypeDefinition => TypeDefinition;

    IReadOnlyCollection<ISqlColumnBuilder> ISqlColumnBuilder.ReferencedComputationColumns =>
        _referencedComputationColumns.GetUnderlyingArray();

    /// <summary>
    /// Returns a string representation of this <see cref="SqlColumnBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    /// <inheritdoc cref="SqlObjectBuilder.SetName(string)" />
    public new SqlColumnBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="ISqlColumnBuilder.SetType(ISqlColumnTypeDefinition)" />
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

    /// <inheritdoc cref="ISqlColumnBuilder.MarkAsNullable(bool)" />
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

    /// <inheritdoc cref="ISqlColumnBuilder.SetDefaultValue(SqlExpressionNode)" />
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

    /// <inheritdoc cref="ISqlColumnBuilder.SetComputation(SqlColumnComputation?)" />
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

    /// <inheritdoc />
    [Pure]
    public SqlOrderByNode Asc()
    {
        ThrowIfRemoved();
        return SqlNode.OrderByAsc( Node );
    }

    /// <inheritdoc />
    [Pure]
    public SqlOrderByNode Desc()
    {
        ThrowIfRemoved();
        return SqlNode.OrderByDesc( Node );
    }

    /// <summary>
    /// Callback invoked just before <see cref="TypeDefinition"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="TypeDefinition"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="TypeDefinition"/> of this column cannot be changed.</exception>
    protected virtual SqlPropertyChange<SqlColumnTypeDefinition> BeforeTypeDefinitionChange(SqlColumnTypeDefinition newValue)
    {
        if ( ReferenceEquals( TypeDefinition, newValue ) )
            return SqlPropertyChange.Cancel<SqlColumnTypeDefinition>();

        ThrowIfReferenced();
        ThrowIfTypeDefinitionIsUnrecognized( newValue );
        SetDefaultValue( null );
        return newValue;
    }

    /// <summary>
    /// Callback invoked just after <see cref="TypeDefinition"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterTypeDefinitionChange(SqlColumnTypeDefinition originalValue)
    {
        AddTypeDefinitionChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just before <see cref="IsNullable"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="IsNullable"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="IsNullable"/> of this column cannot be changed.</exception>
    protected virtual SqlPropertyChange<bool> BeforeIsNullableChange(bool newValue)
    {
        if ( IsNullable == newValue )
            return SqlPropertyChange.Cancel<bool>();

        ThrowIfReferenced();
        return newValue;
    }

    /// <summary>
    /// Callback invoked just after <see cref="IsNullable"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterIsNullableChange(bool originalValue)
    {
        AddIsNullableChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just before <see cref="DefaultValue"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="DefaultValue"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="DefaultValue"/> of this column cannot be changed.</exception>
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

    /// <summary>
    /// Callback invoked just after <see cref="DefaultValue"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterDefaultValueChange(SqlExpressionNode? originalValue)
    {
        AddDefaultValueChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just before <see cref="Computation"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="Computation"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="Computation"/> of this column cannot be changed.</exception>
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

    /// <summary>
    /// Callback invoked just after <see cref="Computation"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterComputationChange(SqlColumnComputation? originalValue)
    {
        AddComputationChange( this, originalValue );
    }

    /// <inheritdoc />
    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ThrowIfReferenced();
        ChangeNameInCollection( Table.Columns, this, newValue );
        return change;
    }

    /// <inheritdoc />
    protected override void AfterNameChange(string originalValue)
    {
        AddNameChange( Table, this, originalValue );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        AddRemoval( Table, this );
    }

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedComputationColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Computation = null;
    }

    /// <summary>
    /// Checks if the provided expression is a valid <see cref="DefaultValue"/>.
    /// </summary>
    /// <param name="node">Expression to check.</param>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="node"/> is not valid.</exception>
    /// <remarks><see cref="CreateDefaultValueExpressionValidator()"/> creates expression's validator.</remarks>
    protected void ValidateDefaultValueExpression(SqlExpressionNode node)
    {
        var validator = CreateDefaultValueExpressionValidator();
        validator.Visit( node );

        var errors = validator.GetErrors();
        if ( errors.Count > 0 )
            throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    /// <summary>
    /// Creates a new <see cref="SqlConstantExpressionValidator"/> used for <see cref="DefaultValue"/> validation.
    /// </summary>
    /// <returns>New <see cref="SqlConstantExpressionValidator"/> instance.</returns>
    [Pure]
    protected virtual SqlConstantExpressionValidator CreateDefaultValueExpressionValidator()
    {
        return new SqlConstantExpressionValidator();
    }

    /// <summary>
    /// Checks if the provided expression is a valid <see cref="Computation"/> expression.
    /// </summary>
    /// <param name="expression">Expression to check.</param>
    /// <returns>Collection of columns referenced by the <paramref name="expression"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="expression"/> is not valid.</exception>
    /// <remarks><see cref="CreateComputationExpressionValidator()"/> creates expression's validator.</remarks>
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

    /// <summary>
    /// Creates a new <see cref="SqlTableScopeExpressionValidator"/> used for <see cref="Computation"/> expression validation.
    /// </summary>
    /// <returns>New <see cref="SqlTableScopeExpressionValidator"/> instance.</returns>
    [Pure]
    protected virtual SqlTableScopeExpressionValidator CreateComputationExpressionValidator()
    {
        return new SqlTableScopeExpressionValidator( Table );
    }

    /// <summary>
    /// Throws an exception when <see cref="DefaultValue"/> cannot be non-null.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When <see cref="DefaultValue"/> cannot be non-null.</exception>
    /// <remarks><see cref="Computation"/> must be null.</remarks>
    protected void ThrowIfCannotHaveDefaultValue()
    {
        if ( Computation is not null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.GeneratedColumnCannotHaveDefaultValue ) );
    }

    /// <summary>
    /// Throws an exception when the provided <paramref name="definition"/>
    /// does not exist in the database's <see cref="ISqlDatabaseBuilder.TypeDefinitions"/>.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="definition"/> is not recognized.</exception>
    protected void ThrowIfTypeDefinitionIsUnrecognized(SqlColumnTypeDefinition definition)
    {
        if ( ! Database.TypeDefinitions.Contains( definition ) )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.UnrecognizedTypeDefinition( definition ) ) );
    }

    /// <summary>
    /// Adds a collection of <paramref name="columns"/> to <see cref="ReferencedComputationColumns"/>
    /// and adds this column's <see cref="Computation"/> to their reference sources.
    /// </summary>
    /// <param name="columns">Collection of columns to add.</param>
    protected void SetComputationColumnReferences(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedComputationColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Computation ) );
        foreach ( var column in _referencedComputationColumns )
            AddReference( column, refSource );
    }

    /// <summary>
    /// Removes all columns from <see cref="ReferencedComputationColumns"/>
    /// and removes this column's <see cref="Computation"/> from their reference sources.
    /// </summary>
    protected void ClearComputationColumnReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this, property: nameof( Computation ) );
        foreach ( var column in _referencedComputationColumns )
            RemoveReference( column, refSource );

        _referencedComputationColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    /// <summary>
    /// Assigns <see cref="ISqlColumnTypeDefinition.DefaultValue"/> of the current <see cref="TypeDefinition"/>
    /// to the <see cref="DefaultValue"/> without notifying the change tracker.
    /// </summary>
    /// <remarks>This method assumes that the current <see cref="Computation"/> is null.</remarks>
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
