using BenchmarkDotNet.Attributes;
using SimpleCsvParser.Streams;
using System.Globalization;
using System.IO;

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
        var records = reader.Parse();
        foreach (var record in records)
        {
          ;//NOOP
        }
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
