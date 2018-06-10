using System.Collections.Generic;
using System.IO;

namespace SimpleCsvParser
{
    public static class CsvParser
    {
        /// <summary>
        /// Method is meant to parse a csv string.
        /// </summary>
        /// <param name="csv">The csv string that needs to be parsed.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static List<TModel> Parse<TModel>(string csv)
            where TModel: class, new()
        {
            using (var reader = new CsvStreamReader<TModel>(GenerateStream(csv)))
            {
                return reader.ReadAll();
            }
        }

        /// <summary>
        /// Method is meant to parse a csv string.
        /// </summary>
        /// <param name="csv">The csv string that needs to be parsed.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static List<TModel> Parse<TModel>(string csv, CsvStreamOptions options)
            where TModel: class, new()
        {
            using (var reader = new CsvStreamReader<TModel>(GenerateStream(csv), options))
            {
                return reader.ReadAll();
            }
        }

        /// <summary>
        /// Method is meant to deal with reading csv files from the system.
        /// </summary>
        /// <param name="path">The file path to the csv file that needs to be parsed.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static List<TModel> ParseFile<TModel>(string path)
            where TModel: class, new()
        {
            using (var reader = new CsvStreamReader<TModel>(path))
            {
                return reader.ReadAll();
            }
        }

        /// <summary>
        /// Method is meant to deal with reading csv files from the system.
        /// </summary>
        /// <param name="path">The file path to the csv file that needs to be parsed.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static List<TModel> ParseFile<TModel>(string path, CsvStreamOptions options)
            where TModel: class, new()
        {
            using (var reader = new CsvStreamReader<TModel>(path, options))
            {
                return reader.ReadAll();
            }
        }

        /// <summary>
        /// Found at: https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
        /// </summary>
        /// <param name="s">The string that should be transformed into a stream.</param>
        /// <returns>Will return a stream that can be used in the csv stream reader.</returns>
        private static Stream GenerateStream(string s)
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