using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SimpleCsvParser
{
    public class CsvParser
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvParserOptions _options;

        /// <summary>
        /// Constructor that is meant to setup basic options for the parser.
        /// </summary>
        public CsvParser()
            : this(new CsvParserOptions()) { }

        /// <summary>
        /// Constructor to deal with custom options.
        /// </summary>
        /// <param name="options">Options that should be used by the parser.</param>
        public CsvParser(CsvParserOptions options)
        {
            _options = options;
        }
        
        /// <summary>
        /// Method is meant to parse a csv string into a list of objects.
        /// </summary>
        /// <param name="csvString">The csv string we need to parse.</param>
        /// <typeparam name="TModel">The model we need to create for each of the rows in the csv string.</typeparam>
        /// <returns>Will return a list of models that were asked for.</returns>
        public List<TModel> Parse<TModel>(string csvString)
            where TModel : class, new()
        {
            using (var file = new CsvStreamReader<TModel>(GenerateStream(csvString), _options))
            {
                return ProcessReader(file);
            }
        }

        /// <summary>
        /// Method is meant to open and parse the file to create a list of objects.
        /// </summary>
        /// <param name="path">The full path to the csv file.</param>
        /// <typeparam name="TModel">The models that we need to generate from the csv file.</typeparam>
        /// <returns>Will return a list of models that were asked for.</returns>
        public List<TModel> ParseFile<TModel>(string path)
            where TModel : class, new()
        {
            using (var file = new CsvStreamReader<TModel>(path, _options))
            {
                return ProcessReader(file);
            }
        }

        /// <summary>
        /// Method is meant to process the reader and return the list of models that were parsed by the reader.
        /// </summary>
        /// <param name="file">The File stream being processed.</param>
        /// <typeparam name="TModel">The model that the file should be parsed to.</typeparam>
        /// <returns>Will return a list of processed models.</returns>
        private List<TModel> ProcessReader<TModel>(CsvStreamReader<TModel> file)
            where TModel : class, new()
        {
            if (_options.ParseHeaders) file.ReadHeader();
            TModel item;
            bool isEmpty;
            var results = new List<TModel>();
            while ((item = file.ReadRow(out isEmpty)) != null)
            {
                if (!_options.RemoveEmptyEntries || !isEmpty)
                    results.Add(item);
            }
            return results;
        }

        /// <summary>
        /// Found at: https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
        /// </summary>
        /// <param name="s">The string that should be transformed into a stream.</param>
        /// <returns>Will return a stream that can be used in the csv stream reader.</returns>
        private Stream GenerateStream(string s)
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
