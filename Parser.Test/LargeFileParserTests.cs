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

      reader.Parse(row => count++);
      Assert.AreEqual(50000, count);
    }

    [TestMethod]
    public void ProcessOtherLargeFile()
    {
      using var stream = File.OpenRead("PackageAssets.csv");
      using var reader = new CsvStreamModelReader<DataModel>(stream);
      reader.LoadHeaders();
      reader.Parse(row =>
      {
        ; // NOOP
      });
    }
  }
}