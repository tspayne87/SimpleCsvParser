using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Converters
{
  internal interface IPropertyConverter
  {
    object Convert(ReadOnlySpan<char> data, string doubleWrap, string singleWrap, bool hasDoubleWrapper);
  }
}
