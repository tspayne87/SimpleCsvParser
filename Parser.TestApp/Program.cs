using SimpleCsvParser.Streams;
using SimpleCsvParser.Options;
using SimpleCsvParser.TestApp;
using System;

var count = 0;
//using (var reader = new CsvStreamModelReader<LargeModel>("large.csv", new CsvStreamReaderWithHeaderOptions() { IgnoreHeaders = true, RowDelimiter = "\n", StartRow = 1 }))
using (var reader = new CsvStreamReader("large.csv"))
{
  //reader.LoadHeaders();
  foreach(var item in reader.Parse()) {
    if (item.Count > 200) {
      Console.WriteLine(item.Count);
      break;
    }
    count++;
  }
}
Console.WriteLine(count);