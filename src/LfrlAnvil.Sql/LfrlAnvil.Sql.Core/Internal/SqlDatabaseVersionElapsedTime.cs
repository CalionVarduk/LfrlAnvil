using System;

namespace LfrlAnvil.Sql.Internal;

internal readonly record struct SqlDatabaseVersionElapsedTime(int Ordinal, TimeSpan ElapsedTime);
