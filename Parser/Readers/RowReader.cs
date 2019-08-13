using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleCsvParser.Readers
{
    internal class RowReader
    {
        /// <summary>
        /// The internal char reader.
        /// </summary>
        private readonly CharReader _reader;
        /// <summary>
        /// The Delimiter we need to break the rows up by.
        /// </summary>
        private string _delimiter;
        /// <summary>
        /// The wrapper we need to use when needing to escape string data.
        /// </summary>
        private char? _wrapper;

        /// <summary>
        /// Constructor to build a row from the stream.
        /// </summary>
        /// <param name="reader">The char reader that needs to be read from.</param>
        /// <param name="delimiter">The delimiter we need to break the char reader up by.</param>
        /// <param name="wrapper">The wrapper we need to use to determine when we need to escape things.</param>
        public RowReader(CharReader reader, string delimiter, char? wrapper)
        {
            _reader = reader;
            _delimiter = delimiter;
            _wrapper = wrapper;
        }

        /// <summary>
        /// Will turn this reader into an enumerable that breaks into rows from the stream.
        /// </summary>
        /// <returns>Will return row enumerables.</returns>
        public IEnumerable<Queue<char>> AsEnumerable()
        {
            using (var iter = _reader.AsEnumerable().GetEnumerator())
            {
                if (iter.MoveNext())
                {
                    var current = iter.Current;
                    var escaped = false;
                    var queue = new Queue<char>();
                    var rowBreak = _delimiter.ToCharArray();
                    var currentRowBreak = new char[rowBreak.Length];

                    while(iter.MoveNext())
                    {
                        if (escaped)
                        { // Deal with if we are in a wrapper.
                            if (current == _wrapper && iter.Current == _wrapper)
                            {
                                queue.Enqueue(current);
                                queue.Enqueue(iter.Current);
                                if (!iter.MoveNext())
                                {
                                    throw new MalformedException("End of stream was reached while being escaped.");
                                }
                            }
                            else if (current == _wrapper)
                            {
                                queue.Enqueue(current);
                                escaped = false;
                            }
                            else
                            {
                                queue.Enqueue(current);
                            }
                        }
                        else if (current == _wrapper)
                        { // Escape the value and start parsing as such
                            queue.Enqueue(current);
                            escaped = true;
                        }
                        else
                        { // Append the new char
                            queue.Enqueue(current);
                        }
                        UpdateCurrentBreak(ref currentRowBreak, current);

                        if (!escaped)
                        {
                            if (EqualArrays(currentRowBreak, rowBreak))
                            {
                                if (queue.Count == 0 || queue.Count == rowBreak.Length)
                                { // If the builder is the same length as the row delimiter than we can skip this set because it is empty.
                                    queue = new Queue<char>();
                                    current = iter.Current;
                                    continue;
                                }
                                yield return new Queue<char>(queue.Take(queue.Count - rowBreak.Length));
                                queue = new Queue<char>();
                            }
                        }
                        current = iter.Current;
                    }
                    
                    queue.Enqueue(current);
                    if (!escaped || current == _wrapper) yield return queue;
                    else if (escaped) throw new MalformedException("End of stream was reached while being escaped.");
                }
            }
        }

        /// <summary>
        /// Helper method to check to see if two arrays are equal.
        /// </summary>
        /// <param name="arr1">The first array.</param>
        /// <param name="arr2">The second array to check against.</param>
        /// <returns>Will return if the arrays are equal.</returns>
        private bool EqualArrays(char[] arr1, char[] arr2)
        {
            var end = true;
            for (var j = 0; end && j < arr1.Length; ++j)
            {
                end = end && arr1[j] == arr2[j];
            }
            return end;
        }

        /// <summary>
        /// Helper method to act like a queue that has a limit and gets rid of old chars to check the row delimiter.
        /// </summary>
        /// <param name="rowBreak">The row break object we are updating.</param>
        /// <param name="newChar">The new char that needs to be included into the list.</param>
        private void UpdateCurrentBreak(ref char[] rowBreak, char newChar)
        {
            for (var j = 1; j < rowBreak.Length; ++j)
            {
                rowBreak[j - 1] = rowBreak[j];
            }
            rowBreak[rowBreak.Length - 1] = newChar;
        }
    }
}