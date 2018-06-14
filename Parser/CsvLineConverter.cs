using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleCsvParser
{
    internal class CsvLineConverter<TModel>
        where TModel : class, new()
    {
        /// <summary>
        /// Special options needed to determine what should be done with the parser.
        /// </summary>
        private readonly CsvStreamOptions _options;
        /// <summary>
        /// The collection of props built from the model bound to this class.
        /// </summary>
        private readonly IEnumerable<KeyValuePair<int, PropertyInfo>> _props;

        /// <summary>
        /// Constructor to cache various pieces of the parser to deal with converting to the model given in the generic class.
        /// </summary>
        /// <param name="options">The options this converter should use when building out the objects.</param>
        /// <param name="headers">The headers that should be used to build the objects.</param>
        public CsvLineConverter(CsvStreamOptions options, List<string> headers)
        {
            if (headers == null && options.ParseHeaders) throw new ArgumentException("No headers were found.");

            _options = options;
            _props = typeof(TModel)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(CsvPropertyAttribute)))
                .Select(x => {
                    var attribute = (CsvPropertyAttribute)x.GetCustomAttributes(true).FirstOrDefault(y => y is CsvPropertyAttribute);
                    if (attribute == null) return new KeyValuePair<int, PropertyInfo>(-1, x);
                    else if (string.IsNullOrEmpty(attribute.Header)) return new KeyValuePair<int, PropertyInfo>(attribute.ColIndex, x);
                    else return headers == null ?
                        new KeyValuePair<int, PropertyInfo>(-1, x)
                        : new KeyValuePair<int, PropertyInfo>(headers.IndexOf(attribute.Header), x);
                });
        }

        /// <summary>
        /// Method is meant to parse the rows collected from the csv file and turn the into the objects requested.
        /// </summary>
        /// <param name="headers">The headers we should be using to parse on based on attributes.</param>
        /// <param name="rows">The data rows we need to process from the create the objects.</param>
        /// <typeparam name="TModel">The models we will be generating.</typeparam>
        /// <returns>A list of models that was requested.</returns>
        public TModel Process(List<string> row, long lineNumber)
        {
            var result = new TModel();
            foreach (var prop in _props)
            {
                if (prop.Key > -1 && prop.Key < row.Count)
                {
                    if (prop.Value.PropertyType.IsGenericType && prop.Value.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if ((string.IsNullOrEmpty(row[prop.Key]) || row[prop.Key] == "null"))
                        {
                            if (string.IsNullOrEmpty(row[prop.Key]) && !_options.AllowDefaults) throw new MalformedException($"Default value in line {lineNumber} does not contain a value.");
                            prop.Value.SetValue(result, null);
                        }
                        else
                        {
                            prop.Value.SetValue(result, GetConvertedValue(Nullable.GetUnderlyingType(prop.Value.PropertyType), row[prop.Key]));
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(row[prop.Key]))
                        {
                            if (!_options.AllowDefaults) throw new MalformedException($"Default value in line {lineNumber} does not contain a value.");
                            prop.Value.SetValue(result, GetDefaultValue(prop.Value.PropertyType));
                        }
                        else
                        {
                            prop.Value.SetValue(result, GetConvertedValue(prop.Value.PropertyType, row[prop.Key]));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Method is meant to convert the string value into the strict type that csharp expects on the model.
        /// </summary>
        /// <param name="type">The type that the property is using.</param>
        /// <param name="item">The item that needs to be converted to the type above.</param>
        /// <returns>Will return an object that is the converted type.</returns>
        private object GetConvertedValue(Type type, string item)
        {
            if (type.IsEnum) return Enum.Parse(type, item);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return item;
                case TypeCode.DateTime:
                    return DateTime.Parse(item);
                case TypeCode.Object:
                case TypeCode.DBNull:
                    throw new ArgumentException($"{type.Name} cannot convert value.");
                default:
                    return Convert.ChangeType(item, type);
            }
        }

        /// <summary>
        /// Method is meant to get the default value for the specific type.
        /// </summary>
        /// <param name="type">The type we need to get a default for.</param>
        /// <returns>The default object we need for the type given.</returns>
        private object GetDefaultValue(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return string.Empty;
                case TypeCode.DateTime:
                    return DateTime.MinValue;
                case TypeCode.Object:
                case TypeCode.DBNull:
                    throw new ArgumentException($"{type.Name} cannot generate default value.");
                default:
                    return Activator.CreateInstance(type);
            }
        }
    }
}