using BenchmarkDotNet.Attributes;
using SimpleCsvParser.Streams;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Parser.Performance
{
  [MemoryDiagnoser]
  public class SimpleParserSuite
  {
    [Benchmark]
    public void SimpleCSVParserEx()
    {
      using var stream = File.OpenRead("PackageAssets.csv");//consumer should own this stream / who are we to say where they get it from and depending on fs they may need to use different options for accessing fs
      using (var reader = new CsvStreamModelReader<DataModel>(stream))
      {
        reader.LoadHeaders();
                var ct = new CancellationTokenSource();
        reader.Parse(row =>
        {
            ;
            if (1 == 0) //if we didn't want to evaluate every row (breaks out of the loop)
                ct.Cancel();
        },ct.Token);
        //foreach (var record in records)
        //{
        //  ;//NOOP
        //}
      }
    }

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
