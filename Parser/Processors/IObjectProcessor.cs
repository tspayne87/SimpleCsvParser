using System;

namespace SimpleCsvParser.Processors
{
   /// <summary>
   /// The processor meant to handle each column being added to the object being created
   /// </summary>
   internal interface IObjectProcessor<T>
      where T : class, new()
   {
     void AddColumn(ReadOnlySpan<char> str);

     void AddColumn(ReadOnlySpan<char> str, ReadOnlySpan<char> overflow);

     bool IsEmpty();

     bool IsAColumnSet();

     T GetObject();

     void ClearObject();
   }
}