using System;

namespace Parser
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CsvPropertyAttribute : Attribute
    {
        /// <summary>
        /// The header the attribute is attached to should be bound with.
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// The column index the attribute is attached to should be bound with.
        /// </summary>
        public int ColIndex { get; set; }

        /// <summary>
        /// Constructor is meant to bind this property to a specific header.
        /// </summary>
        /// <param name="header">The header needed to be bound to, this is case-sensitive</param>
        public CsvPropertyAttribute(string header)
        {
            Header = header;
        }

        /// <summary>
        /// Constructor is meant to be bound to the column index specified.
        /// </summary>
        /// <param name="colIndex">The column index that should be used for this property.</param>
        public CsvPropertyAttribute(int colIndex)
        {
            ColIndex = colIndex;
        }
    }
}