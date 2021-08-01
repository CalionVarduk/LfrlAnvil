using System;

namespace LfrlSoft.NET.Core.Tests.Extensions.Enum
{
    public class EnumExtensionsOfEnumInt32Tests : GenericEnumExtensionsTests<TestEnumInt32> { }

    [Flags]
    public enum TestEnumInt32
    {
        A = 0,
        B = 1
    }
}
