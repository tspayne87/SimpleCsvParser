using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class DataParserTests
    {
        [TestMethod]
        public void TestRemoveEntries()
        {
            var entries = CsvParser.ParseFile<TestModel>("test.csv", new CsvStreamOptions() { RemoveEmptyEntries = true }).ToList();
            Assert.AreEqual(entries.Count(), 20);
        }

        [TestMethod]
        public void TestStringParser()
        {
            using (var reader = new CsvStreamReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\r\nClaws,Attachment,10,\"34.5\",03/27/1987")))
            {
                var result = reader.AsEnumerable().FirstOrDefault();

                Assert.AreEqual(result.Name, "Claws");
                Assert.AreEqual(result.Type, TestType.Attachment);
                Assert.AreEqual(result.Cost, 10);
                Assert.AreEqual(result.Id, 34.5);
                Assert.AreEqual(result.Date, DateTime.Parse("03/27/1987"));
            }
        }

        [TestMethod]
        public void TestTabDelimited()
        {
            var result = CsvParser.Parse<TestModel>("name\ttype\tcost\tid\tdate\nClaws\tAttachment\t10\t\"34.5\"\t03/27/1987", new CsvStreamOptions() { Delimiter = '\t' }).FirstOrDefault();

            Assert.AreEqual(result.Name, "Claws");
            Assert.AreEqual(result.Type, TestType.Attachment);
            Assert.AreEqual(result.Cost, 10);
            Assert.AreEqual(result.Id, 34.5);
            Assert.AreEqual(result.Date, DateTime.Parse("03/27/1987"));

        }

        [TestMethod]
        public void TestCustomDelimiterAndWrapper()
        {
            var stream = StreamHelper.GenerateStream("name;type;cost;id;date\n*Claws*;Spell;50;50.55;*6-19-2012*");
            using (var reader = new CsvStreamReader<TestModel>(stream, new CsvStreamOptions() { Delimiter = ';', Wrapper = '*' }))
            {
                var result = reader.AsEnumerable().FirstOrDefault();

                Assert.AreEqual(result.Name, "Claws");
                Assert.AreEqual(result.Type, TestType.Spell);
                Assert.AreEqual(result.Cost, 50);
                Assert.AreEqual(result.Id, 50.55);
                Assert.AreEqual(result.Date, DateTime.Parse("6-19-2012"));
            }
        }

        [TestMethod]
        public void TestNoHeaders()
        {
            using (var reader = new CsvStreamReader<SecondTestModel>(StreamHelper.GenerateStream("Claws,Effect,10,60.05,9-5-1029"), new CsvStreamOptions() { ParseHeaders = false }))
            {
                var result = reader.AsEnumerable().FirstOrDefault();

                Assert.AreEqual(result.Name, "Claws");
                Assert.AreEqual(result.Type, TestType.Effect);
                Assert.AreEqual(result.Cost, 10);
                Assert.AreEqual(result.Id, 60.05);
                Assert.AreEqual(result.Date, DateTime.Parse("9-5-1029"));
            }
        }

        [TestMethod]
        public void TestNoDefaults()
        {
            using (var reader = new CsvStreamReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\nClaws,Attachment,10,\"34.5\",03/27/1987"), new CsvStreamOptions() { AllowDefaults = false }))
            {
                var result = reader.AsEnumerable().FirstOrDefault();

                Assert.AreEqual(result.Name, "Claws");
                Assert.AreEqual(result.Type, TestType.Attachment);
                Assert.AreEqual(result.Cost, 10);
                Assert.AreEqual(result.Id, 34.5);
                Assert.AreEqual(result.Date, DateTime.Parse("03/27/1987"));
            }
        }

        [TestMethod]
        public void TestTypes()
        {
            using (var reader = new CsvStreamReader<TypeModel>("type.csv"))
            {
                var results = reader.AsEnumerable().ToList();

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
                Assert.AreEqual(results[2].String, string.Empty);
            }
        }
    }
}
