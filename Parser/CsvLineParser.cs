using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCsvParser
{
    internal class CsvLineParser
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;

        /// <summary>
        /// Constructor that is used to build out this piece of the parser.
        /// </summary>
        /// <param name="options">The options that will be used in the parser.</param>
        public CsvLineParser(CsvStreamOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Method is meant to process one line of the csv file and convert it to a string array to be processed later.
        /// </summary>
        /// <param name="line">The line we are processing.</param>
        /// <param name="lineNumber">The current line number in the csv file we are processing on, this is mainly used for exceptions.</param>
        /// <returns>Will return a list of string gathered from the csv row.</returns>
        public List<string> Process(char[] line, long lineNumber)
        {
            if (Array.IndexOf(line, _options.Delimiter) == -1) throw new MalformedException($"No Delimiter found on line {lineNumber}, is the correct delimiter used in the options?");
            var inWrapper = false;

            var buffer = new StringBuilder();
            var result = new List<string>();
            for (var i = 0; i < line.Length; ++i)
            {
                if (i == 0)
                { // Deal with edge case 1: We are at the start of the string
                    if (line[i] == _options.Wrapper) inWrapper = true;
                    else if (line[i] == _options.Delimiter) result.Add(buffer.ToString());
                    else buffer.Append(line[i]);
                }
                else if (i + 1 == line.Length)
                { // Deal with edge case 2: We are at the end of the string.
                    if (inWrapper && line[i] != _options.Wrapper)
                    {
                        throw new MalformedException($"Line {lineNumber} does not end its data wrapper.");
                    }
                    else if (line[i] == _options.Delimiter)
                    {
                        result.Add(buffer.ToString());
                        result.Add(string.Empty);
                    }
                    else
                    {
                        result.Add(inWrapper ? buffer.ToString() : buffer.ToString() + line[i]);
                    }
                }
                else
                { // Deal with normal case: we are in the middle of the string.
                    if (inWrapper)
                    { // If we are in a wrapper we should process it as such
                        if (line[i] == _options.Wrapper && line[i + 1] == _options.Wrapper)
                        {
                            buffer.Append(_options.Wrapper);
                            i++;
                        }
                        else if (line[i] == _options.Wrapper && line[i + 1] == _options.Delimiter)
                        {
                            result.Add(buffer.ToString());
                            buffer.Clear();
                            inWrapper = false;
                            i++;
                            if (i + 1 == line.Length) result.Add(string.Empty);
                        }
                        else
                        {
                            buffer.Append(line[i]);
                        }
                    }
                    else if (line[i] == _options.Wrapper)
                    {
                        inWrapper = true;
                    }
                    else if (line[i] == _options.Delimiter)
                    {
                        result.Add(buffer.ToString());
                        buffer.Clear();
                    }
                    else
                    {
                        buffer.Append(line[i]);
                    }
                }
            }
            return result;
        }
    }
}