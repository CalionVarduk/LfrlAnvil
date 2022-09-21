using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

public readonly struct ValidationMessageFormatterArgs
{
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

    public bool IncludeIndex { get; }
    public string? PrefixAll { get; }
    public string? PostfixAll { get; }
    public string? PrefixEach { get; }
    public string? PostfixEach { get; }
    public string Separator => _separator ?? Environment.NewLine;

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

    [Pure]
    public ValidationMessageFormatterArgs SetIncludeIndex(bool value)
    {
        return new ValidationMessageFormatterArgs( value, PrefixAll, PostfixAll, PrefixEach, PostfixEach, _separator );
    }

    [Pure]
    public ValidationMessageFormatterArgs SetPrefixAll(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, value, PostfixAll, PrefixEach, PostfixEach, _separator );
    }

    [Pure]
    public ValidationMessageFormatterArgs SetPostfixAll(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, value, PrefixEach, PostfixEach, _separator );
    }

    [Pure]
    public ValidationMessageFormatterArgs SetPrefixEach(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, PostfixAll, value, PostfixEach, _separator );
    }

    [Pure]
    public ValidationMessageFormatterArgs SetPostfixEach(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, PostfixAll, PrefixEach, value, _separator );
    }

    [Pure]
    public ValidationMessageFormatterArgs SetSeparator(string? value)
    {
        return new ValidationMessageFormatterArgs( IncludeIndex, PrefixAll, PostfixAll, PrefixEach, PostfixEach, value );
    }
}
