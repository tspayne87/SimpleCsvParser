using System;
using System.IO;
using System.Diagnostics;
using SimpleCsvParser.Streams;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using Parser.Performance;
using BenchmarkDotNet.Configs;

namespace SimpleCsvParser.Performance
{
    class Program
    {
        private static string file = "PackageAssets.csv";

        static void Main(string[] args)
        {
            IConfig config = null;
#if DEBUG
            config = new DebugInProcessConfig();
#endif
            BenchmarkRunner.Run<SimpleParserSuite>(config);
        }
    }
}
