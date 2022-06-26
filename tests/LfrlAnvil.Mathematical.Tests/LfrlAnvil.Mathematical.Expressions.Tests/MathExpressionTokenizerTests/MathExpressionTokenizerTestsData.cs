using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.Mathematical.Expressions.Internal;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.MathExpressionTokenizerTests;

public class MathExpressionTokenizerTestsData
{
    public static TheoryData<string> GetWhiteSpaceInputData(IFixture fixture)
    {
        return new TheoryData<string>
        {
            " ",
            "\t",
            "\r\n",
            "  \r\n \t  "
        };
    }

    public static TheoryData<string> GetStringOnlyData(IFixture fixture)
    {
        return new TheoryData<string>
        {
            "''",
            "''''",
            "'a'",
            "' a b '",
            "' '' a '' b '"
        };
    }

    public static TheoryData<string, string> GetStartsWithStringData(IFixture fixture)
    {
        return new TheoryData<string, string>
        {
            { "' lorem '''foo", "' lorem '''" },
            { "' lorem '' ", "' lorem '' " },
            { "'", "'" }
        };
    }

    public static TheoryData<string> GetNumberOnlyData(IFixture fixture)
    {
        return new TheoryData<string>
        {
            "0",
            "123",
            "1_234",
            "0.0",
            "1_234_567.890123",
            "5E123",
            "5e123",
            "1_234E+567",
            "1_234e+567",
            "1_234E-567",
            "1_234e-567",
            "9_876.54321E+900",
            "9_876.54321e+900",
            "9_876.54321E-900",
            "9_876.54321e-900"
        };
    }

    public static TheoryData<string, string> GetStartsWithNumberData(IFixture fixture)
    {
        return new TheoryData<string, string>
        {
            { "1_234.567E+89foo", "1_234.567E+89" },
            { "1234__567", "1234" },
            { "1234_.567", "1234" },
            { "1234_.E567", "1234" },
            { "1234_.e567", "1234" },
            { "1234_foo", "1234" },
            { "1234_", "1234_" },
            { "1234..567", "1234" },
            { "1234.E567", "1234" },
            { "1234.e567", "1234" },
            { "1234.foo", "1234" },
            { "1234.", "1234." },
            { "1234EE567", "1234" },
            { "1234ee567", "1234" },
            { "1234Ee567", "1234" },
            { "1234eE567", "1234" },
            { "1234Efoo", "1234" },
            { "1234efoo", "1234" },
            { "1234E", "1234E" },
            { "1234e", "1234e" },
            { "1234+", "1234" },
            { "1234-", "1234" },
            { "1234E+foo", "1234" },
            { "1234E-foo", "1234" },
            { "1234E++foo", "1234" },
            { "1234E+-foo", "1234" },
            { "1234E--foo", "1234" },
            { "1234E-+foo", "1234" },
            { "1234E++", "1234" },
            { "1234E+-", "1234" },
            { "1234E--", "1234" },
            { "1234E-+", "1234" }
        };
    }

    public static TheoryData<string> GetBooleanOnlyData(IFixture fixture)
    {
        return new TheoryData<string>
        {
            "true",
            "TRUE",
            "True",
            "false",
            "FALSE",
            "False"
        };
    }

    public static TheoryData<string, string> GetStartsWithBooleanData(IFixture fixture)
    {
        return new TheoryData<string, string>
        {
            { "true ", "true" },
            { "false ", "false" },
            { "true(", "true" },
            { "false(", "false" },
            { "true)", "true" },
            { "false)", "false" },
            { "true;", "true" },
            { "false;", "false" },
            { "true'", "true" },
            { "false'", "false" }
        };
    }

    public static TheoryData<string> GetArgumentOnlyData(IFixture fixture)
    {
        return new TheoryData<string>
        {
            "foo",
            "+",
            "-",
            "++",
            "+-"
        };
    }

    public static TheoryData<string, string> GetStartsWithArgumentData(IFixture fixture)
    {
        return new TheoryData<string, string>
        {
            { "foo ", "foo" },
            { "foo(", "foo" },
            { "foo)", "foo" },
            { "foo;", "foo" },
            { "foo'", "foo" }
        };
    }

    public static TheoryData<string, IEnumerable<string>, IEnumerable<Token>> GetComplexInputData(IFixture fixture)
    {
        var noTokens = Array.Empty<string>();
        return new TheoryData<string, IEnumerable<string>, IEnumerable<Token>>
        {
            {
                " x+y ",
                new[] { "x+y" },
                new[]
                {
                    new Token( IntermediateTokenType.TokenSet, "x+y" )
                }
            },
            {
                " foo+ ",
                noTokens,
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "foo" ),
                    new Token( IntermediateTokenType.Argument, "+" )
                }
            },
            {
                " foo+ ",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "foo" ),
                    new Token( IntermediateTokenType.TokenSet, "+" )
                }
            },
            {
                "1+2-3",
                new[] { "+", "-" },
                new[]
                {
                    new Token( IntermediateTokenType.NumberConstant, "1" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.NumberConstant, "2" ),
                    new Token( IntermediateTokenType.TokenSet, "-" ),
                    new Token( IntermediateTokenType.NumberConstant, "3" )
                }
            },
            {
                "a===b",
                new[] { "==" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "==" ),
                    new Token( IntermediateTokenType.Argument, "=" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a===b",
                new[] { "=", "==" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "==" ),
                    new Token( IntermediateTokenType.TokenSet, "=" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a===b",
                new[] { "==", "===" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "===" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a===b",
                noTokens,
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.Argument, "===" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a+==b",
                new[] { "==" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.Argument, "+" ),
                    new Token( IntermediateTokenType.TokenSet, "==" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a-+===b",
                new[] { "==" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.Argument, "-+" ),
                    new Token( IntermediateTokenType.TokenSet, "==" ),
                    new Token( IntermediateTokenType.Argument, "=" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a+b",
                new[] { "a" },
                new[]
                {
                    new Token( IntermediateTokenType.TokenSet, "a" ),
                    new Token( IntermediateTokenType.Argument, "+" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a+b",
                new[] { "a", "+" },
                new[]
                {
                    new Token( IntermediateTokenType.TokenSet, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a.b",
                noTokens,
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a..b",
                noTokens,
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a..b",
                new[] { ".." },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, ".." ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a.+b",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a+.b",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a,b",
                noTokens,
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a,,b",
                noTokens,
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a,,b",
                new[] { ",," },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, ",," ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a,+b",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "a+,b",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.Argument, "b" )
                }
            },
            {
                "Tuue|FuLSe",
                new[] { "|" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "Tuue" ),
                    new Token( IntermediateTokenType.TokenSet, "|" ),
                    new Token( IntermediateTokenType.Argument, "FuLSe" )
                }
            },
            {
                "_a_+_b_",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "_a_" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.Argument, "_b_" )
                }
            },
            {
                "_0+_1",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "_0" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.Argument, "_1" )
                }
            },
            {
                "1_+2_ ",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.NumberConstant, "1" ),
                    new Token( IntermediateTokenType.Argument, "_" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.NumberConstant, "2" ),
                    new Token( IntermediateTokenType.Argument, "_" )
                }
            },
            {
                "1_+2_",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.NumberConstant, "1" ),
                    new Token( IntermediateTokenType.Argument, "_" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.NumberConstant, "2_" )
                }
            },
            {
                "x+1.2.3",
                new[] { "+" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "x" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.NumberConstant, "1.2" ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.NumberConstant, "3" )
                }
            },
            {
                " x- _y +-z *( v mod w) ",
                new[] { "-", "+", "*", "mod" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "x" ),
                    new Token( IntermediateTokenType.TokenSet, "-" ),
                    new Token( IntermediateTokenType.Argument, "_y" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.TokenSet, "-" ),
                    new Token( IntermediateTokenType.Argument, "z" ),
                    new Token( IntermediateTokenType.TokenSet, "*" ),
                    new Token( IntermediateTokenType.OpenParenthesis, "(" ),
                    new Token( IntermediateTokenType.Argument, "v" ),
                    new Token( IntermediateTokenType.TokenSet, "mod" ),
                    new Token( IntermediateTokenType.Argument, "w" ),
                    new Token( IntermediateTokenType.CloseParenthesis, ")" )
                }
            },
            {
                "a+b*c/d mod e.calc() ++(f-g--h*func(+mixed i/j,f,g*h)/(1_234.567-'foo'))",
                new[] { "+", "*", "/", "mod", "++", "-", "--", "func", "+mixed" },
                new[]
                {
                    new Token( IntermediateTokenType.Argument, "a" ),
                    new Token( IntermediateTokenType.TokenSet, "+" ),
                    new Token( IntermediateTokenType.Argument, "b" ),
                    new Token( IntermediateTokenType.TokenSet, "*" ),
                    new Token( IntermediateTokenType.Argument, "c" ),
                    new Token( IntermediateTokenType.TokenSet, "/" ),
                    new Token( IntermediateTokenType.Argument, "d" ),
                    new Token( IntermediateTokenType.TokenSet, "mod" ),
                    new Token( IntermediateTokenType.Argument, "e" ),
                    new Token( IntermediateTokenType.MemberAccess, "." ),
                    new Token( IntermediateTokenType.Argument, "calc" ),
                    new Token( IntermediateTokenType.OpenParenthesis, "(" ),
                    new Token( IntermediateTokenType.CloseParenthesis, ")" ),
                    new Token( IntermediateTokenType.TokenSet, "++" ),
                    new Token( IntermediateTokenType.OpenParenthesis, "(" ),
                    new Token( IntermediateTokenType.Argument, "f" ),
                    new Token( IntermediateTokenType.TokenSet, "-" ),
                    new Token( IntermediateTokenType.Argument, "g" ),
                    new Token( IntermediateTokenType.TokenSet, "--" ),
                    new Token( IntermediateTokenType.Argument, "h" ),
                    new Token( IntermediateTokenType.TokenSet, "*" ),
                    new Token( IntermediateTokenType.TokenSet, "func" ),
                    new Token( IntermediateTokenType.OpenParenthesis, "(" ),
                    new Token( IntermediateTokenType.TokenSet, "+mixed" ),
                    new Token( IntermediateTokenType.Argument, "i" ),
                    new Token( IntermediateTokenType.TokenSet, "/" ),
                    new Token( IntermediateTokenType.Argument, "j" ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.Argument, "f" ),
                    new Token( IntermediateTokenType.FunctionParameterSeparator, "," ),
                    new Token( IntermediateTokenType.Argument, "g" ),
                    new Token( IntermediateTokenType.TokenSet, "*" ),
                    new Token( IntermediateTokenType.Argument, "h" ),
                    new Token( IntermediateTokenType.CloseParenthesis, ")" ),
                    new Token( IntermediateTokenType.TokenSet, "/" ),
                    new Token( IntermediateTokenType.OpenParenthesis, "(" ),
                    new Token( IntermediateTokenType.NumberConstant, "1_234.567" ),
                    new Token( IntermediateTokenType.TokenSet, "-" ),
                    new Token( IntermediateTokenType.StringConstant, "'foo'" ),
                    new Token( IntermediateTokenType.CloseParenthesis, ")" ),
                    new Token( IntermediateTokenType.CloseParenthesis, ")" )
                }
            }
        };
    }

    public sealed class Token
    {
        private readonly IntermediateToken _token;

        internal Token(IntermediateTokenType type, string symbol)
        {
            var s = StringSlice.Create( symbol );
            _token = type switch
            {
                IntermediateTokenType.OpenParenthesis => IntermediateToken.CreateOpenParenthesis( s ),
                IntermediateTokenType.CloseParenthesis => IntermediateToken.CreateCloseParenthesis( s ),
                IntermediateTokenType.FunctionParameterSeparator => IntermediateToken.CreateFunctionParameterSeparator( s ),
                IntermediateTokenType.InlineFunctionSeparator => IntermediateToken.CreateInlineFunctionSeparator( s ),
                IntermediateTokenType.MemberAccess => IntermediateToken.CreateMemberAccess( s ),
                IntermediateTokenType.StringConstant => IntermediateToken.CreateStringConstant( s ),
                IntermediateTokenType.NumberConstant => IntermediateToken.CreateNumberConstant( s ),
                IntermediateTokenType.BooleanConstant => IntermediateToken.CreateBooleanConstant( s ),
                IntermediateTokenType.Argument => IntermediateToken.CreateArgument( s ),
                _ => IntermediateToken.CreateTokenSet( s, new MathExpressionTokenSet( null, null, null ) )
            };
        }

        internal IntermediateToken GetToken(IReadOnlyDictionary<StringSlice, MathExpressionTokenSet> tokenSets)
        {
            return _token.Type == IntermediateTokenType.TokenSet
                ? IntermediateToken.CreateTokenSet( _token.Symbol, tokenSets[_token.Symbol] )
                : _token;
        }

        public override string ToString()
        {
            return _token.ToString();
        }
    }
}
