using System;
using System.Collections.Generic;

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

            var data = string.Empty;
            var result = new List<string>();
            for (var i = 0; i < line.Length; ++i)
            {
                if (i == 0)
                { // Deal with edge case 1: We are at the start of the string
                    if (line[i] == _options.Wrapper) inWrapper = true;
                    else if (line[i] == _options.Delimiter) result.Add(data);
                    else data += line[i];
                }
                else if (i + 1 == line.Length)
                { // Deal with edge case 2: We are at the end of the string.
                    if (inWrapper && line[i] != _options.Wrapper)
                    {
                        throw new MalformedException($"Line {lineNumber} does not end its data wrapper.");
                    }
                    else if(line[i] == _options.Delimiter)
                    {
                        result.Add(data);
                        result.Add(string.Empty);
                    }
                    else
                    {
                        result.Add(inWrapper ? data : data + line[i]);
                    }
                }
                else
                { // Deal with normal case: we are in the middle of the string.
                    if (inWrapper)
                    { // If we are in a wrapper we should process it as such
                        if (line[i] == _options.Wrapper && line[i + 1] == _options.Wrapper)
                        {
                            data += _options.Wrapper;
                            i++;
                        }
                        else if (line[i] == _options.Wrapper && line[i + 1] == _options.Delimiter)
                        {
                            result.Add(data);
                            data = string.Empty;
                            inWrapper = false;
                            i++;
                            if (i + 1 == line.Length) result.Add(data);
                        }
                        else
                        {
                            data += line[i];
                        }
                    }
                    else if (line[i] == _options.Wrapper)
                    {
                        inWrapper = true;
                    }
                    else if (line[i] == _options.Delimiter)
                    {
                        result.Add(data);
                        data = string.Empty;
                    }
                    else
                    {
                        data += line[i];
                    }
                }
            }
            return result;
        }
    }
}