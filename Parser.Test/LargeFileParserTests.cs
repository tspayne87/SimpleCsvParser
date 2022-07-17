using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCsvParser.Streams;
using SimpleCsvParser.Options;
using Parser.Test;
using System.IO;

namespace SimpleCsvParser.Test
{
  [TestClass]
  public class LargeFileParserTests
  {
    [TestMethod]
    public void ProcessLargeFile()
    {
      using var reader = new CsvStreamModelReader<LargeModel>("large.csv", new CsvStreamReaderWithHeaderOptions() { IgnoreHeaders = true, RowDelimiter = "\n", StartRow = 1 });
      reader.LoadHeaders();
      var count = 0;
      foreach(var item in reader.Parse())
        count++;
      Assert.AreEqual(50000, count);
    }

    [TestMethod]
    public void ProcessOtherLargeFile()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
      using var reader = new CsvStreamModelReader<DataModel>(stream);
      reader.LoadHeaders();
      foreach(var item in reader.Parse())
        ; // NOOP

      var i = 0;
    }

    [TestMethod]
    public void EscapedLargeFile()
    {
      using var reader = new CsvStreamReader("EscapedLargeFile.csv", new CsvStreamReaderOptions() { EscapeChar = '"' });
      var count = 0;
      foreach(var item in reader.Parse()) {
        count++;

        var index = -1;
        foreach(var col in item) {
          index++;
          Assert.IsFalse(col.Contains('"'));
        }
        Assert.AreEqual(item.Count, 200);
      }

      Assert.AreEqual(count, 15000);
    }
  }
}