using System;

namespace SimpleCsvParser.Options
{
   /// <summary>
   /// Options for the csv stream reader that will load a basic csv file
   /// </summary>
   public class CsvStreamReaderOptions
   {
      /// <summary>
      /// The delimiter that is used to seperate data values.
      /// </summary>
      public string ColumnDelimiter { get; set; } = ",";

      /// <summary>
      /// The escape char that is used when the column delimiter is part of the data value.
      /// </summary>
      public char? EscapeChar { get; set; } = '"';

      /// <summary>
      /// The row delimiter that should be used when breaking up the rows.
      /// </summary>
      /// <value></value>
      public string RowDelimiter { get; set; } = Environment.NewLine;

      /// <summary>
      /// If empty rows need to be removed from the result.
      /// </summary>
      public bool RemoveEmptyEntries { get; set; } = false;

      /// <summary>
      /// The row index where the data starts in the file.
      /// </summary>
      /// <value></value>
      public int StartRow { get; set; } = 0;
   }
}