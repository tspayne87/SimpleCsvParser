using System.Collections.Generic;
using System;
using System.Linq;

namespace SimpleCsvParser.Processors
{
    /// <summary>
    /// The processor meant to handle each column being added to the object being created
    /// </summary>
    internal class ListStringProcessor : IObjectProcessor<IList<string>>
    {

        private string[] _result = default;
        private int _colIndex = 0;
        private int _colCount;
        /// <summary>
        /// Boolean to determine if a column has been set or not
        /// </summary>
        private bool _isAColumnSet = false;

        public ListStringProcessor(int numCols)
        {
            _result = new string[numCols];
            _colCount = numCols;
        }

        /// <inheritdoc />
        public void AddColumn(string str)
        {
            _result[_colIndex++] = str;
            _isAColumnSet = true;
        }

        /// <inheritdoc />
        public bool IsEmpty()
        {
            for (int i = 0; i < _colIndex; i++)
                if (!String.IsNullOrWhiteSpace(_result[i]))
                    return false;
            return true;
        }

        /// <inheritdoc />
        public bool IsAColumnSet()
        {
            return _isAColumnSet;
        }

        /// <inheritdoc />
        public IList<string> GetObject()
        {
            var returnable = new string[_colCount];
            _result.AsSpan().CopyTo(returnable);
            return returnable;
        }

        /// <inheritdoc />
        public void ClearObject()
        {
            _colIndex = 0;
            _isAColumnSet = false;
        }
    }
}