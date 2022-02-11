using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.PropertyInfo
{
    public class PropertyInfoExtensionsTests : TestsBase
    {
        [Fact]
        public void GetBackingField_ShouldReturnNull_WhenPropertyIsPublic_AndExplicit_AndWritable()
        {
            var sut = TestClass.GetPublicExplicitWritableInfo();
            var result = sut.GetBackingField();
            result.Should().BeNull();
        }

        [Fact]
        public void GetBackingField_ShouldReturnNull_WhenPropertyIsPublic_AndExplicit_AndWriteOnly()
        {
            var sut = TestClass.GetPublicExplicitWriteOnlyInfo();
            var result = sut.GetBackingField();
            result.Should().BeNull();
        }

        [Fact]
        public void GetBackingField_ShouldReturnNull_WhenPropertyIsPublic_AndExplicit_AndReadOnly()
        {
            var sut = TestClass.GetPublicExplicitReadOnlyInfo();
            var result = sut.GetBackingField();
            result.Should().BeNull();
        }

        [Fact]
        public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPublic_AndAuto_AndWritable()
        {
            var sut = TestClass.GetPublicAutoWritableInfo();
            var result = sut.GetBackingField();
            result.Should().Match<FieldInfo>( r => MatchBackingField( sut, r ) );
        }

        [Fact]
        public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPublic_AndAuto_AndReadOnly()
        {
            var sut = TestClass.GetPublicAutoReadOnlyInfo();
            var result = sut.GetBackingField();
            result.Should().Match<FieldInfo>( r => MatchBackingField( sut, r ) );
        }

        [Fact]
        public void GetBackingField_ShouldReturnNull_WhenPropertyIsPrivate_AndExplicit_AndWritable()
        {
            var sut = TestClass.GetPrivateExplicitWritableInfo();
            var result = sut.GetBackingField();
            result.Should().BeNull();
        }

        [Fact]
        public void GetBackingField_ShouldReturnNull_WhenPropertyIsPrivate_AndExplicit_AndWriteOnly()
        {
            var sut = TestClass.GetPrivateExplicitWriteOnlyInfo();
            var result = sut.GetBackingField();
            result.Should().BeNull();
        }

        [Fact]
        public void GetBackingField_ShouldReturnNull_WhenPropertyIsPrivate_AndExplicit_AndReadOnly()
        {
            var sut = TestClass.GetPrivateExplicitReadOnlyInfo();
            var result = sut.GetBackingField();
            result.Should().BeNull();
        }

        [Fact]
        public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPrivate_AndAuto_AndWritable()
        {
            var sut = TestClass.GetPrivateAutoWritableInfo();
            var result = sut.GetBackingField();
            result.Should().Match<FieldInfo>( r => MatchBackingField( sut, r ) );
        }

        [Fact]
        public void GetBackingField_ShouldReturnFieldInfo_WhenPropertyIsPrivate_AndAuto_AndReadOnly()
        {
            var sut = TestClass.GetPrivateAutoReadOnlyInfo();
            var result = sut.GetBackingField();
            result.Should().Match<FieldInfo>( r => MatchBackingField( sut, r ) );
        }

        private static bool MatchBackingField(System.Reflection.PropertyInfo property, FieldInfo? info)
        {
            return info != null &&
                info.IsPrivate &&
                info.Name.Contains( property.Name ) &&
                Attribute.IsDefined( info, typeof( CompilerGeneratedAttribute ) );
        }
    }

    public class TestClass
    {
        public static System.Reflection.PropertyInfo GetPublicExplicitWritableInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PublicExplicitWritableProperty ),
                BindingFlags.Instance | BindingFlags.Public )!;
        }

        public static System.Reflection.PropertyInfo GetPublicExplicitWriteOnlyInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PublicExplicitWriteOnlyProperty ),
                BindingFlags.Instance | BindingFlags.Public )!;
        }

        public static System.Reflection.PropertyInfo GetPublicExplicitReadOnlyInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PublicExplicitReadOnlyProperty ),
                BindingFlags.Instance | BindingFlags.Public )!;
        }

        public static System.Reflection.PropertyInfo GetPublicAutoWritableInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PublicAutoWritableProperty ),
                BindingFlags.Instance | BindingFlags.Public )!;
        }

        public static System.Reflection.PropertyInfo GetPublicAutoReadOnlyInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PublicAutoReadOnlyProperty ),
                BindingFlags.Instance | BindingFlags.Public )!;
        }

        public static System.Reflection.PropertyInfo GetPrivateExplicitWritableInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PrivateExplicitWritableProperty ),
                BindingFlags.Instance | BindingFlags.NonPublic )!;
        }

        public static System.Reflection.PropertyInfo GetPrivateExplicitWriteOnlyInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PrivateExplicitWriteOnlyProperty ),
                BindingFlags.Instance | BindingFlags.NonPublic )!;
        }

        public static System.Reflection.PropertyInfo GetPrivateExplicitReadOnlyInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PrivateExplicitReadOnlyProperty ),
                BindingFlags.Instance | BindingFlags.NonPublic )!;
        }

        public static System.Reflection.PropertyInfo GetPrivateAutoWritableInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PrivateAutoWritableProperty ),
                BindingFlags.Instance | BindingFlags.NonPublic )!;
        }

        public static System.Reflection.PropertyInfo GetPrivateAutoReadOnlyInfo()
        {
            return typeof( TestClass ).GetProperty(
                nameof( PrivateAutoReadOnlyProperty ),
                BindingFlags.Instance | BindingFlags.NonPublic )!;
        }

        private int _explicitBackingField;

        public int PublicExplicitWritableProperty
        {
            get => _explicitBackingField;
            set => _explicitBackingField = value;
        }

        public int PublicExplicitWriteOnlyProperty
        {
            set => _explicitBackingField = value;
        }

        public int PublicExplicitReadOnlyProperty
        {
            get => _explicitBackingField;
        }

        public int PublicAutoWritableProperty { get; set; }
        public int PublicAutoReadOnlyProperty { get; }

        private int PrivateExplicitWritableProperty
        {
            get => _explicitBackingField;
            set => _explicitBackingField = value;
        }

        private int PrivateExplicitWriteOnlyProperty
        {
            set => _explicitBackingField = value;
        }

        private int PrivateExplicitReadOnlyProperty
        {
            get => _explicitBackingField;
        }

        private int PrivateAutoWritableProperty { get; set; }
        private int PrivateAutoReadOnlyProperty { get; }
    }
}
