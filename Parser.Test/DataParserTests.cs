using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCsvParser.Streams;
using SimpleCsvParser.Options;
using System.Collections.Generic;
using System.Threading;

namespace SimpleCsvParser.Test
{
  [TestClass]
  public class DataParserTests
  {
    [TestMethod]
    public void TestRemoveEntries()
    {
      using var stream = File.OpenRead("test.csv");
      var reader = new CsvStreamModelReader<TestModel>(stream, new CsvStreamReaderWithHeaderOptions() { StartRow = 1, RemoveEmptyEntries = true });

      reader.LoadHeaders();
      var items = new List<TestModel>();
      reader.Parse(row => items.Add(row));
      Assert.AreEqual(20, items.Count);
    }

    [TestMethod]
    public void TestSkipRows()
    {
      var entries = new List<TestModel>();
      CsvParser.ParseFile<TestModel>("test.csv", new CsvStreamReaderWithHeaderOptions() { RemoveEmptyEntries = true, StartRow = 10 }, row => entries.Add(row));
      Assert.AreEqual(14, entries.Count);
    }

    [TestMethod]
    public void TestRowStream()
    {
      var stream = StreamHelper.GenerateStream("This is an example message\r\nThe Data follows this\r\nAnother Test String");
      using var reader = new CsvStreamReader(stream, new CsvStreamReaderOptions() { RemoveEmptyEntries = true });
      var items = new List<IList<string>>();
      reader.Parse(row => items.Add(row));

      Assert.AreEqual("This is an example message", items[0][0]);
      Assert.AreEqual("The Data follows this", items[1][0]);
      Assert.AreEqual("Another Test String", items[2][0]);
    }

    [TestMethod]
    public void TestWithEmptyDelimiter()
    {
      var stream = StreamHelper.GenerateStream("name,type,cost,id,date\r\nClaws,Attachment,10,34.5,03/27/1987");
      using var reader = new CsvStreamReader(stream, new CsvStreamReaderOptions() { Wrapper = null, Delimiter = string.Empty });
      var result = new List<IList<string>>();
      reader.Parse(row => result.Add(row));

      Assert.AreEqual(2, result.Count);
      Assert.AreEqual("name,type,cost,id,date", result[0][0]);
      Assert.AreEqual("Claws,Attachment,10,34.5,03/27/1987", result[1][0]);
    }

    [TestMethod]
    public void TestWithNoWrapper()
    {
      var stream = StreamHelper.GenerateStream("name,type,cost,id,date\r\nClaws,Attachment,10,34.5,03/27/1987");
      using var reader = new CsvStreamModelReader<TestModel>(stream, new CsvStreamReaderWithHeaderOptions() { Wrapper = null, HeaderWrapper = null, StartRow = 1 });
      reader.LoadHeaders();

      TestModel result = default;
      CancellationTokenSource ct = new CancellationTokenSource();
      reader.Parse(row => { result = row; ct.Cancel(); }, ct.Token);

      Assert.AreEqual("Claws", result.Name);
      Assert.AreEqual(TestType.Attachment, result.Type);
      Assert.AreEqual(10, result.Cost);
      Assert.AreEqual(34.5, result.Id);
      Assert.AreEqual(DateTime.Parse("03/27/1987"), result.Date);
    }

    [TestMethod]
    public void TestSkipHeaderAndData()
    {
      var stream = StreamHelper.GenerateStream("This is an example message\r\n \r\nname,type,cost,id,date\r\n \r\nThe Data follows this:\r\nClaws,Attachment,10,\"34.5\",03/27/1987");
      using var reader = new CsvStreamModelReader<TestModel>(stream, new CsvStreamReaderWithHeaderOptions() { StartRow = 5, HeaderStartRow = 2 });
      reader.LoadHeaders();
      TestModel result = default;
      CancellationTokenSource ct = new CancellationTokenSource();
      reader.Parse(row => { result = row; ct.Cancel(); }, ct.Token);

      Assert.AreEqual("Claws", result.Name);
      Assert.AreEqual(TestType.Attachment, result.Type);
      Assert.AreEqual(10, result.Cost);
      Assert.AreEqual(34.5, result.Id);
      Assert.AreEqual(DateTime.Parse("03/27/1987"), result.Date);
    }

    [TestMethod]
    public void TestStringParser()
    {
      using var reader = new CsvStreamModelReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\r\nClaws,Attachment,10,\"34.5\",03/27/1987"), new CsvStreamReaderWithHeaderOptions() { StartRow = 1 });
      reader.LoadHeaders();
      TestModel result = default;
      CancellationTokenSource ct = new CancellationTokenSource();
      reader.Parse(row => { result = row; ct.Cancel(); }, ct.Token);

      Assert.AreEqual("Claws", result.Name);
      Assert.AreEqual(TestType.Attachment, result.Type);
      Assert.AreEqual(10, result.Cost);
      Assert.AreEqual(34.5, result.Id);
      Assert.AreEqual(DateTime.Parse("03/27/1987"), result.Date);
    }

    [TestMethod]
    public void TestTabDelimited()
    {
      TestModel result = default;
      CancellationTokenSource ct = new CancellationTokenSource();
      CsvParser.Parse<TestModel>(
          "name\ttype\tcost\tid\tdate\nClaws\tAttachment\t10\t\"34.5\"\t03/27/1987",
          new CsvStreamReaderWithHeaderOptions() { Delimiter = "\t", RowDelimiter = "\n", HeaderRowDelimiter = "\n", HeaderDelimiter = "\t", StartRow = 1 },
          row => { result = row; ct.Cancel(); },
          ct.Token);

      Assert.AreEqual("Claws", result.Name);
      Assert.AreEqual(TestType.Attachment, result.Type);
      Assert.AreEqual(10, result.Cost);
      Assert.AreEqual(34.5, result.Id);
      Assert.AreEqual(DateTime.Parse("03/27/1987"), result.Date);

    }

    [TestMethod]
    public void TestCustomDelimiterAndWrapper()
    {
      var stream = StreamHelper.GenerateStream("name;type;cost;id;date\n*Claws*;Spell;50;50.55;*6-19-2012*");
      using var reader = new CsvStreamModelReader<TestModel>(stream, new CsvStreamReaderWithHeaderOptions() { Delimiter = ";", Wrapper = '*', RowDelimiter = "\n", HeaderRowDelimiter = "\n", HeaderDelimiter = ";", StartRow = 1 });
      reader.LoadHeaders();
      TestModel result = default;
      CancellationTokenSource ct = new CancellationTokenSource();
      reader.Parse(row => { result = row; ct.Cancel(); }, ct.Token);

      Assert.AreEqual("Claws", result.Name);
      Assert.AreEqual(TestType.Spell, result.Type);
      Assert.AreEqual(50, result.Cost);
      Assert.AreEqual(50.55, result.Id);
      Assert.AreEqual(DateTime.Parse("6-19-2012"), result.Date);
    }

    [TestMethod]
    public void TestNoHeaders()
    {
      using var reader = new CsvStreamModelReader<SecondTestModel>(StreamHelper.GenerateStream("Claws,Effect,10,60.05,9-5-1029"), new CsvStreamReaderWithHeaderOptions() { IgnoreHeaders = true, StartRow = 0 });
      reader.LoadHeaders();
      SecondTestModel result = default;
      CancellationTokenSource ct = new CancellationTokenSource();
      reader.Parse(row => { result = row; ct.Cancel(); }, ct.Token);

      Assert.AreEqual("Claws", result.Name);
      Assert.AreEqual(TestType.Effect, result.Type);
      Assert.AreEqual(10, result.Cost);
      Assert.AreEqual(60.05, result.Id);
      Assert.AreEqual(DateTime.Parse("9-5-1029"), result.Date);

    }

    [TestMethod]
    public void TestTypes()
    {
      using var reader = new CsvStreamModelReader<TypeModel>("type.csv", new CsvStreamReaderWithHeaderOptions() { StartRow = 1 });
      reader.LoadHeaders();

      List<TypeModel> results = new List<TypeModel>();
      reader.Parse(row => results.Add(row));

      Assert.AreEqual(3, results.Count);

      Assert.AreEqual(results[0].Boolean, true);
      Assert.AreEqual(results[1].Boolean, false);
      Assert.AreEqual(results[2].Boolean, default(bool));

      Assert.AreEqual(results[0].Char, 't');
      Assert.AreEqual(results[1].Char, 'h');
      Assert.AreEqual(results[2].Char, default(char));

      Assert.AreEqual(results[0].SByte, (sbyte)1);
      Assert.AreEqual(results[1].SByte, (sbyte)1);
      Assert.AreEqual(results[2].SByte, default(sbyte));

      Assert.AreEqual(results[0].Byte, (byte)5);
      Assert.AreEqual(results[1].Byte, (byte)2);
      Assert.AreEqual(results[2].Byte, default(byte));

      Assert.AreEqual(results[0].Int16, (short)1);
      Assert.AreEqual(results[1].Int16, (short)16);
      Assert.AreEqual(results[2].Int16, default(short));

      Assert.AreEqual(results[0].UInt16, (ushort)5);
      Assert.AreEqual(results[1].UInt16, (ushort)116);
      Assert.AreEqual(results[2].UInt16, default(ushort));

      Assert.AreEqual(results[0].Int32, (int)1);
      Assert.AreEqual(results[1].Int32, (int)32);
      Assert.AreEqual(results[2].Int32, default(int));

      Assert.AreEqual(results[0].UInt32, (uint)5);
      Assert.AreEqual(results[1].UInt32, (uint)332);
      Assert.AreEqual(results[2].UInt32, default(uint));

      Assert.AreEqual(results[0].Int64, (long)1);
      Assert.AreEqual(results[1].Int64, (long)64);
      Assert.AreEqual(results[2].Int64, default(int));

      Assert.AreEqual(results[0].UInt64, (ulong)5);
      Assert.AreEqual(results[1].UInt64, (ulong)664);
      Assert.AreEqual(results[2].UInt64, default(ulong));

      Assert.AreEqual(results[0].Single, (Single)1);
      Assert.AreEqual(results[1].Single, (Single)111);
      Assert.AreEqual(results[2].Single, default(Single));

      Assert.AreEqual(results[0].Double, (double)5.55);
      Assert.AreEqual(results[1].Double, (double)34.86);
      Assert.AreEqual(results[2].Double, default(double));

      Assert.AreEqual(results[0].Decimal, (decimal)1.88);
      Assert.AreEqual(results[1].Decimal, (decimal)176.88);
      Assert.AreEqual(results[2].Decimal, default(decimal));

      Assert.AreEqual(results[0].DateTime, DateTime.Parse("01/03/1994"));
      Assert.AreEqual(results[1].DateTime, DateTime.Parse("9-12-2005"));
      Assert.AreEqual(results[2].DateTime, default(DateTime));

      Assert.AreEqual(results[0].String, "This is a string for you");
      Assert.AreEqual(results[1].String, "Special String, for \"you\"");
      Assert.AreEqual(results[2].String, null);
    }
  }
}