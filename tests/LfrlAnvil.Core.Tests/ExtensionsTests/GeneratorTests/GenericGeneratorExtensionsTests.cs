using System;
using System.Linq;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.GeneratorTests
{
    public abstract class GenericGeneratorExtensionsTests<T> : TestsBase
    {
        [Fact]
        public void ToEnumerable_ShouldReturnResultThatYieldsUntilGeneratorFailsToGenerateNextValue()
        {
            var expected = Fixture.CreateDistinctCollection<T>( 10 ).ToArray();
            var generator = CreateGeneratorMock( expected );

            var result = generator.ToEnumerable().ToArray();

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void ToEnumerable_ShouldReturnEmptyResult_WhenGeneratorFailsToGenerateFirstValue()
        {
            var generator = CreateGeneratorMock( Array.Empty<T>() );
            var result = generator.ToEnumerable().ToArray();
            result.Should().BeEmpty();
        }

        [Fact]
        public void IGeneratorToEnumerable_ShouldReturnResultThatYieldsUntilGeneratorFailsToGenerateNextValue()
        {
            var expected = Fixture.CreateDistinctCollection<T>( 10 ).ToArray();
            IGenerator generator = CreateGeneratorMock( expected );

            var result = generator.ToEnumerable().Cast<T>().ToArray();

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void IGeneratorToEnumerable_ShouldReturnEmptyResult_WhenGeneratorFailsToGenerateFirstValue()
        {
            IGenerator generator = CreateGeneratorMock( Array.Empty<T>() );
            var result = generator.ToEnumerable().Cast<T>().ToArray();
            result.Should().BeEmpty();
        }

        private static IGenerator<T> CreateGeneratorMock(T[] values)
        {
            bool Failure(CallInfo c)
            {
                c[0] = default( T );
                return false;
            }

            Func<CallInfo, bool> SuccessFactory(T v)
            {
                return c =>
                {
                    c[0] = v;
                    return true;
                };
            }

            var mock = Substitute.For<IGenerator<T>>();

            if ( values.Length == 0 )
            {
                mock.TryGenerate( out Arg.Any<T?>() ).Returns( Failure );
                mock.TryGenerate( out Arg.Any<object?>() ).Returns( Failure );
                return mock;
            }

            mock.TryGenerate( out Arg.Any<T?>() )
                .Returns(
                    SuccessFactory( values[0] ),
                    values.Skip( 1 ).Select( SuccessFactory ).Append( Failure ).ToArray() );

            mock.TryGenerate( out Arg.Any<object?>() )
                .Returns(
                    SuccessFactory( values[0] ),
                    values.Skip( 1 ).Select( SuccessFactory ).Append( Failure ).ToArray() );

            return mock;
        }
    }
}
