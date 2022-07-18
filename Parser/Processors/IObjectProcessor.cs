using System;

namespace SimpleCsvParser.Processors
{
   /// <summary>
   /// The processor meant to handle each column being added to the object being created
   /// </summary>
   internal interface IObjectProcessor<T>
   {
     void AddColumn(ReadOnlySpan<char> str, bool hasWrapper, bool hasDoubleWrapper);

     bool IsEmpty();

     bool IsAColumnSet();

     T GetObject();

     void ClearObject();
   }
}