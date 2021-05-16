using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LfrlSoft.NET.Common.Benchmarks.CL
{
    public class BenchmarkLocator
    {
        public IEnumerable<Type> LocateTypes(CommandLineBenchmarkOptions options)
        {
            if ( options is null )
                return Enumerable.Empty<Type>();

            var benchmarkProperties = FindBenchmarkProperties();

            var types = benchmarkProperties
                .Where( x => (( bool )x.Property.GetValue( options )) == true )
                .Select( x => x.BenchmarkType )
                .ToList();

            return types;
        }

        private static IEnumerable<(PropertyInfo Property, Type BenchmarkType)> FindBenchmarkProperties()
        {
            var result = typeof( CommandLineBenchmarkOptions )
                .GetProperties( BindingFlags.Instance | BindingFlags.Public )
                .Where( p => p.PropertyType == typeof( bool ) )
                .Select( p => (Property: p, Attribute: ( CommandLineBenchmarkAttribute )Attribute.GetCustomAttribute( p, typeof( CommandLineBenchmarkAttribute ) )) )
                .Where( x => !(x.Attribute is null) )
                .Select( x => (Property: x.Property, BenchmarkType: x.Attribute.BenchmarkType) )
                .ToList();

            return result;
        }
    }
}
