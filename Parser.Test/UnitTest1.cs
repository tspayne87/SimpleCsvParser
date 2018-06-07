using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parser.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var parser = new CsvParser(new CsvParserOptions() { RemoveEmptyEntries = true }))
            {
                parser.ParseFile<TestModel>("test.csv");
            }
        }
    }
}
