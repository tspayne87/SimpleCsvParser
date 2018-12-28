using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class WriterTests
    {
        [TestMethod]
        public void TestFile()
        {
            using (var writer = new CsvStreamWriter<TypeModel>("type.writer.csv"))
            {
                writer.WriteHeader();
                for (var i = 0; i < 25; ++i) {
                    writer.WriteLine(new TypeModel() { Int32 = i, Char = 'A' });
                }
            }

            // Parse the respones and check if it was saved correctly.
            var entries = CsvParser.ParseFile<TypeModel>("type.writer.csv", new CsvStreamOptions() { RemoveEmptyEntries = true }).ToList();
            Assert.AreEqual(entries.Count(), 25);

            var index = 0;
            foreach (var entry in entries) {
                Assert.AreEqual(entry.Char, 'A');
                Assert.AreEqual(entry.Int32, index++);
            }
        }

        [TestMethod]
        public void TestStream() {
            var stream = StreamHelper.GenerateStream(string.Empty);
            using (var writer = new CsvStreamWriter<TestModel>(stream)) {
                var model = new TestModel() {
                    Name = "Doe, Jon",
                    Type = TestType.Attachment,
                    Cost = 50,
                    Id = 1,
                    Date = DateTime.Now
                };
                writer.WriteLine(model);
                writer.Flush();

                var reader = new StreamReader(stream);
                var result = reader.ReadToEnd();

                Assert.AreEqual($"\"{model.Name}\",{model.Type},{model.Cost},{model.Id},{model.Date}\r\n", result);
            }
        }

        [TestMethod]
        public void TestStringify() {
            var model = new TestModel() {
                Name = "Doe, Jon",
                Type = TestType.Attachment,
                Cost = 50,
                Id = 1,
                Date = DateTime.Now
            };
            Assert.AreEqual($"name,type,cost,id,date\r\n\"{model.Name}\",{model.Type},{model.Cost},{model.Id},{model.Date}\r\n", CsvParser.Stringify(model));
        }

        [TestMethod]
        public void TestSaveFile()
        {
            var total = 25;
            var options = new CsvStreamOptions() {
                Delimiter = ':',
                RowDelimiter = "|||",
                Wrapper = '\''
            };

            var list = new List<TestModel>();
            for (var i = 0; i < total; ++i) {
                list.Add(new TestModel() {
                    Name = $"Test {i}",
                    Type = TestType.Attachment,
                    Cost = 10 * i,
                    Id = i,
                    Date = DateTime.Now
                });
            }
            CsvParser.SaveFile("test.writer.csv", list, options);

            // Parse the respones and check if it was saved correctly.
            var entries = CsvParser.ParseFile<TestModel>("test.writer.csv", options).ToList();
            Assert.AreEqual(entries.Count(), total);

            for (var i = 0; i < entries.Count; ++i) {
                Assert.AreEqual(entries[i].Name, list[i].Name);
                Assert.AreEqual(entries[i].Type, list[i].Type);
                Assert.AreEqual(entries[i].Cost, list[i].Cost);
                Assert.AreEqual(entries[i].Id, list[i].Id);
                Assert.AreEqual(entries[i].Date.ToString(), list[i].Date.ToString());
            }
        }
    }
}