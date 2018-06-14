using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class ExceptionParserTests
    {
        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNoDefaults()
        {
            using (var reader = new CsvStreamReader<SecondTestModel>(StreamHelper.GenerateStream("Claws,,10,\"34.5\",03/27/1987"), new CsvStreamOptions() { AllowDefaults = false, ParseHeaders = false }))
            {
                reader.AsEnumerable().ToList();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestEndString()
        {
            using (var reader = new CsvStreamReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\nClaws,Attachment,10,\"34.5,03/27/1987")))
            {
                reader.AsEnumerable().ToList();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNotEnoughtItems()
        {
            using (var reader = new CsvStreamReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\nClaws,Attachment,10,\"34.5")))
            {
                reader.AsEnumerable().ToList();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNoDelimiterException()
        {
            var result = CsvParser.Parse<TestModel>("name\ttype\tcost\tid\tdate\nClaws\tAttachment\t10\t\"34.5\"\t03/27/1987");
        }
    }
}