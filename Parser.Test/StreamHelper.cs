using System.IO;

namespace SimpleCsvParser.Test
{
    public static class StreamHelper
    {
        /// <summary>
        /// Found at: https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
        /// </summary>
        /// <param name="s">The string that should be transformed into a stream.</param>
        /// <returns>Will return a stream that can be used in the csv stream reader.</returns>
        public static Stream GenerateStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}