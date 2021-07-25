using System;

namespace LfrlSoft.NET.Core.Tests.Extensions.Enum
{
    [Flags]
    public enum TestEnumInt32
    {
        A = 0,
        B = 1
    }

    public class EnumInt32 : EnumExtensionsTests<TestEnumInt32> { }
}
