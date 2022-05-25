using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCsvParser.Streams;
using SimpleCsvParser.Options;

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
      foreach (var item in reader.Parse())
        count++;
    }
  }
}