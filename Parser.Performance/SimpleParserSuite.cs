using BenchmarkDotNet.Attributes;
using SimpleCsvParser.Streams;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using SimpleCsvParser;
using System.Collections.Generic;

namespace Parser.Performance
{
  [MemoryDiagnoser]
  public class SimpleParserSuite
  {
    [Benchmark]
    public void SimpleCSVParserModel()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
      using var reader = new CsvStreamModelReader<DataModel>(stream);
      reader.LoadHeaders();
      foreach(var item in reader.Parse())
        ; // NOOP
    }

    [Benchmark]
    public void SimpleCsvParser()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
      using var reader = new CsvStreamReader(stream);
      foreach(var item in reader.Parse())
        ; // NOOP
    }

    [Benchmark]
    public void SimpleRead()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
      using var reader = new StreamReader(stream, Encoding.UTF8, true, 4 * 1024, true);
      Span<char> buffer = new Span<char>(new char[4 * 1024]);
      int bufferLength;

      while ((bufferLength = reader.Read(buffer)) > 0)
      {
        for (var i = 0; i < bufferLength; ++i)
        {
          ; // NOOP
        }
      }
    }

    [Benchmark]
    public void CSVHelperModel()
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
