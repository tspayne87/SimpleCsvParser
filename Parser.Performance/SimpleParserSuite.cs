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
    public void SimpleCSVParserModel()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
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
    public void SimpleCsvParser()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
      using var reader = new CsvStreamReader(stream);
      var records = reader.Parse();
      foreach (var record in records)
      {
        ;//NOOP
      }
    }

    [Benchmark]
    public void CSVHelper()
    {
      using var reader = new StreamReader("PackageAssets.csv");
      using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
      var records = csv.GetRecords<DataModel>();
      foreach (var record in records)
      {
        ;//NOOP
      }
    }
  }
}
