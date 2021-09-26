﻿using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    [TestClass( typeof( BitmaskOfEnumUnit32TestsData ) )]
    public class BitmaskOfEnumUInt32Tests : GenericBitmaskTests<TestEnumUInt32>
    {
        [Fact]
        public void BitCount_ShouldBeCorrect()
        {
            BitCount_ShouldBeCorrect_Impl( 32 );
        }

        [Fact]
        public void BaseType_ShouldBeCorrect()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( uint ) );
        }

        [Theory]
        [MethodData( nameof( BitmaskOfEnumUnit32TestsData.GetSanitizeData ) )]
        public void Sanitize_ShouldReturnCorrectResult(uint value, TestEnumUInt32 expected)
        {
            Sanitize_ShouldReturnCorrectResult_Impl( (TestEnumUInt32)value, expected );
        }
    }
}
