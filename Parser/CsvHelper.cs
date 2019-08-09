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
        /// Helper method is meant to take a char array that is representation of a row and split it up to its string columns.
        /// </summary>
        /// <param name="q">The char array we need to convert.</param>
        /// <param name="delimiter">The delimiter we need to break the columns up by.</param>
        /// <param name="wrapper">The wrapper to break apart strings.</param>
        /// <param name="rowDelimiter">The row delimiter to determine exceptions.</param>
        /// <param name="emptyColumns">The lamda expression to get empty columns.</param>
        /// <returns></returns>
        public static List<string> Split(Queue<char> q, string delimiter, char wrapper, string rowDelimiter, Func<int, string> emptyColumns = null)
        {
            if (q == null) return null;

            var escaped = false;
            var results = new List<string>();
            var builder = new StringBuilder();

            while(q.Count > 0)
            {
                var current = q.Dequeue();
                builder.Append(current);

                if (escaped)
                { // Deal with if we are in a wrapper.
                    if (current == wrapper && q.Count > 0 && q.Peek() == wrapper)
                    {
                        q.Dequeue();
                    }
                    else if (current == wrapper)
                    {
                        escaped = false;
                        builder.Remove(builder.Length - 1, 1);
                    }
                }
                else if (current == wrapper)
                { // Escape the value and start parsing as such
                    escaped = true;
                    builder.Remove(builder.Length - 1, 1);
                }
                else if (builder.EndsWith(delimiter))
                { // If we encounter a delmitier we want to put the current string into the results and clear the builder.
                    results.Add(builder.ToString(0, builder.Length - delimiter.Length));
                    builder.Clear();
                }
            }
            
            if (builder.Length > 0)
            {
                results.Add(builder.EndsWith(rowDelimiter) ? builder.ToString(0, builder.Length - rowDelimiter.Length) : builder.ToString());
            }

            if (emptyColumns != null)
                for (var i = 0; i < results.Count; ++i)
                    if (results[i].Trim().Length == 0)
                        results[i] = emptyColumns(i);

            return results;
        }
    }
}