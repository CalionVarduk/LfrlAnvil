using System;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests;

public abstract class GenericObjectExtensionsOfRefTypeTests<T> : GenericObjectExtensionsOfComparableTypeTests<T>
    where T : class, IComparable<T> { }
