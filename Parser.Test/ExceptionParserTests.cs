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
                reader.ReadAll();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestEndString()
        {
            using (var reader = new CsvStreamReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\nClaws,Attachment,10,\"34.5,03/27/1987")))
            {
                reader.ReadAll();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNotEnoughtItems()
        {
            using (var reader = new CsvStreamReader<TestModel>(StreamHelper.GenerateStream("name,type,cost,id,date\nClaws,Attachment,10,\"34.5")))
            {
                reader.ReadAll();
            }
        }
    }
}