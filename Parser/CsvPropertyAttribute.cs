using System;

namespace Parser
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CsvPropertyAttribute : Attribute
    {
        public string Header { get; set; }
        public int RowIndex { get; set; }

        public CsvPropertyAttribute(string header)
        {
            Header = header;
        }

        public CsvPropertyAttribute(int rowIndex)
        {
            RowIndex = rowIndex;
        }
    }
}