using SimpleCsvParser.Streams;
using SimpleCsvParser.Options;
using SimpleCsvParser.TestApp;
using System;

var count = 0;
//using (var reader = new CsvStreamModelReader<LargeModel>("large.csv", new CsvStreamReaderWithHeaderOptions() { IgnoreHeaders = true, RowDelimiter = "\n", StartRow = 1 }))
using (var reader = new CsvStreamReader("large.csv"))
{
  //reader.LoadHeaders();
  reader.Parse(row => count++);
}
Console.WriteLine(count);