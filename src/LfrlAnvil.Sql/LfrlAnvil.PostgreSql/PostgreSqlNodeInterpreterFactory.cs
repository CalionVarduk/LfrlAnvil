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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc cref="ISqlNodeInterpreterFactory" />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public class PostgreSqlNodeInterpreterFactory : ISqlNodeInterpreterFactory
{
    /// <summary>
    /// Creates a new <see cref="PostgreSqlNodeInterpreterFactory"/> instance.
    /// </summary>
    /// <param name="options"><see cref="PostgreSqlNodeInterpreterOptions"/> instance applied to created node interpreters.</param>
    protected internal PostgreSqlNodeInterpreterFactory(PostgreSqlNodeInterpreterOptions options)
    {
        Options = options;
        if ( Options.TypeDefinitions is null )
            Options = Options.SetTypeDefinitions( new PostgreSqlColumnTypeDefinitionProviderBuilder().Build() );
    }

    /// <summary>
    /// <see cref="PostgreSqlNodeInterpreterOptions"/> instance applied to created node interpreters.
    /// </summary>
    public PostgreSqlNodeInterpreterOptions Options { get; }

    /// <inheritdoc cref="ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext)" />
    [Pure]
    public virtual PostgreSqlNodeInterpreter Create(SqlNodeInterpreterContext context)
    {
        return new PostgreSqlNodeInterpreter( Options, context );
    }

    [Pure]
    SqlNodeInterpreter ISqlNodeInterpreterFactory.Create(SqlNodeInterpreterContext context)
    {
        return Create( context );
    }
}
