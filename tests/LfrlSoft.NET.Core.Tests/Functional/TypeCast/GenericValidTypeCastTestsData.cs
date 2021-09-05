﻿using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public class GenericValidTypeCastTestsData<TSource, TDestination>
        where TSource : TDestination
    {
        public static TheoryData<object?, object?, bool> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<TSource>( 2 );

            return new TheoryData<object?, object?, bool>
            {
                { _1, _1, true },
                { _1, _2, false },
                { _1, null, false },
                { null, _1, false },
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
