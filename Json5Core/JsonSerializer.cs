using System;
using System.Collections;
using System.Collections.Generic;
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER)
using System.Data;
#endif
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Specialized;

namespace Json5Core
{
    internal sealed class JSONSerializer
    {
        private StringBuilder _output = new StringBuilder();
        private int _before;
        private int _MAX_DEPTH = 20;
        int _current_depth = 0;
        private Dictionary<string, int> _globalTypes = new Dictionary<string, int>();
        private Dictionary<object, int> _cirobj;
        private JsonParameters _params;
        private bool _useEscapedUnicode = false;

        internal JSONSerializer(JsonParameters param)
        {
            _cirobj = param.OverrideObjectHashCodeChecking ? new Dictionary<object, int>(10, ReferenceEqualityComparer.Default) : new Dictionary<object, int>();
            _params = param;
            _useEscapedUnicode = _params.UseEscapedUnicode;
            _MAX_DEPTH = _params.SerializerMaxDepth;
        }

        internal string ConvertToJSON(object obj)
        {
            WriteValue(obj);

            if (_params.UsingGlobalTypes && _globalTypes != null && _globalTypes.Count > 0)
            {
                StringBuilder? sb = new StringBuilder();
                sb.Append("\"$types\":{");
                bool pendingSeparator = false;
                foreach (KeyValuePair<string, int> kv in _globalTypes)
                {
                    if (pendingSeparator) sb.Append(',');
                    pendingSeparator = true;
                    sb.Append('\"');
                    sb.Append(kv.Key);
                    sb.Append("\":\"");
                    sb.Append(kv.Value);
                    sb.Append('\"');
                }
                sb.Append("},");
                _output.Insert(_before, sb.ToString());
            }
            return _output.ToString();
        }

        private void WriteValue(object obj)
        {
            switch (obj)
            {
                case null:
                case DBNull:
                    _output.Append("null");
                    break;
                case string:
                case char:
                    WriteString(obj.ToString());
                    break;
                case Guid guid:
                    WriteGuid(guid);
                    break;
                case bool b:
                    _output.Append(b ? "true" : "false"); // conform to standard
                    break;
                case int:
                case long:
                case byte:
                case short:
                case sbyte:
                case ushort:
                case uint:
                case ulong:
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                    break;
                case decimal decimalNumber when decimalNumber == 0.0m:
                    _output.Append((decimal.GetBits(decimalNumber)[3] & -2147483648) != -2147483648 ? "0" : "-0");
                    break;
                case decimal decimalNumber:
                    _output.Append(decimalNumber.ToString(NumberFormatInfo.InvariantInfo));
                    break;
                case double d1:
                {
                    double d = d1;
                    if (double.IsNaN(d))
                        _output.Append("NaN");
                    else if (double.IsInfinity(d))
                        _output.Append(d > 0 ? "Infinity" : "-Infinity");
                    else if (d == 0)
                        _output.Append((BitConverter.GetBytes(d)[BitConverter.IsLittleEndian ? 7 : 0] & 128) == 0 ? "0" : "-0");
                    else
                        _output.Append(d.ToString("R", NumberFormatInfo.InvariantInfo));
                    break;
                }
                case float f:
                {
                    float d = f;
                    if (float.IsNaN(d))
                        _output.Append("NaN");
                    else if (float.IsInfinity(d))
                        _output.Append(d > 0 ? "Infinity" : "-Infinity");
                    else if (d == 0)
                        _output.Append((BitConverter.GetBytes(d)[BitConverter.IsLittleEndian ? 3 : 0] & 128) == 0 ? "0" : "-0");
                    else
                        _output.Append(d.ToString("R", NumberFormatInfo.InvariantInfo));
                    break;
                }
                case DateTime time:
                    WriteDateTime(time);
                    break;
                case DateTimeOffset offset:
                    WriteDateTimeOffset(offset);
                    break;
                case TimeSpan span:
                    _output.Append(span.Ticks);
                    break;
                default:
                {
                    switch (_params.KVStyleStringDictionary)
                    {
                        case false when
                            obj is IEnumerable<KeyValuePair<string, object>> pairs:
                            WriteStringDictionary(pairs);
                            break;
                        case false when obj is IDictionary dictionary &&
                                        dictionary.GetType().IsGenericType() && Reflection.Instance.GetGenericArguments(dictionary.GetType())[0] == typeof(string):
                            WriteStringDictionary(dictionary);
                            break;
                        default:
                            switch (obj)
                            {
                                case IDictionary dictionary1:
                                    WriteDictionary(dictionary1);
                                    break;
                                case DataSet set:
                                    WriteDataset(set);
                                    break;
                                case DataTable table:
                                    WriteDataTable(table);
                                    break;
                                case byte[] bytes:
                                    WriteBytes(bytes);
                                    break;
                                case StringDictionary stringDictionary:
                                    WriteSD(stringDictionary);
                                    break;
                                case NameValueCollection collection:
                                    WriteNV(collection);
                                    break;
                                case Array array:
                                    WriteArrayRanked(array);
                                    break;
                                case IEnumerable enumerable:
                                    WriteArray(enumerable);
                                    break;
                                case Enum @enum:
                                    WriteEnum(@enum);
                                    break;
                                default:
                                {
                                    if (Reflection.Instance.IsTypeRegistered(obj.GetType()))
                                        WriteCustom(obj);

                                    else
                                        WriteObject(obj);
                                    break;
                                }
                            }

                            break;
                    }

                    break;
                }
            }
        }

        private void WriteDateTimeOffset(DateTimeOffset d)
        {
            DateTime dt = _params.UseUTCDateTime ? d.UtcDateTime : d.DateTime;

            write_date_value(dt);

            long ticks = dt.Ticks % TimeSpan.TicksPerSecond;
            _output.Append('.');
            _output.Append(ticks.ToString("0000000", NumberFormatInfo.InvariantInfo));

            if (_params.UseUTCDateTime)
                _output.Append('Z');
            else
            {
                _output.Append(d.Offset.Hours > 0 ? '+' : '-');
                _output.Append(d.Offset.Hours.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(d.Offset.Minutes.ToString("00", NumberFormatInfo.InvariantInfo));
            }

            _output.Append('\"');
        }

        private void WriteNV(NameValueCollection nameValueCollection)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (string key in nameValueCollection)
            {
                if (_params.SerializeNullValues == false && (nameValueCollection[key] == null))
                {
                }
                else
                {
                    if (pendingSeparator) _output.Append(',');
                    WritePair(_params.SerializeToLowerCaseNames ? key.ToLowerInvariant() : key, nameValueCollection[key]);
                    pendingSeparator = true;
                }
            }
            _output.Append('}');
        }

        private void WriteSD(StringDictionary stringDictionary)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in stringDictionary)
            {
                if (_params.SerializeNullValues == false && (entry.Value == null))
                {
                }
                else
                {
                    if (pendingSeparator) _output.Append(',');

                    string k = (string)entry.Key;
                    WritePair(_params.SerializeToLowerCaseNames ? k.ToLowerInvariant() : k, entry.Value);
                    pendingSeparator = true;
                }
            }
            _output.Append('}');
        }

        private void WriteCustom(object obj)
        {
            Reflection.Serialize s;
            Reflection.Instance._customSerializer.TryGetValue(obj.GetType(), out s);
            WriteStringFast(s(obj));
        }

        private void WriteEnum(Enum e)
        {
            // FEATURE : optimize enum write
            if (_params.UseValuesOfEnums)
                WriteValue(Convert.ToInt32(e));
            else
                WriteStringFast(e.ToString());
        }

        private void WriteGuid(Guid g)
        {
            if (_params.UseFastGuid == false)
                WriteStringFast(g.ToString());
            else
                WriteBytes(g.ToByteArray());
        }

        private void WriteBytes(byte[] bytes)
        {
            WriteStringFast(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
        }

        private void WriteDateTime(DateTime dateTime)
        {
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            DateTime dt = dateTime;
            if (_params.UseUTCDateTime)
                dt = dateTime.ToUniversalTime();

            write_date_value(dt);

            if (_params.DateTimeMilliseconds)
            {
                _output.Append('.');
                _output.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));
            }

            if (_params.UseUTCDateTime)
                _output.Append('Z');

            _output.Append('\"');
        }

        private void write_date_value(DateTime dt)
        {
            _output.Append('\"');
            _output.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
            _output.Append('-');
            _output.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append('-');
            _output.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append('T'); // strict ISO date compliance 
            _output.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));
        }


        private DatasetSchema GetSchema(DataTable ds)
        {
            if (ds == null) return null;

            DatasetSchema m = new DatasetSchema();
            m.Info = new List<string>();
            m.Name = ds.TableName;

            foreach (DataColumn c in ds.Columns)
            {
                m.Info.Add(ds.TableName);
                m.Info.Add(c.ColumnName);
                m.Info.Add(_params.FullyQualifiedDataSetSchema ? c.DataType.AssemblyQualifiedName : c.DataType.ToString());
            }
            // FEATURE : serialize relations and constraints here

            return m;
        }

        private DatasetSchema GetSchema(DataSet ds)
        {
            if (ds == null) return null;

            DatasetSchema m = new DatasetSchema();
            m.Info = new List<string>();
            m.Name = ds.DataSetName;

            foreach (DataTable t in ds.Tables)
            {
                foreach (DataColumn c in t.Columns)
                {
                    m.Info.Add(t.TableName);
                    m.Info.Add(c.ColumnName);
                    m.Info.Add(_params.FullyQualifiedDataSetSchema ? c.DataType.AssemblyQualifiedName : c.DataType.ToString());
                }
            }
            // FEATURE : serialize relations and constraints here

            return m;
        }

        private string GetXmlSchema(DataTable dt)
        {
            using (StringWriter? writer = new StringWriter())
            {
                dt.WriteXmlSchema(writer);
                return dt.ToString();
            }
        }

        private void WriteDataset(DataSet ds)
        {
            _output.Append('{');
            if (_params.UseExtensions)
            {
                WritePair("$schema", _params.UseOptimizedDatasetSchema ? (object)GetSchema(ds) : ds.GetXmlSchema());
                _output.Append(',');
            }
            bool tablesep = false;
            foreach (DataTable table in ds.Tables)
            {
                if (tablesep) _output.Append(',');
                tablesep = true;
                WriteDataTableData(table);
            }
            // end dataset
            _output.Append('}');
        }

        private void WriteDataTableData(DataTable table)
        {
            _output.Append('\"');
            _output.Append(table.TableName);
            _output.Append("\":[");
            DataColumnCollection cols = table.Columns;
            bool rowseparator = false;
            foreach (DataRow row in table.Rows)
            {   
                if (rowseparator) _output.Append(',');
                rowseparator = true;
                _output.Append('[');

                bool pendingSeperator = false;
                foreach (DataColumn column in cols)
                {
                    if (pendingSeperator) _output.Append(',');
                    WriteValue(row[column]);
                    pendingSeperator = true;
                }
                _output.Append(']');
            }

            _output.Append(']');
        }

        void WriteDataTable(DataTable dt)
        {
            this._output.Append('{');
            if (_params.UseExtensions)
            {
                this.WritePair("$schema", _params.UseOptimizedDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
                this._output.Append(',');
            }

            WriteDataTableData(dt);

            // end datatable
            this._output.Append('}');
        }

        bool _TypesWritten = false;
        private void WriteObject(object obj)
        {
            int i = 0;
            if (_cirobj.TryGetValue(obj, out i) == false)
                _cirobj.Add(obj, _cirobj.Count + 1);
            else
            {
                if (_current_depth > 0 && _params.InlineCircularReferences == false)
                {
                    //_circular = true;
                    _output.Append("{\"$i\":");
                    _output.Append(i.ToString());
                    _output.Append('}');
                    return;
                }
            }
            if (_params.UsingGlobalTypes == false)
                _output.Append('{');
            else
            {
                if (_TypesWritten == false)
                {
                    _output.Append('{');
                    _before = _output.Length;
                    //_output = new StringBuilder();
                }
                else
                    _output.Append('{');
            }
            _TypesWritten = true;
            _current_depth++;
            if (_current_depth > _MAX_DEPTH)
                throw new Exception("Serializer encountered maximum depth of " + _MAX_DEPTH);


            Dictionary<string, string> map = new Dictionary<string, string>();
            Type t = obj.GetType();
            bool append = false;
            if (_params.UseExtensions)
            {
                if (_params.UsingGlobalTypes == false)
                    WritePairFast("$type", Reflection.Instance.GetTypeAssemblyName(t));
                else
                {
                    string ct = Reflection.Instance.GetTypeAssemblyName(t);
                    if (_globalTypes.TryGetValue(ct, out int dt) == false)
                    {
                        dt = _globalTypes.Count + 1;
                        _globalTypes.Add(ct, dt);
                    }
                    WritePairFast("$type", dt.ToString());
                }
                append = true;
            }

            Getters[] g = Reflection.Instance.GetGetters(t, /*_params.ShowReadOnlyProperties,*/ _params.IgnoreAttributes);
            int c = g.Length;
            for (int ii = 0; ii < c; ii++)
            {
                Getters p = g[ii];
                if (_params.ShowReadOnlyProperties == false && p.ReadOnly)
                    continue;
                object o = p.Getter(obj);
                if (_params.SerializeNullValues == false && (o == null || o is DBNull))
                {
                    //append = false;
                }
                else
                {
                    if (append)
                        _output.Append(',');
                    if (p.memberName != null)
                        WritePair(p.memberName, o);
                    else if (_params.SerializeToLowerCaseNames)
                        WritePair(p.lcName, o);
                    else
                        WritePair(p.Name, o);
                    if (o != null && _params.UseExtensions)
                    {
                        Type tt = o.GetType();
                        if (tt == typeof(object))
                            map.Add(p.Name, tt.ToString());
                    }
                    append = true;
                }
            }
            if (map.Count > 0 && _params.UseExtensions)
            {
                _output.Append(",\"$map\":");
                WriteStringDictionary(map);
            }
            _output.Append('}');
            _current_depth--;
        }

        private void WritePairFast(string name, string value)
        {
            WriteStringFast(name);

            _output.Append(':');

            WriteStringFast(value);
        }

        private void WritePair(string name, object value)
        {
            WriteString(name);

            _output.Append(':');

            WriteValue(value);
        }

        private void WriteArray(IEnumerable array)
        {
            _output.Append('[');

            bool pendingSeperator = false;

            foreach (object obj in array)
            {
                if (pendingSeperator) _output.Append(',');

                WriteValue(obj);

                pendingSeperator = true;
            }
            _output.Append(']');
        }

        private void WriteArrayRanked(Array array)
        {
            if (array.Rank == 1)
                WriteArray(array);
            else
            {
                // FIXx : use getlength 
                //var x = array.GetLength(0);
                //var y = array.GetLength(1);

                _output.Append('[');

                bool pendingSeperator = false;

                foreach (object obj in array)
                {
                    if (pendingSeperator) _output.Append(',');

                    WriteValue(obj);

                    pendingSeperator = true;
                }
                _output.Append(']');
            }
        }

        private void WriteStringDictionary(IDictionary dic)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (_params.SerializeNullValues == false && (entry.Value == null))
                {
                }
                else
                {
                    if (pendingSeparator) _output.Append(',');

                    string k = (string)entry.Key;
                    WritePair(_params.SerializeToLowerCaseNames ? k.ToLowerInvariant() : k, entry.Value);
                    pendingSeparator = true;
                }
            }
            _output.Append('}');
        }

        private void WriteStringDictionary(IEnumerable<KeyValuePair<string, object>> dic)
        {
            _output.Append('{');
            bool pendingSeparator = false;
            foreach (KeyValuePair<string, object> entry in dic)
            {
                if (_params.SerializeNullValues == false && (entry.Value == null))
                {
                }
                else
                {
                    if (pendingSeparator) _output.Append(',');
                    string k = entry.Key;

                    WritePair(_params.SerializeToLowerCaseNames ? k.ToLowerInvariant() : k, entry.Value);
                    pendingSeparator = true;
                }
            }
            _output.Append('}');
        }

        private void WriteDictionary(IDictionary dic)
        {
            _output.Append('[');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (pendingSeparator) _output.Append(',');
                _output.Append('{');
                WritePair("k", entry.Key);
                _output.Append(',');
                WritePair("v", entry.Value);
                _output.Append('}');

                pendingSeparator = true;
            }
            _output.Append(']');
        }

        private void WriteStringFast(string s)
        {
            _output.Append('\"');
            _output.Append(s);
            _output.Append('\"');
        }

        private void WriteString(string s)
        {
            _output.Append('\"');

            int runIndex = -1;
            int l = s.Length;
            for (int index = 0; index < l; ++index)
            {
                char c = s[index];

                if (_useEscapedUnicode)
                {
                    if ((c >= ' ' && c < 128 && c != '\"' && c != '\\')
                        || (CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter or UnicodeCategory.LetterNumber or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.DecimalDigitNumber or UnicodeCategory.ConnectorPunctuation)
                        || (c is '$' or '_' or '\u200C' or '\u200D'))
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }
                else
                {
                    if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\' && c != '\0' && c != '\f' && c != '\v' && c != '\b' && c != '\a')// && c != ':' && c!=',')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }

                if (runIndex != -1)
                {
                    _output.Append(s, runIndex, index - runIndex);
                    runIndex = -1;
                }

                switch (c)
                {
                    case '\t': _output.Append('\\').Append('t'); break;
                    case '\r': _output.Append('\\').Append('r'); break;
                    case '\n': _output.Append('\\').Append('n'); break;
                    case '"':
                    case '\\': _output.Append('\\'); _output.Append(c); break;
                    case '\0': _output.Append('\\').Append('0'); break;
                    case '\f': _output.Append('\\').Append('f'); break;
                    case '\v': _output.Append('\\').Append('v'); break;
                    case '\b': _output.Append('\\').Append('b'); break;
                    case '\a': _output.Append('\\').Append('a'); break;
                    default:
                        if (_useEscapedUnicode
                            && (CharUnicodeInfo.GetUnicodeCategory(c) is not UnicodeCategory.UppercaseLetter and not UnicodeCategory.LowercaseLetter and not UnicodeCategory.TitlecaseLetter and not UnicodeCategory.ModifierLetter and not UnicodeCategory.OtherLetter and not UnicodeCategory.LetterNumber and not UnicodeCategory.NonSpacingMark and not UnicodeCategory.SpacingCombiningMark and not UnicodeCategory.DecimalDigitNumber and not UnicodeCategory.ConnectorPunctuation)
                            && (c is not '$' and not '_' and not '\u200C' and not '\u200D'))
                        {
                            _output.Append("\\u");
                            _output.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
                        }
                        else
                            _output.Append(c);

                        break;
                }
            }

            if (runIndex != -1)
                _output.Append(s, runIndex, s.Length - runIndex);

            _output.Append('\"');
        }
    }
}
