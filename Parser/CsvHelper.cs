using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleCsvParser
{
    internal static class CsvHelper
    {
        /// <summary>
        /// Method is meant to parse the current char array and build out the lines for the csv object.
        /// </summary>
        /// <param name="current">The current array we are processing.</param>
        /// <param name="isEnd">If we are at the end of the file being processed.</param>
        /// <returns>Returns an List of strings that represent the csv file.</returns>
        public static List<string> Split(Queue<char> q, CsvStreamOptions options)
        {
            var escaped = false;
            var noDelimiter = true;
            var results = new List<string>();
            var builder = new StringBuilder();

            while(q.Count > 0)
            {
                var current = q.Dequeue();
                if (escaped)
                { // Deal with if we are in a wrapper.
                    if (current == options.Wrapper && q.Count > 0 && q.Peek() == options.Wrapper)
                    {
                        builder.Append(q.Dequeue());
                    }
                    else if (current == options.Wrapper)
                    {
                        escaped = false;
                    }
                    else
                    {
                        builder.Append(current);
                    }
                }
                else if (current == options.Wrapper)
                { // Escape the value and start parsing as such
                    escaped = true;
                }
                else if (current == options.Delimiter)
                { // If we encounter a delmitier we want to put the current string into the results and clear the builder.
                    noDelimiter = false;
                    results.Add(builder.ToString());
                    builder.Clear();
                }
                else
                { // Append the new char
                    builder.Append(current);
                }
            }

            if (noDelimiter) throw new MalformedException("No Delimiter was found.");
            if (builder.Length > 0)
            {
                results.Add(builder.EndsWith(options.RowDelimiter) ? builder.ToString(0, builder.Length - options.RowDelimiter.Length) : builder.ToString());
            }

            return results;
        }
    }
}