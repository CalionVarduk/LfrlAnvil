using System;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    [Flags]
    public enum TestEnumUInt32 : uint
    {
        A = 0,
        B = 1,
        C = 2,
        D = 4,
        E = 8,
        F = 1U << 31
    }

    public class BitmaskOfEnumUnit32TestsData
    {
        public static TheoryData<uint, TestEnumUInt32> GetSanitizeData(IFixture fixture)
        {
            return new()
            {
                { 0U, TestEnumUInt32.A },
                { 1U, TestEnumUInt32.B },
                { 2U, TestEnumUInt32.C },
                { 4U, TestEnumUInt32.D },
                { 8U, TestEnumUInt32.E },
                { 16U, TestEnumUInt32.A },
                { 17U, TestEnumUInt32.B },
                { 18U, TestEnumUInt32.C },
                { 20U, TestEnumUInt32.D },
                { 24U, TestEnumUInt32.E },
                { 16U | (1U << 31), TestEnumUInt32.F },
                { 17U | (1U << 31), TestEnumUInt32.B | TestEnumUInt32.F },
                { 18U | (1U << 31), TestEnumUInt32.C | TestEnumUInt32.F },
                { 20U | (1U << 31), TestEnumUInt32.D | TestEnumUInt32.F },
                { 24U | (1U << 31), TestEnumUInt32.E | TestEnumUInt32.F }
            };
        }
    }
}
