using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleCsvParser.Streams;

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
            return Parse<TModel>(csv, new CsvStreamOptions());
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
                return reader.AsEnumerable().ToList();
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
            return ParseFile<TModel>(path, new CsvStreamOptions());
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
                return reader.AsEnumerable().ToList();
            }
        }

        /// <summary>
        /// Method is meant to parse out the models to a string for use somewhere else.
        /// </summary>
        /// <param name="models">The models that need to be parsed into a string.</param>
        /// <typeparam name="TModel">The model the parser needs to translate from.</typeparam>
        /// <returns>Will return a parsed string of the objects being passed in.</returns>
        public static string Stringify<TModel>(params TModel[] models)
            where TModel: class, new()
        {
            return Stringify(models.ToList(), new CsvStreamOptions());
        }

        /// <summary>
        /// Method is meant to parse out the models to a string for use somewhere else.
        /// </summary>
        /// <param name="models">The models that need to be parsed into a string.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to translate from.</typeparam>
        /// <returns>Will return a parsed string of the objects being passed in.</returns>
        public static string Stringify<TModel>(IEnumerable<TModel> models, CsvStreamOptions options)
            where TModel: class, new()
        {
            var result = string.Empty;
            var stream = GenerateStream(string.Empty);
            using (var writer = new CsvStreamWriter<TModel>(stream, options))
            {
                if (options.WriteHeaders) writer.WriteHeader();
                foreach(var model in models) writer.WriteLine(model);
                writer.Flush();
                result = ReadStream(stream);
            }
            return result;
        }

        /// <summary>
        /// Method is meant to write a file with a set of models.
        /// </summary>
        /// <param name="path">The path to the file we are saving to.</param>
        /// <param name="models">The models we are saving.</param>
        /// <typeparam name="TModel">The model the parser needs to translate from.</typeparam>
        public static void SaveFile<TModel>(string path, params TModel[] models)
            where TModel: class, new()
        {
            SaveFile<TModel>(path, models.ToList(), new CsvStreamOptions());
        }

        /// <summary>
        /// Method is meant to write a file with a set of models.
        /// </summary>
        /// <param name="path">The path to the file we are saving to.</param>
        /// <param name="models">The models we are saving.</param>
        /// <param name="options">The options that should be sent off to the stream.</param>
        /// <typeparam name="TModel">The model the parser needs to translate from.</typeparam>
        public static void SaveFile<TModel>(string path, IEnumerable<TModel> models, CsvStreamOptions options)
            where TModel: class, new()
        {
            using (var writer = new CsvStreamWriter<TModel>(path, options))
            {
                if (options.WriteHeaders) writer.WriteHeader();
                foreach (var model in models) writer.WriteLine(model);
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