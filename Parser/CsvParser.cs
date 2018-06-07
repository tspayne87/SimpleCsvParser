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

        public CsvParser()
            : this(new CsvParserOptions()) { }

        public CsvParser(CsvParserOptions options)
        {
            _options = options;
        }

        public List<TModel> Parse<TModel>(string csvString)
            where TModel : new()
        {
            var list = csvString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> headers = null;
            var rows = new List<List<string>>();
            var lineNumber = 0;
            foreach (var line in list)
            {
                if (_options.ParseHeaders && headers == null) {
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

        public List<TModel> ParseFile<TModel>(string path)
            where TModel : new()
        {

            using (var file = new StreamReader(path))
            {
                string line;

                List<string> headers = null;
                var rows = new List<List<string>>();
                var lineNumber = 0;
                while((line = file.ReadLine()) != null)
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
        }

        private List<TModel> ProcessRows<TModel>(List<string> headers, List<List<string>> rows)
            where TModel : new()
        {
            var props = typeof(TModel)
                .GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(CsvPropertyAttribute)))
                .Select(x => {
                    var attribute = (CsvPropertyAttribute)x.GetCustomAttributes(true).FirstOrDefault(y => y is CsvPropertyAttribute);
                    if (attribute == null) return new KeyValuePair<int, PropertyInfo>(-1, x);
                    else if (string.IsNullOrEmpty(attribute.Header)) return new KeyValuePair<int, PropertyInfo>(attribute.RowIndex, x);
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
                        if (prop.Value.PropertyType.IsEnum)
                        {
                            if (string.IsNullOrEmpty(row[prop.Key]) && _options.AllowDefaults)
                            {
                                prop.Value.SetValue(item, Activator.CreateInstance(prop.Value.PropertyType));
                            }
                            else
                            {
                                var e = Enum.Parse(prop.Value.PropertyType, row[prop.Key]);
                                prop.Value.SetValue(item, e);
                            }
                        }
                        else if (prop.Value.PropertyType.IsPrimitive)
                        {
                            if (string.IsNullOrEmpty(row[prop.Key]) && _options.AllowDefaults)
                            {
                                prop.Value.SetValue(item, Activator.CreateInstance(prop.Value.PropertyType));
                            }
                            else
                            {
                                prop.Value.SetValue(item, Convert.ChangeType(row[prop.Key], prop.Value.PropertyType));
                            }
                        }
                    }
                }
                results.Add(item);
            }
            return results;
        }

        private List<string> ProcessLine(string line, int lineNumber)
        {
            var inWrapper = false;

            var data = string.Empty;
            var result = new List<string>();
            for (var i = 0; i < line.Length; ++i)
            {
                if (i == 0)
                {
                    if (line[i] == _options.Wrapper) inWrapper = true;
                    else if (line[i] == _options.Delimiter) result.Add(data);
                    else data += line[i];
                }
                else if (i + 1 == line.Length)
                {
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
                {
                    if (inWrapper)
                    {
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
