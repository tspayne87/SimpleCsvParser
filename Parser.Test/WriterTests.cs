using System;
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

                Assert.AreEqual($"\"{model.Name}\",{model.Type},{model.Cost},{model.Id},{model.Date},{Environment.NewLine}", result);
            }
        }
    }
}