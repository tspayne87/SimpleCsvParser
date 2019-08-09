using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCsvParser.Streams;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class ExceptionParserTests
    {
        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNoDefaults()
        {
            using (var reader = new CsvStreamReader<SecondTestModel>(StreamHelper.GenerateStream("Claws,,10,\"34.5\",03/27/1987"), new CsvStreamOptions() { AllowDefaults = false, ParseHeaders = false, DataRow = 0 }))
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
    }
}