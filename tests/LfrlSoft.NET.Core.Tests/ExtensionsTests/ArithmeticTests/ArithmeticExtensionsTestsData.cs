using System.Collections.Generic;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ExtensionsTests.ArithmeticTests
{
    public class ArithmeticExtensionsTestsData
    {
        public static TheoryData<float, float, float> GetEuclidModuloFloatData(IFixture fixture)
        {
            var result = new TheoryData<float, float, float>();
            foreach ( var (a, b, r) in GetEuclidModuloRealData() )
                result.Add( (float)a, (float)b, (float)r );

            return result;
        }

        public static TheoryData<double, double, double> GetEuclidModuloDoubleData(IFixture fixture)
        {
            var result = new TheoryData<double, double, double>();
            foreach ( var (a, b, r) in GetEuclidModuloRealData() )
                result.Add( (double)a, (double)b, (double)r );

            return result;
        }

        public static TheoryData<decimal, decimal, decimal> GetEuclidModuloDecimalData(IFixture fixture)
        {
            var result = new TheoryData<decimal, decimal, decimal>();
            foreach ( var (a, b, r) in GetEuclidModuloRealData() )
                result.Add( a, b, r );

            return result;
        }

        public static TheoryData<ulong, ulong, ulong> GetEuclidModuloUint64Data(IFixture fixture)
        {
            var result = new TheoryData<ulong, ulong, ulong>();
            foreach ( var (a, b, r) in GetEuclidModuloUnsignedIntData() )
                result.Add( a, b, r );

            return result;
        }

        public static TheoryData<long, long, long> GetEuclidModuloInt64Data(IFixture fixture)
        {
            var result = new TheoryData<long, long, long>();
            foreach ( var (a, b, r) in GetEuclidModuloSignedIntData() )
                result.Add( a, b, r );

            return result;
        }

        public static TheoryData<uint, uint, uint> GetEuclidModuloUint32Data(IFixture fixture)
        {
            var result = new TheoryData<uint, uint, uint>();
            foreach ( var (a, b, r) in GetEuclidModuloUnsignedIntData() )
                result.Add( a, b, r );

            return result;
        }

        public static TheoryData<int, int, int> GetEuclidModuloInt32Data(IFixture fixture)
        {
            var result = new TheoryData<int, int, int>();
            foreach ( var (a, b, r) in GetEuclidModuloSignedIntData() )
                result.Add( a, b, r );

            return result;
        }

        public static TheoryData<ushort, ushort, ushort> GetEuclidModuloUint16Data(IFixture fixture)
        {
            var result = new TheoryData<ushort, ushort, ushort>();
            foreach ( var (a, b, r) in GetEuclidModuloUnsignedIntData() )
                result.Add( (ushort)a, (ushort)b, (ushort)r );

            return result;
        }

        public static TheoryData<short, short, short> GetEuclidModuloInt16Data(IFixture fixture)
        {
            var result = new TheoryData<short, short, short>();
            foreach ( var (a, b, r) in GetEuclidModuloSignedIntData() )
                result.Add( (short)a, (short)b, (short)r );

            return result;
        }

        public static TheoryData<byte, byte, byte> GetEuclidModuloUint8Data(IFixture fixture)
        {
            var result = new TheoryData<byte, byte, byte>();
            foreach ( var (a, b, r) in GetEuclidModuloUnsignedIntData() )
                result.Add( (byte)a, (byte)b, (byte)r );

            return result;
        }

        public static TheoryData<sbyte, sbyte, sbyte> GetEuclidModuloInt8Data(IFixture fixture)
        {
            var result = new TheoryData<sbyte, sbyte, sbyte>();
            foreach ( var (a, b, r) in GetEuclidModuloSignedIntData() )
                result.Add( (sbyte)a, (sbyte)b, (sbyte)r );

            return result;
        }

        public static TheoryData<ulong, bool> GetIsEvenUint64Data(IFixture fixture)
        {
            var result = new TheoryData<ulong, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( x, r );

            return result;
        }

        public static TheoryData<ulong, bool> GetIsOddUint64Data(IFixture fixture)
        {
            var result = new TheoryData<ulong, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( x, ! r );

            return result;
        }

        public static TheoryData<long, bool> GetIsEvenInt64Data(IFixture fixture)
        {
            var result = new TheoryData<long, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( x, r );

            return result;
        }

        public static TheoryData<long, bool> GetIsOddInt64Data(IFixture fixture)
        {
            var result = new TheoryData<long, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( x, ! r );

            return result;
        }

        public static TheoryData<uint, bool> GetIsEvenUint32Data(IFixture fixture)
        {
            var result = new TheoryData<uint, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( x, r );

            return result;
        }

        public static TheoryData<uint, bool> GetIsOddUint32Data(IFixture fixture)
        {
            var result = new TheoryData<uint, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( x, ! r );

            return result;
        }

        public static TheoryData<int, bool> GetIsEvenInt32Data(IFixture fixture)
        {
            var result = new TheoryData<int, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( x, r );

            return result;
        }

        public static TheoryData<int, bool> GetIsOddInt32Data(IFixture fixture)
        {
            var result = new TheoryData<int, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( x, ! r );

            return result;
        }

        public static TheoryData<ushort, bool> GetIsEvenUint16Data(IFixture fixture)
        {
            var result = new TheoryData<ushort, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( (ushort)x, r );

            return result;
        }

        public static TheoryData<ushort, bool> GetIsOddUint16Data(IFixture fixture)
        {
            var result = new TheoryData<ushort, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( (ushort)x, ! r );

            return result;
        }

        public static TheoryData<short, bool> GetIsEvenInt16Data(IFixture fixture)
        {
            var result = new TheoryData<short, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( (short)x, r );

            return result;
        }

        public static TheoryData<short, bool> GetIsOddInt16Data(IFixture fixture)
        {
            var result = new TheoryData<short, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( (short)x, ! r );

            return result;
        }

        public static TheoryData<byte, bool> GetIsEvenUint8Data(IFixture fixture)
        {
            var result = new TheoryData<byte, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( (byte)x, r );

            return result;
        }

        public static TheoryData<byte, bool> GetIsOddUint8Data(IFixture fixture)
        {
            var result = new TheoryData<byte, bool>();
            foreach ( var (x, r) in GetIsEvenUnsignedIntData() )
                result.Add( (byte)x, ! r );

            return result;
        }

        public static TheoryData<sbyte, bool> GetIsEvenInt8Data(IFixture fixture)
        {
            var result = new TheoryData<sbyte, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( (sbyte)x, r );

            return result;
        }

        public static TheoryData<sbyte, bool> GetIsOddInt8Data(IFixture fixture)
        {
            var result = new TheoryData<sbyte, bool>();
            foreach ( var (x, r) in GetIsEvenSignedIntData() )
                result.Add( (sbyte)x, ! r );

            return result;
        }

        private static IEnumerable<(decimal A, decimal B, decimal Result)> GetEuclidModuloRealData()
        {
            yield return (-3.0m, 3.0m, 0.0m);
            yield return (-2.0m, 3.0m, 1.0m);
            yield return (-1.0m, 3.0m, 2.0m);
            yield return (0.0m, 3.0m, 0.0m);
            yield return (1.0m, 3.0m, 1.0m);
            yield return (2.0m, 3.0m, 2.0m);
            yield return (3.0m, 3.0m, 0.0m);
            yield return (4.0m, 3.0m, 1.0m);
            yield return (5.0m, 3.0m, 2.0m);
            yield return (6.0m, 3.0m, 0.0m);
            yield return (-3.25m, 3.0m, 2.75m);
            yield return (-2.25m, 3.0m, 0.75m);
            yield return (-1.25m, 3.0m, 1.75m);
            yield return (-0.25m, 3.0m, 2.75m);
            yield return (0.25m, 3.0m, 0.25m);
            yield return (1.25m, 3.0m, 1.25m);
            yield return (2.25m, 3.0m, 2.25m);
            yield return (3.25m, 3.0m, 0.25m);
            yield return (4.25m, 3.0m, 1.25m);
            yield return (5.25m, 3.0m, 2.25m);
            yield return (6.25m, 3.0m, 0.25m);
            yield return (-3.0m, 3.25m, 0.25m);
            yield return (-2.0m, 3.25m, 1.25m);
            yield return (-1.0m, 3.25m, 2.25m);
            yield return (0.0m, 3.25m, 0.0m);
            yield return (1.0m, 3.25m, 1.0m);
            yield return (2.0m, 3.25m, 2.0m);
            yield return (3.0m, 3.25m, 3.0m);
            yield return (4.0m, 3.25m, 0.75m);
            yield return (5.0m, 3.25m, 1.75m);
            yield return (6.0m, 3.25m, 2.75m);
            yield return (-3.5m, 3.25m, 3.0m);
            yield return (-2.5m, 3.25m, 0.75m);
            yield return (-1.5m, 3.25m, 1.75m);
            yield return (-0.5m, 3.25m, 2.75m);
            yield return (0.5m, 3.25m, 0.5m);
            yield return (1.5m, 3.25m, 1.5m);
            yield return (2.5m, 3.25m, 2.5m);
            yield return (3.5m, 3.25m, 0.25m);
            yield return (4.5m, 3.25m, 1.25m);
            yield return (5.5m, 3.25m, 2.25m);
            yield return (6.5m, 3.25m, 0.0m);
        }

        private static IEnumerable<(uint A, uint B, uint Result)> GetEuclidModuloUnsignedIntData()
        {
            yield return (0, 3, 0);
            yield return (1, 3, 1);
            yield return (2, 3, 2);
            yield return (3, 3, 0);
            yield return (4, 3, 1);
            yield return (5, 3, 2);
            yield return (6, 3, 0);
        }

        private static IEnumerable<(int A, int B, int Result)> GetEuclidModuloSignedIntData()
        {
            yield return (-3, 3, 0);
            yield return (-2, 3, 1);
            yield return (-1, 3, 2);
            yield return (0, 3, 0);
            yield return (1, 3, 1);
            yield return (2, 3, 2);
            yield return (3, 3, 0);
            yield return (4, 3, 1);
            yield return (5, 3, 2);
            yield return (6, 3, 0);
        }

        private static IEnumerable<(uint X, bool Result)> GetIsEvenUnsignedIntData()
        {
            yield return (0, true);
            yield return (1, false);
            yield return (2, true);
            yield return (3, false);
        }

        private static IEnumerable<(int X, bool Result)> GetIsEvenSignedIntData()
        {
            yield return (-3, false);
            yield return (-2, true);
            yield return (-1, false);
            yield return (0, true);
            yield return (1, false);
            yield return (2, true);
            yield return (3, false);
        }
    }
}
