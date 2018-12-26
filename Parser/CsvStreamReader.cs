using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleCsvParser
{
    public class CsvStreamReader<TModel> : IDisposable
        where TModel: class, new()
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;
        /// <summary>
        /// The line parser to convert each line to a list of string that will be parsed.
        /// </summary>
        private readonly CsvLineParser _line;
        /// <summary>
        /// The converter that will deal with changing the list of strings into an object.
        /// </summary>
        private CsvLineConverter<TModel> _converter;
        /// <summary>
        /// Internal use of the previous count to make sure the csv file is formatted correctly.
        /// </summary>
        private int _previousCount = -1;
        /// <summary>
        /// The reader we will be getting data from.
        /// </summary>
        private StreamReader _reader;
        /// <summary>
        /// The stream that we need to dispose.
        /// </summary>
        private Stream _stream;

        #region Constructors
        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        public CsvStreamReader(Stream stream)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
            _stream = stream;
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        public CsvStreamReader(string path)
        {
            _options = new CsvStreamOptions();
            _line = new CsvLineParser(_options);
            _stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Constructor that will deal with streams.
        /// </summary>
        /// <param name="stream">The stream the csv data exists in.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(Stream stream, CsvStreamOptions options)
        {
            _options = options;
            _line = new CsvLineParser(_options);
            _stream = stream;
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Constructor that will deal with files and convert them into a stream.
        /// </summary>
        /// <param name="path">The file path that we need to create a stream from.</param>
        /// <param name="options">The stream options we need to use for parsing.</param>
        public CsvStreamReader(string path, CsvStreamOptions options)
        {
            _options = options;
            _line = new CsvLineParser(_options);
            _stream = File.Open(path, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_stream);
        }
        #endregion

        /// <summary>
        /// Converts the stream into a enumerable list of models.
        /// </summary>
        /// <param name="eachItem">The callback that should be used for each item.</param>
        public IEnumerable<TModel> AsEnumerable()
        {
            ProcessConverter();
            return InternalRead()
                .Select((x, index) => ReadLine(x, index + 1))
                .Where(x => x != null);
        }

        /// <summary>
        /// Method is meant to be used with large files, so that the user can process each row one at a time instead of creating
        /// a list.
        /// </summary>
        /// <param name="eachItem">The action that will be called back foreach item.</param>
        public void ForEach(Action<TModel> eachItem)
        {
            ProcessConverter();
            Parallel.ForEach(InternalRead(), (x, state, index) => {
                eachItem(ReadLine(x, (int)index + 1));
            });
        }

        /// <summary>
        /// Helper method that will build out the converter before the process starts creating the enumerable list.
        /// </summary>
        private void ProcessConverter()
        {
            if (_options.ParseHeaders)
            { // If we need to parse the first line of headers in the stream/file.
                var line = _reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var headers = _line.Process(line.ToCharArray(), 0);
                    _previousCount = headers.Count;
                    _converter = new CsvLineConverter<TModel>(_options, headers);
                }
            }
            if (_converter == null) _converter = new CsvLineConverter<TModel>(_options, null);
        }

        /// <summary>
        /// Helper method that will convert the streams into a list of char characters based on each of the rows.
        /// </summary>
        /// <returns>Will return an enumerable list of char arrays represented by the row of characters.</returns>
        private IEnumerable<char[]> InternalRead()
        {
            char[] buffer = new char[256 * 1024]; // Create a buffer to store the characters loaded from the stream.
            char[] previous = new char[(256 * 1024) + 1]; // Create a previous buffer to deal with saving snippets between reads.
            int previousIndex = 0; // The previous index to read to when getting the previous characters.
            while (true)
            {
                var blockIndex = _reader.ReadBlock(buffer, 0, buffer.Length);
                if (_reader.Peek() == -1)
                {
                    foreach(var line in Split(previous.Take(previousIndex).Concat(buffer.Take(blockIndex)).ToArray()))
                    { // If we are dealing with a \r at the end of the char array we need to trim it off.
                        if (line.Length == 0) continue;
                        yield return line[line.Length - 1] == '\r' ? line.Take(line.Length - 1).ToArray() : line;
                    }
                    break;
                }
                else
                {
                    // Get the current lines from the read buffer
                    var lines = Split(previous.Take(previousIndex).Concat(buffer).ToArray()).ToList();

                    // Read all the lines but the last one because it might not be the end of that 
                    var lastIndex = lines.Count - 1;
                    for (var i = 0; i < lastIndex; ++i) yield return lines[i];

                    // Save the previous line for use in the next buffer
                    if (lines[lastIndex].Length > previous.Length) throw new ArgumentOutOfRangeException("Line exceeded maxium limit for row");
                    for (var i = 0; i < lines[lastIndex].Length; ++i) previous[i] = lines[lastIndex][i];
                    previousIndex = lines[lastIndex].Length;
                }
            }
        }

        /// <summary>
        /// Helper method is meant to split char arrays up into a list of new line arrays.
        /// </summary>
        /// <param name="current">The current array we are splitting up on newline characters.</param>
        /// <returns>Will return an enumerable for each of the rows in the char array.</returns>
        private IEnumerable<char[]> Split(char[] current)
        {
            char[] buffer = new char[current.Length];
            int bufferIndex = 0;
            for (var i = 0; i < current.Length; ++i)
            {
                if (current.Length > (i + 1) && current[i] == '\r' && current[i + 1] == '\n')
                { // If this is a windows style new line we should progress the iterator return a char array and update the buffer index.
                    i++;
                    yield return buffer.Take(bufferIndex).ToArray();
                    bufferIndex = 0;
                }
                else if (current[i] == '\r')
                { // Deal with carrage returns.
                    if (i == current.Length - 1)
                    { // If the \r is at the end of the line we need to save this just in case the \n is in the next read
                        buffer[bufferIndex++] = current[i];
                    }
                    else
                    {
                        yield return buffer.Take(bufferIndex).ToArray();
                        bufferIndex = 0;
                    }
                }
                else if (current[i] == '\n')
                { // Deal with just new lines.
                    yield return buffer.Take(bufferIndex).ToArray();
                    bufferIndex = 0;
                }
                else
                {
                    buffer[bufferIndex++] = current[i];
                }
            }

            // Return the final result
            yield return buffer.Take(bufferIndex).ToArray();
        }

        /// <summary>
        /// Helper method is meant to read a char array and convert it to the desired object.
        /// </summary>
        /// <param name="line">The char array we need to parse.</param>
        /// <param name="lineNumber">The current line number this line is in the file or string.</param>
        /// <returns>Will return the desired object or null if nothing needs to be processed.</returns>
        private TModel ReadLine(char[] line, int lineNumber)
        {
            var parsed = _line.Process(line, lineNumber);
            if (_previousCount != -1 && _previousCount != parsed.Count) throw new MalformedException($"Line {lineNumber} has {parsed.Count} but should have {_previousCount}.");
            _previousCount = parsed.Count;
            if (!_options.RemoveEmptyEntries || parsed.Where(x => string.IsNullOrEmpty(x)).Count() != _previousCount)
                return _converter.Parse(parsed, lineNumber);
            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Method is meant to dispose this object after use.
        /// </summary>
        /// <param name="disposing">If we are needing to dispose the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream.Dispose();
                    _reader.Dispose();
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Method is meant to dispose of this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}