﻿using System;

namespace LfrlSoft.NET.Core.Benchmarks.CL
{
    [AttributeUsage( AttributeTargets.Property )]
    public class CommandLineBenchmarkAttribute : Attribute
    {
        public CommandLineBenchmarkAttribute(Type benchmarkType)
        {
            BenchmarkType = benchmarkType;
        }

        public Type BenchmarkType { get; }
    }
}