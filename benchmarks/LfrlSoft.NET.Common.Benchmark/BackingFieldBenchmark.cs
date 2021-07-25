using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace LfrlSoft.NET.Common.Benchmarks
{
    [MemoryDiagnoser]
    public class BackingFieldBenchmark
    {
        private class Test
        {
            public int Field1;
            public int Field2;
            private int Field3;
            public int Field4;
            private int Field5;
            public int Field6;
            public int Field7;
            private int Field8;
            public int Field9;
            public int Field10;
            private int Field11;
            private int Field12;
            public int Field13;
            private int Field14;
            public int Field15;
            private int Field16;
            public int Field17;
            private int Field18;
            public int Field19;
            public int Field20;

            public int Property1 { get; }
            private int Property2 { get; set; }
            public int Property3 { get; }
            public int Property4 { get; }
            public int Property5 { get; }
            public int Property6 { get; set; }
            private int Property7 { get; }
            private int Property8 { get; set; }
            private int Property9 { get; }
            public int Property10 { get; }
            public int Property11 { get; set; }
            private int Property12 { get; }
            public int Property13 { get; set; }
            private int Property14 { get; }
            public int Property15 { get; set; }
            public int Property16 { get; set; }
            public int Property17 { get; set; }
            private int Property18 { get; }
            private int Property19 { get; }
            public int Property20 { get; set; }

            public void Method1() { }
            public void Method2() { }
            public void Method3() { }
            private void Method4() { }
            private void Method5() { }
            public void Method6() { }
            public void Method7() { }
            private void Method8() { }
            public void Method9() { }
            private void Method10() { }
            public void Method11() { }
            private void Method12() { }
            public void Method13() { }
            public void Method14() { }
            public void Method15() { }
            private void Method16() { }
            public void Method17() { }
            public void Method18() { }
            private void Method19() { }
            public void Method20() { }
        }

        public readonly string Name = "<Property1>k__BackingField";

        [Benchmark]
        public bool GetFields()
        {
            var type = typeof( Test );

            var fields = type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic );
            var result = fields.FirstOrDefault( f => f.Name == Name );

            return result != null;
        }

        [Benchmark]
        public bool FindMembers()
        {
            var type = typeof( Test );

            var members = type.FindMembers( MemberTypes.Field, BindingFlags.Instance | BindingFlags.NonPublic, Type.FilterName, Name );
            var result = (FieldInfo?) members.FirstOrDefault();

            return result != null;
        }

        [Benchmark]
        public bool GetField()
        {
            var type = typeof( Test );

            var result = type.GetField( Name, BindingFlags.Instance | BindingFlags.NonPublic );

            return result != null;
        }
    }
}
