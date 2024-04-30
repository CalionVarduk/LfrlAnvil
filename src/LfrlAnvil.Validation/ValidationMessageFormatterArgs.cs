using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

/// <summary>
/// Represents miscellaneous <see cref="ValidationMessageFormatter{TResource}"/> arguments.
/// </summary>
public readonly struct ValidationMessageFormatterArgs
{
    /// <summary>
    /// Default <see cref="ValidationMessageFormatterArgs"/> instance.
    /// </summary>
    public static readonly ValidationMessageFormatterArgs Default = new ValidationMessageFormatterArgs();

    private readonly string? _separator;

    private ValidationMessageFormatterArgs(
        bool includeIndex,
        string? prefixAll,
        string? postfixAll,
        string? prefixEach,
        string? postfixEach,
        string? separator)
    {
        IncludeIndex = includeIndex;
        PrefixAll = prefixAll;
        PostfixAll = postfixAll;
        PrefixEach = prefixEach;
        PostfixEach = postfixEach;
        _separator = separator;
    }

    /// <summary>
    /// Specifies whether or not the message formatter should include message indexes.
    /// </summary>
    public bool IncludeIndex { get; }

    /// <summary>
    /// Specifies an optional message to add once before all validation messages.
    /// This message receives a single <see cref="Int32"/> parameter that defines the number of validation messages.
    /// </summary>
    public string? PrefixAll { get; }

    /// <summary>
    /// Specifies an optional message to add once after all validation messages.
    /// </summary>
    public string? PostfixAll { get; }

    /// <summary>
    /// Specifies an optional message to add before each validation message.
    /// </summary>
    public string? PrefixEach { get; }

    /// <summary>
    /// Specifies an optional message to add after each validation message.
    /// </summary>
    public string? PostfixEach { get; }

    /// <summary>
    /// Specifies a separator for validation messages. Equal to <see cref="Environment.NewLine"/> by default.
    /// </summary>
    public string Separator => _separator ?? Environment.NewLine;

    /// <summary>
    /// Returns a string representation of this <see cref="ValidationMessageFormatterArgs"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var headerText = $"{nameof( Separator )}: '{Separator}', {nameof( IncludeIndex )}: {IncludeIndex}";
        var prefixAllText = string.IsNullOrEmpty( PrefixAll ) ? string.Empty : $", {nameof( PrefixAll )}: '{PrefixAll}'";
        var postfixAllText = string.IsNullOrEmpty( PostfixAll ) ? string.Empty : $", {nameof( PostfixAll )}: '{PostfixAll}'";
        var prefixEachText = string.IsNullOrEmpty( PrefixEach ) ? string.Empty : $", {nameof( PrefixEach )}: '{PrefixEach}'";
        var postfixEachText = string.IsNullOrEmpty( PostfixEach ) ? string.Empty : $", {nameof( PostfixEach )}: '{PostfixEach}'";
        return $"{headerText}{prefixAllText}{postfixAllText}{prefixEachText}{postfixEachText}";
    }

    /// <summary>
    /// Creates a new <see cref="ValidationMessageFormatterArgs"/> with updates <see cref="IncludeIndex"/> value.
    /// </summary>
    /// <param name="value"><see cref="IncludeIndex"/> value to set.</param>
    /// <returns>New <see cref="ValidationMessageFormatterArgs"/> instance.</returns>
    [Pure]
    public ValidationMessageFormatterArgs SetIncludeIndex(bool value)
    {
        return new ValidationMessageFormatterArgs( value, PrefixAll, PostfixAll, PrefixEach, PostfixEach, _separator );
    }

    /// <summary>
    /// Creates a new <see cref="ValidationMessageFormatterArgs"/> with updates <see cref="PrefixAll"/> value.
    /// </summary>
    /// <param name="value"><see cref="PrefixAll"/> value to set.</param>
    /// <returns>New <see cref="ValidationMessageFormatterArgs"/> instance.</returns>
    [Pure]
    public ValidationMessageFormatterArgs SetPrefixAll(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, value, PostfixAll, PrefixEach, PostfixEach, _separator );
    }

    /// <summary>
    /// Creates a new <see cref="ValidationMessageFormatterArgs"/> with updates <see cref="PostfixAll"/> value.
    /// </summary>
    /// <param name="value"><see cref="PostfixAll"/> value to set.</param>
    /// <returns>New <see cref="ValidationMessageFormatterArgs"/> instance.</returns>
    [Pure]
    public ValidationMessageFormatterArgs SetPostfixAll(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, value, PrefixEach, PostfixEach, _separator );
    }

    /// <summary>
    /// Creates a new <see cref="ValidationMessageFormatterArgs"/> with updates <see cref="PrefixEach"/> value.
    /// </summary>
    /// <param name="value"><see cref="PrefixEach"/> value to set.</param>
    /// <returns>New <see cref="ValidationMessageFormatterArgs"/> instance.</returns>
    [Pure]
    public ValidationMessageFormatterArgs SetPrefixEach(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, PostfixAll, value, PostfixEach, _separator );
    }

    /// <summary>
    /// Creates a new <see cref="ValidationMessageFormatterArgs"/> with updates <see cref="PostfixEach"/> value.
    /// </summary>
    /// <param name="value"><see cref="PostfixEach"/> value to set.</param>
    /// <returns>New <see cref="ValidationMessageFormatterArgs"/> instance.</returns>
    [Pure]
    public ValidationMessageFormatterArgs SetPostfixEach(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, PostfixAll, PrefixEach, value, _separator );
    }

    /// <summary>
    /// Creates a new <see cref="ValidationMessageFormatterArgs"/> with updates <see cref="Separator"/> value.
    /// </summary>
    /// <param name="value"><see cref="Separator"/> value to set.</param>
    /// <returns>New <see cref="ValidationMessageFormatterArgs"/> instance.</returns>
    [Pure]
    public ValidationMessageFormatterArgs SetSeparator(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, PostfixAll, PrefixEach, PostfixEach, value );
    }
}
