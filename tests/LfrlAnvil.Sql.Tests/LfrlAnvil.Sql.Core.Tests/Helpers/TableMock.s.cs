using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class TableMock
{
    [Pure]
    public static ISqlTable Create(string name, bool areColumnsNullable = false, params string[] columnNames)
    {
        var table = Substitute.For<ISqlTable>();
        table.FullName.Returns( name );
        var columns = new ColumnsMock( table, areColumnsNullable, columnNames );
        table.Columns.Returns( columns );
        return table;
    }

    private sealed class ColumnsMock : ISqlColumnCollection
    {
        private readonly ISqlColumn[] _columns;

        public ColumnsMock(ISqlTable table, bool nullable, string[] columnNames)
        {
            Table = table;
            var typeMock = Substitute.For<ISqlColumnTypeDefinition>();
            typeMock.RuntimeType.Returns( typeof( int ) );

            _columns = new ISqlColumn[columnNames.Length];
            for ( var i = 0; i < _columns.Length; ++i )
            {
                var column = Substitute.For<ISqlColumn>();
                column.IsNullable.Returns( nullable );
                column.TypeDefinition.Returns( typeMock );
                column.Name.Returns( columnNames[i] );
                _columns[i] = column;
            }
        }

        public IEnumerator<ISqlColumn> GetEnumerator()
        {
            return _columns.AsEnumerable().GetEnumerator();
        }

        public int Count => _columns.Length;
        public ISqlTable Table { get; }

        public bool Contains(string name)
        {
            return Array.FindIndex( _columns, c => c.Name == name ) != -1;
        }

        public ISqlColumn Get(string name)
        {
            return Array.Find( _columns, c => c.Name == name )!;
        }

        public bool TryGet(string name, [MaybeNullWhen( false )] out ISqlColumn result)
        {
            result = Get( name );
            return result is not null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
