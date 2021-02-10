using BenchmarkDotNet.Attributes;
using SimpleCsvParser;
using SimpleCsvParser.Streams;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser.Performance
{
    [MemoryDiagnoser]
    public class SimpleParserSuite
    {
        //[Benchmark]
        //public void SimpleCSVParser()
        //{
        //    CsvParser.ParseFile<DataModel>("PackageAssets.csv", new CsvStreamOptions() { RemoveEmptyEntries = true });
        //}

        [Benchmark]
        public void SimpleCSVParserEx()
        {
            using var stream = File.OpenRead("PackageAssets.csv");//consumer should own this stream / who are we to say where they get it from and depending on fs they may need to use different options for accessing fs
            CsvParser.ParseFile<DataModel>(stream, new CsvStreamOptions() { RemoveEmptyEntries = true });
        }

        [Benchmark]
        public void SimpleCSVParserParrallel()
        {
            using (var reader = new CsvStreamReader<DataModel>("PackageAssets.csv", new CsvStreamOptions() { RemoveEmptyEntries = true }))
            {
                var entries = reader.AsParallel()
                    .ToList();
            }
        }

        [Benchmark]
        public void CSVHelper()
        {
            using (var reader = new StreamReader("PackageAssets.csv"))
            using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<DataModel>();
            }
        }
    }
}
