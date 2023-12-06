using Parser.Converters;
using System;

namespace Parser
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  internal class CsvPropertyConverterAttribute : Attribute
  {
    public IPropertyConverter Converter { get; private set; }

    public CsvPropertyConverterAttribute(Type type)
    {
      if (!type.IsAssignableTo(typeof(IPropertyConverter)))
        throw new ArgumentException("Could not create a property converter, please implement IPropertyConverter");
      Converter = Activator.CreateInstance(type) as IPropertyConverter;
    }
  }
}
