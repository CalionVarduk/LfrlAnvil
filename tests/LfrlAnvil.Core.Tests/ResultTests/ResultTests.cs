// Copyright 2024 Łukasz Furlepa
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

using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ResultTests;

public class ResultTests : TestsBase
{
    [Fact]
    public void Valid_ShouldReturnResultWithoutException()
    {
        var sut = Result.Valid;
        sut.Exception.Should().BeNull();
    }

    [Fact]
    public void Error_ShouldCreateResultWithException()
    {
        var exception = new Exception( "foo" );
        var result = Result.Error( exception );
        result.Exception.Should().BeSameAs( exception );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenExceptionIsNull()
    {
        var sut = Result.Valid;
        var result = sut.ToString();
        result.Should().Be( "<VALID>" );
    }

    [Fact]
    public void ToString_ShouldReturnExceptionToString_WhenExceptionIsNotNull()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var result = sut.ToString();
        result.Should().Be( exception.ToString() );
    }

    [Fact]
    public void ThrowIfError_ShouldDoNothing_WhenExceptionIsNull()
    {
        var sut = Result.Valid;
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfError_ShouldThrow_WhenExceptionIsNotNull()
    {
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().Throw<Exception>().And.Should().BeSameAs( exception );
    }

    [Fact]
    public void Create_ShouldCreateGenericResultWithoutException()
    {
        var value = Fixture.Create<string>();
        var result = Result.Create( value );

        using ( new AssertionScope() )
        {
            result.Value.Should().BeSameAs( value );
            result.Exception.Should().BeNull();
        }
    }

    [Fact]
    public void Error_ShouldCreateGenericResultWithException()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var result = Result.Error( exception, value );

        using ( new AssertionScope() )
        {
            result.Value.Should().BeSameAs( value );
            result.Exception.Should().BeSameAs( exception );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = sut.ToString();

        result.Should()
            .Be(
                $"""
                 Value: {value}
                 <VALID>
                 """ );
    }

    [Fact]
    public void ToString_ShouldReturnExceptionToString_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var result = sut.ToString();

        result.Should()
            .Be(
                $"""
                 Value: {value}
                 {exception}
                 """ );
    }

    [Fact]
    public void ThrowIfError_ShouldDoNothing_WhenGenericExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfError_ShouldThrow_WhenGenericExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var action = Lambda.Of( () => sut.ThrowIfError() );
        action.Should().Throw<Exception>().And.Should().BeSameAs( exception );
    }

    [Fact]
    public void ResultConversionOperator_ShouldReturnValid_WhenExceptionIsNull()
    {
        var value = Fixture.Create<string>();
        var sut = Result.Create( value );
        var result = ( Result )sut;
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void ResultConversionOperator_ShouldReturnError_WhenExceptionIsNotNull()
    {
        var value = Fixture.Create<string>();
        var exception = new Exception( "foo" );
        var sut = Result.Error( exception, value );
        var result = ( Result )sut;
        result.Exception.Should().BeSameAs( exception );
    }
}
