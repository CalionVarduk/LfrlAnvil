﻿// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Exceptions;

/// <summary>
/// Contains helper methods for generating exception messages.
/// </summary>
public static class ExceptionResources
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public const string DividedByZero = "Attempted to divide by zero.";
    public const string SequenceContainsNoElements = "Sequence contains no elements.";
    public const string NonStaticMethodRequiresTarget = "Non-static method requires a target.";
    public const string AssumedCodeToBeUnreachable = "Assumed the code to be unreachable.";
    public const string FixedSizeCollection = "Collection was of a fixed size.";
    public const string InvalidType = "Invalid type.";

    internal const string FailedToGenerateNextValue = "Failed to generate next value.";
    internal const string ExpectedIndexToBeZero = "Expected index to be equal to 0.";
    internal const string LazyDisposableCannotAssign = "Lazy disposable cannot assign an inner disposable.";
    internal const string ChainHasAlreadyBeenExtended = "Chain has already been extended.";
    internal const string MeasurableHasAlreadyBeenInvoked = "Measurable has already been invoked.";
    internal const string TaskRegistryIsDisposed = "Task registry is disposed.";

    internal const string ChainCannotBeExtendedBecauseItIsAttachedToAnotherChain =
        "Chain cannot be extended because it is attached to another chain.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedNull<T>(T value, string paramName)
    {
        return $"Assumed {paramName} to be null but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedNotNull(string paramName)
    {
        return $"Assumed {paramName} to not be null.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedDefinedEnum<T>(T value, Type enumType, string paramName)
    {
        return $"Assumed {paramName} to be defined in {enumType.GetDebugString()} enum type but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedEqualTo<T>(T value, T expectedValue, string paramName)
    {
        return $"Assumed {paramName} to be equal to {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedNotEqualTo<T>(T value, string paramName)
    {
        return $"Assumed {paramName} to not be equal to {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedGreaterThan<T>(T value, T expectedValue, string paramName)
    {
        return $"Assumed {paramName} to be greater than {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedGreaterThanOrEqualTo<T>(T value, T expectedValue, string paramName)
    {
        return $"Assumed {paramName} to be greater than or equal to {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedLessThan<T>(T value, T expectedValue, string paramName)
    {
        return $"Assumed {paramName} to be less than {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedLessThanOrEqualTo<T>(T value, T expectedValue, string paramName)
    {
        return $"Assumed {paramName} to be less than or equal to {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedInRange<T>(T value, T min, T max, string paramName)
    {
        return $"Assumed {paramName} to be in [{min}, {max}] range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedNotInRange<T>(T value, T min, T max, string paramName)
    {
        return $"Assumed {paramName} to not be in [{min}, {max}] range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedInExclusiveRange<T>(T value, T min, T max, string paramName)
    {
        return $"Assumed {paramName} to be in ({min}, {max}) range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedNotInExclusiveRange<T>(T value, T min, T max, string paramName)
    {
        return $"Assumed {paramName} to not be in ({min}, {max}) range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedEmpty(string paramName)
    {
        return $"Assumed {paramName} to be empty.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedNotEmpty(string paramName)
    {
        return $"Assumed {paramName} to not be empty.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedToContainAtLeast(int count, string paramName)
    {
        return $"Assumed {paramName} to contain at least {count} elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedToContainAtMost(int count, string paramName)
    {
        return $"Assumed {paramName} to contain at most {count} elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedToContainInRange(int minCount, int maxCount, string paramName)
    {
        return $"Assumed {paramName} element count to be in [{minCount}, {maxCount}] range.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedToContainExactly(int count, string paramName)
    {
        return $"Assumed {paramName} to contain exactly {count} elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedInstanceOfType(Type type, Type? actualType, string paramName)
    {
        return $"Assumed {paramName} to be an instance of type {type.GetDebugString()} but found {actualType?.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedToBeTrue(string description)
    {
        return $"Assumed {description} to be true.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string AssumedToBeFalse(string description)
    {
        return $"Assumed {description} to be false.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotNull<T>(T value, string paramName)
    {
        return $"Expected {paramName} to be null but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedDefault<T>(T value, string paramName)
    {
        return $"Expected {paramName} to be default but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotDefault(string paramName)
    {
        return $"Expected {paramName} to not be default.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedOfType(Type type, Type actualType, string paramName)
    {
        return $"Expected {paramName} to be of type {type.GetDebugString()} but found {actualType.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotOfType(Type type, string paramName)
    {
        return $"Expected {paramName} to not be of type {type.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedInstanceOfType(Type type, Type actualType, string paramName)
    {
        return $"Expected {paramName} to be an instance of type {type.GetDebugString()} but found {actualType.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotInstanceOfType(Type type, string paramName)
    {
        return $"Expected {paramName} to not be an instance of type {type.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedDefinedEnum<T>(T value, Type enumType, string paramName)
    {
        return $"Expected {paramName} to be defined in {enumType.GetDebugString()} enum type but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedEqualTo<T>(T value, T expectedValue, string paramName)
    {
        return $"Expected {paramName} to be equal to {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotEqualTo<T>(T value, string paramName)
    {
        return $"Expected {paramName} to not be equal to {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedRefEqualTo(string paramName)
    {
        return $"Expected {paramName} to be a reference to the provided object.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotRefEqualTo(string paramName)
    {
        return $"Expected {paramName} to not be a reference to the provided object.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedGreaterThan<T>(T value, T expectedValue, string paramName)
    {
        return $"Expected {paramName} to be greater than {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedGreaterThanOrEqualTo<T>(T value, T expectedValue, string paramName)
    {
        return $"Expected {paramName} to be greater than or equal to {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedLessThan<T>(T value, T expectedValue, string paramName)
    {
        return $"Expected {paramName} to be less than {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedLessThanOrEqualTo<T>(T value, T expectedValue, string paramName)
    {
        return $"Expected {paramName} to be less than or equal to {expectedValue} but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedInRange<T>(T value, T min, T max, string paramName)
    {
        return $"Expected {paramName} to be in [{min}, {max}] range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotInRange<T>(T value, T min, T max, string paramName)
    {
        return $"Expected {paramName} to not be in [{min}, {max}] range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedInExclusiveRange<T>(T value, T min, T max, string paramName)
    {
        return $"Expected {paramName} to be in ({min}, {max}) range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotInExclusiveRange<T>(T value, T min, T max, string paramName)
    {
        return $"Expected {paramName} to not be in ({min}, {max}) range but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedEmpty(string paramName)
    {
        return $"Expected {paramName} to be empty.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedEmpty(string value, string paramName)
    {
        return $"Expected {paramName} to be empty but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotEmpty(string paramName)
    {
        return $"Expected {paramName} to not be empty.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNullOrEmpty(string paramName)
    {
        return $"Expected {paramName} to be null or empty.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNullOrEmpty(string value, string paramName)
    {
        return $"Expected {paramName} to be null or empty but found {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotNullOrEmpty(string paramName)
    {
        return $"Expected {paramName} to not be null or empty.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedNotNullOrWhiteSpace(string paramName)
    {
        return $"Expected {paramName} to not be null or white space.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToContainNull(string paramName)
    {
        return $"Expected {paramName} to contain a null element.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToNotContainNull(string paramName)
    {
        return $"Expected {paramName} to not contain any null elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToContainAtLeast(int count, string paramName)
    {
        return $"Expected {paramName} to contain at least {count} elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToContainAtMost(int count, string paramName)
    {
        return $"Expected {paramName} to contain at most {count} elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToContainInRange(int minCount, int maxCount, string paramName)
    {
        return $"Expected {paramName} element count to be in [{minCount}, {maxCount}] range.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToContainExactly(int count, string paramName)
    {
        return $"Expected {paramName} to contain exactly {count} elements.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToContain<T>(T value, string paramName)
    {
        return $"Expected {paramName} to contain an element equal to {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToNotContain<T>(T value, string paramName)
    {
        return $"Expected {paramName} to not contain an element equal to {value}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedAnyToPassThePredicate(string paramName)
    {
        return $"Expected any {paramName} element to pass the predicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedAllToPassThePredicate(string paramName)
    {
        return $"Expected all {paramName} elements to pass the predicate.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedOrdered(string paramName)
    {
        return $"Expected {paramName} to be ordered.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToBeTrue(string description)
    {
        return $"Expected {description} to be true.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExpectedToBeFalse(string description)
    {
        return $"Expected {description} to be false.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingEnumFlagsAttribute<T>()
    {
        return $"Enum type {typeof( T ).GetDebugString()} doesn't have the {nameof( FlagsAttribute )} attribute.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingEnumZeroValueMember<T>()
    {
        return $"Enum type {typeof( T ).GetDebugString()} doesn't have the 0-value member.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToCreateConverter<T>(string converterName)
    {
        return $"Failed to create {converterName} converter for type {typeof( T ).GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MinCannotBeGreaterThanMax<T>(T min, T max, string minName, string maxName)
    {
        return $"{minName} ({min}) cannot be greater than {maxName} ({max}).";
    }
}
