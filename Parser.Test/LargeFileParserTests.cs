using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class LargeFileParserTests
    {
        public void ProcessLargeFile()
        {
            using (var reader = new CsvStreamReader<LargeModel>("large.csv", new CsvStreamOptions() { ParseHeaders = false, RowDelimiter = "\n" }))
            {
                var count = 0;
                Parallel.ForEach(reader.AsEnumerable(), (x, state, index) => {
                    count++;
                });
            }
        }

        [TestMethod]
        public void TestSaveFile()
        {
            var total = 100000;
            var fileName = "test.large.csv";
            var options = new CsvStreamOptions()
            {
                Delimiter = ":",
                RowDelimiter = "|||",
                Wrapper = '\''
            };

            var list = new List<TestModel>();
            for (var i = 0; i < total; ++i)
            {
                list.Add(new TestModel()
                {
                    Name = $"Test {i}",
                    Type = TestType.Attachment,
                    Cost = 10 * i,
                    Id = i,
                    Date = DateTime.Now
                });
            }
            CsvParser.SaveFile(fileName, list, options);

            // Parse the respones and check if it was saved correctly.
            var entries = CsvParser.ParseFile<TestModel>(fileName, options).ToList();
            Assert.AreEqual(entries.Count(), total);

            for (var i = 0; i < entries.Count; ++i)
            {
                Assert.AreEqual(entries[i].Name, list[i].Name);
                Assert.AreEqual(entries[i].Type, list[i].Type);
                Assert.AreEqual(entries[i].Cost, list[i].Cost);
                Assert.AreEqual(entries[i].Id, list[i].Id);
                Assert.AreEqual(entries[i].Date.ToString(), list[i].Date.ToString());
            }
        }
    }
}