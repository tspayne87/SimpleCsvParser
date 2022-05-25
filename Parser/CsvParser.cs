using System.Collections.Generic;
using System.IO;
using SimpleCsvParser.Streams;
using SimpleCsvParser.Options;
using System.Linq;
using System.Threading;
using System;

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
        public static void Parse<TModel>(string csv, Action<TModel> rowHandler)
           where TModel : class, new()
        {
            Parse<TModel>(csv, new CsvStreamReaderWithHeaderOptions(), rowHandler);
        }

        /// <summary>
        /// Method is meant to parse a csv string.
        /// </summary>
        /// <param name="csv">The csv string that needs to be parsed.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static void Parse<TModel>(string csv, CsvStreamReaderWithHeaderOptions options, Action<TModel> rowHandler)
           where TModel : class, new()
        {
            using var reader = new CsvStreamModelReader<TModel>(GenerateStream(csv), options);
            reader.LoadHeaders();
            reader.Parse(rowHandler, CancellationToken.None);
        }

        /// <summary>
        /// Method is meant to deal with reading csv files from the system.
        /// </summary>
        /// <param name="path">The file path to the csv file that needs to be parsed.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static void ParseFile<TModel>(string path, Action<TModel> rowHandler)
           where TModel : class, new()
        {
            ParseFile<TModel>(path, new CsvStreamReaderWithHeaderOptions(), rowHandler);
        }

        /// <summary>
        /// Method is meant to deal with reading csv files from the system.
        /// </summary>
        /// <param name="path">The file path to the csv file that needs to be parsed.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static void ParseFile<TModel>(string path, CsvStreamReaderWithHeaderOptions options, Action<TModel> rowHandler)
           where TModel : class, new()
        {
            using var reader = new CsvStreamModelReader<TModel>(path, options);
            reader.LoadHeaders();
            reader.Parse(rowHandler, CancellationToken.None);
        }

        /// <summary>
        /// Method is meant to deal with reading csv files from the system.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to turn into.</typeparam>
        /// <returns>The list of objects that the parser comes back at.</returns>
        public static void ParseFile<TModel>(Stream stream, CsvStreamReaderWithHeaderOptions csvStreamOptions, Action<TModel> rowHandler)
            where TModel : class, new()
        {
            ParseFile<TModel>(stream, csvStreamOptions, rowHandler, CancellationToken.None);
        }

        public static void ParseFile<TModel>(Stream stream, CsvStreamReaderWithHeaderOptions csvStreamOptions, Action<TModel> rowHandler, CancellationToken cancellationToken)
            where TModel : class, new()
        {
            using var reader = new CsvStreamModelReader<TModel>(stream, csvStreamOptions);
            reader.LoadHeaders();
            reader.Parse(rowHandler, cancellationToken);
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

        /// <summary>
        /// Helper method to convert the stream into a string result.
        /// </summary>
        /// <param name="stream">The stream that needs to be read.</param>
        /// <returns>Will return the string read from the stream.</returns>
        private static string ReadStream(Stream stream)
        {
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}