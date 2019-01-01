using System.Collections.Generic;
using System.IO;

namespace SimpleCsvParser.Readers
{
    /// <summary>
    /// The char reader for streams to read in chars on demand.
    /// </summary>
    internal class CharReader
    {
        /// <summary>
        /// The stream that we will be reading from.
        /// </summary>
        private readonly Stream _stream;
        /// <summary>
        /// The reader that needs to be used for reading from the stream.
        /// </summary>
        private readonly StreamReader _reader;

        /// <summary>
        /// Constructor to build out the char reader
        /// </summary>
        /// <param name="stream">The stream that needs to be read from.</param>
        public CharReader(Stream stream)
        {
            _stream = stream;
            _reader = new StreamReader(_stream);
        }

        /// <summary>
        /// Will turn this reader into an enumerable for use in reading one char at a time and read the file in blocks.
        /// </summary>
        /// <returns>Returns an IEnuemrable of chars.</returns>
        public IEnumerable<char> AsEnumerable()
        {
            // Restart the reading of this enumerable
            _stream.Position = 0;
            _reader.DiscardBufferedData();

            char[] buffer = new char[256 * 1024]; // Create a buffer to store the characters loaded from the stream.
            int blockIndex;
            while((blockIndex = _reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < blockIndex; ++i) {
                    yield return buffer[i];
                }
            }
        }
    }
}