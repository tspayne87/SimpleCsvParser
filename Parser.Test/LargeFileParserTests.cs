using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleCsvParser.Test
{
    [TestClass]
    public class LargeFileParserTests
    {
        [TestMethod]
        public void ProcessLargeFile()
        {
            using (var file = new CsvStreamReader<LargeModel>("large.csv", new CsvStreamOptions() { ParseHeaders = false }))
            {
                LargeModel item;
                int count = 0;
                while ((item = file.ReadRow()) != null)
                {
                    count++;
                }
            }
        }
    }
}