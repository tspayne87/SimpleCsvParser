using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parser.Test
{
    [TestClass]
    public class ExceptionParserTests
    {
        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNoDefaults()
        {
            using (var parser = new CsvParser(new CsvParserOptions() { AllowDefaults = false, ParseHeaders = false }))
            {
                parser.Parse<SecondTestModel>("Claws,,10,\"34.5\",03/27/1987");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestEndString()
        {
            using (var parser = new CsvParser())
            {
                parser.Parse<TestModel>("name,type,cost,id,date\nClaws,Attachment,10,\"34.5,03/27/1987");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MalformedException))]
        public void TestNotEnoughtItems()
        {
            using (var parser = new CsvParser())
            {
                parser.Parse<TestModel>("name,type,cost,id,date\nClaws,Attachment,10,\"34.5");
            }
        }
    }
}