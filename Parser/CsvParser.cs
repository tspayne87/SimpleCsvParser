using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Parser
{
    public class CsvParser : IDisposable
    {
        private readonly CsvParserOptions _options;

        /// <summary>
        /// Constructor that is meant to setup basic options for the parser.
        /// </summary>
        public CsvParser()
            : this(new CsvParserOptions()) { }

        /// <summary>
        /// Constructor to deal with custom options.
        /// </summary>
        /// <param name="options">Options that should be used by the parser.</param>
        public CsvParser(CsvParserOptions options)
        {
            _options = options;
        }
        
        /// <summary>
        /// Method is meant to parse a csv string into a list of objects.
        /// </summary>
        /// <param name="csvString">The csv string we need to parse.</param>
        /// <typeparam name="TModel">The model we need to create for each of the rows in the csv string.</typeparam>
        /// <returns>Will return a list of models that were asked for.</returns>
        public List<TModel> Parse<TModel>(string csvString)
            where TModel : new()
        {
            var list = csvString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> headers = null;
            var rows = new List<List<string>>();
            var lineNumber = 0;
            foreach (var line in list)
            {
                if (_options.ParseHeaders && headers == null)
                {
                    headers = ProcessLine(line, lineNumber);
                }
                else if (headers != null)
                {
                    var data = ProcessLine(line, lineNumber);
                    if (data.Count != headers.Count) throw new MalformedException($"Line {lineNumber} has {data.Count} but should have {headers.Count}.");
                    rows.Add(data);
                }
                else if (_options.ParseHeaders)
                {
                    var data = ProcessLine(line, lineNumber);
                    if (rows.Count > 0 && rows[rows.Count - 1].Count != data.Count) throw new MalformedException($"Line {lineNumber} has {data.Count} but should have {rows[rows.Count - 1].Count}.");
                    rows.Add(data);
                }
                lineNumber++;
            }
            if (_options.RemoveEmptyEntries) rows = rows.Where(x => x.Where(y => string.IsNullOrEmpty(y)).Count() != headers.Count).ToList();
            return ProcessRows<TModel>(headers, rows);
        }

        /// <summary>
        /// Method is meant to open and parse the file to create a list of objects.
        /// </summary>
        /// <param name="path">The full path to the csv file.</param>
        /// <typeparam name="TModel">The models that we need to generate from the csv file.</typeparam>
        /// <returns>Will return a list of models that were asked for.</returns>
        public List<TModel> ParseFile<TModel>(string path)
            where TModel : new()
        {
            using (var file = new StreamReader(path))
            {
                string line;
                var lines = new List<string>();
                while((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                return Parse<TModel>(string.Join("\n", lines));
            }
        }

        /// <summary>
        /// Method is meant to parse the rows collected from the csv file and turn the into the objects requested.
        /// </summary>
        /// <param name="headers">The headers we should be using to parse on based on attributes.</param>
        /// <param name="rows">The data rows we need to process from the create the objects.</param>
        /// <typeparam name="TModel">The models we will be generating.</typeparam>
        /// <returns>A list of models that was requested.</returns>
        private List<TModel> ProcessRows<TModel>(List<string> headers, List<List<string>> rows)
            where TModel : new()
        {
            if (headers == null && _options.ParseHeaders) throw new ArgumentException("No headers were found.");
            var props = typeof(TModel)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(CsvPropertyAttribute)))
                .Select(x => {
                    var attribute = (CsvPropertyAttribute)x.GetCustomAttributes(true).FirstOrDefault(y => y is CsvPropertyAttribute);
                    if (attribute == null) return new KeyValuePair<int, PropertyInfo>(-1, x);
                    else if (string.IsNullOrEmpty(attribute.Header)) return new KeyValuePair<int, PropertyInfo>(attribute.ColIndex, x);
                    else return new KeyValuePair<int, PropertyInfo>(headers.IndexOf(attribute.Header), x);
                });

            var results = new List<TModel>();
            foreach (var row in rows)
            {
                var item = new TModel();
                foreach (var prop in props)
                {
                    if (prop.Key > -1)
                    {
                        if (prop.Value.PropertyType.IsGenericType && prop.Value.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            if ((string.IsNullOrEmpty(row[prop.Key]) || row[prop.Key] == "null") && _options.AllowDefaults)
                            {
                                prop.Value.SetValue(item, null);
                            }
                            else
                            {
                                prop.Value.SetValue(item, GetConvertedValue(Nullable.GetUnderlyingType(prop.Value.PropertyType), row[prop.Key]));
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(row[prop.Key]) && _options.AllowDefaults)
                            {
                                prop.Value.SetValue(item, GetDefaultValue(prop.Value.PropertyType));
                            }
                            else
                            {
                                prop.Value.SetValue(item, GetConvertedValue(prop.Value.PropertyType, row[prop.Key]));
                            }
                        }
                    }
                }
                results.Add(item);
            }
            return results;
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

        /// <summary>
        /// Method is meant to process one line of the csv file and convert it to a string array to be processed later.
        /// </summary>
        /// <param name="line">The line we are processing.</param>
        /// <param name="lineNumber">The current line number in the csv file we are processing on, this is mainly used for exceptions.</param>
        /// <returns>Will return a list of string gathered from the csv row.</returns>
        private List<string> ProcessLine(string line, int lineNumber)
        {
            var inWrapper = false;

            var data = string.Empty;
            var result = new List<string>();
            for (var i = 0; i < line.Length; ++i)
            {
                if (i == 0)
                { // Deal with edge case 1: We are at the start of the string
                    if (line[i] == _options.Wrapper) inWrapper = true;
                    else if (line[i] == _options.Delimiter) result.Add(data);
                    else data += line[i];
                }
                else if (i + 1 == line.Length)
                { // Deal with edge case 2: We are at the end of the string.
                    if (inWrapper && line[i] != _options.Wrapper)
                    {
                        throw new MalformedException($"Line {lineNumber} does not end its data wrapper.");
                    }
                    else if(line[i] == _options.Delimiter)
                    {
                        result.Add(data);
                        result.Add(string.Empty);
                    }
                    else
                    {
                        result.Add(inWrapper ? data : data + line[i]);
                    }
                }
                else
                { // Deal with normal case: we are in the middle of the string.
                    if (inWrapper)
                    { // If we are in a wrapper we should process it as such
                        if (line[i] == _options.Wrapper && line[i + 1] == _options.Wrapper)
                        {
                            data += _options.Wrapper;
                            i++;
                        }
                        else if (line[i] == _options.Wrapper && line[i + 1] == _options.Delimiter)
                        {
                            result.Add(data);
                            data = string.Empty;
                            inWrapper = false;
                            i++;
                            if (i + 1 == line.Length) result.Add(data);
                        }
                        else
                        {
                            data += line[i];
                        }
                    }
                    else if (line[i] == _options.Wrapper)
                    {
                        inWrapper = true;
                    }
                    else if (line[i] == _options.Delimiter)
                    {
                        result.Add(data);
                        data = string.Empty;
                    }
                    else
                    {
                        data += line[i];
                    }
                }
            }
            return result;
        }

        #region IDisposable Support
        // Do not have disposable support yet but will use this in the future.
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CsvParser() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
