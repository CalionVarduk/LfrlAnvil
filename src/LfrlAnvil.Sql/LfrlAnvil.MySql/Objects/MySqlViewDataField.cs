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

using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlViewDataField : SqlViewDataField
{
    internal MySqlViewDataField(MySqlView view, string name)
        : base( view, name ) { }

    /// <inheritdoc cref="SqlViewDataField.View" />
    public new MySqlView View => ReinterpretCast.To<MySqlView>( base.View );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
