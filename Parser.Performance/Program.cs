using System;
using System.IO;
using System.Diagnostics;
using SimpleCsvParser.Streams;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using Parser.Performance;
using BenchmarkDotNet.Configs;
using System.Threading.Tasks;

namespace SimpleCsvParser.Performance
{
    class Program
    {
        private static string file = "PackageAssets.csv";

        static async Task Main(string[] args)
        {
            IConfig config = null;
#if DEBUG
            config = new DebugInProcessConfig();
#endif
            BenchmarkRunner.Run<SimpleParserSuite>(config);
        }
    }
}
