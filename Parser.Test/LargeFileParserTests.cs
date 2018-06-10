using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class LargeFileParserTests
    {
        [TestMethod]
        public void ProcessLargeFile()
        {

            using (var reader = new CsvStreamReader<LargeModel>("large.csv", new CsvStreamOptions() { ParseHeaders = false }))
            {
                int count = 0;
                reader.Read(x => count++);
            }
        }
    }
}