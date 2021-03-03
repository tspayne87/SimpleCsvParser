using BenchmarkDotNet.Attributes;
using SimpleCsvParser;
using SimpleCsvParser.Streams;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Performance
{
    [MemoryDiagnoser]
    public class SimpleParserSuite
    {
        ////[Benchmark]
        ////public void SimpleCSVParser()
        ////{
        ////    CsvParser.ParseFile<DataModel>("PackageAssets.csv", new CsvStreamOptions() { RemoveEmptyEntries = true });
        ////}

        [Benchmark]
        public async Task SimpleCSVParserEx()
        {
            using var stream = File.OpenRead("PackageAssets.csv");//consumer should own this stream / who are we to say where they get it from and depending on fs they may need to use different options for accessing fs
            var reader = new CsvStreamReader<DataModel>(stream, new CsvStreamOptions() { RemoveEmptyEntries = true, ParseHeaders = false, CloseStream = false });
            //var records = CsvParser.ParseFile<DataModel>(stream, new CsvStreamOptions() { RemoveEmptyEntries = true, ParseHeaders=false, CloseStream=false });
            await foreach (var record in reader.AsEnumerable())
            {
                ;//NOOP
            }
        }

        //[Benchmark]
        //public void NoLinq()
        //{
        //    using var stream = File.OpenRead("PackageAssets.csv");//consumer should own this stream / who are we to say where they get it from and depending on fs they may need to use different options for accessing fs            
        //    CustomParser.ParseFile(stream);
        //}

        ////[Benchmark]
        ////public void SimpleCSVParserParrallel()
        ////{
        ////    using (var reader = new CsvStreamReader<DataModel>("PackageAssets.csv", new CsvStreamOptions() { RemoveEmptyEntries = true }))
        ////    {
        ////        var entries = reader.AsParallel()
        ////            .ToList();
        ////    }
        ////}

        [Benchmark]
        public void CSVHelper()
        {
            using (var reader = new StreamReader("PackageAssets.csv"))
            using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<DataModel>();
                foreach (var record in records)
                {
                    ;//NOOP
                }
            }
        }
    }
}
