using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Json5Core;

internal class Deserializer
{
    public Deserializer(Json5Parameters param)
    {
        _circobj = param.OverrideObjectHashCodeChecking ? new Dictionary<object, int>(10, ReferenceEqualityComparer.Default) : new Dictionary<object, int>();
        param.FixValues();
        _params = param.MakeCopy();
    }

    private Json5Parameters _params;
    private bool _usingglobals;
    private Dictionary<object, int> _circobj; // = new Dictionary<object, int>();
    private Dictionary<int, object> _cirrev = new Dictionary<int, object>();

    public T? ToObject<T>(string json)
    {
        Type t = typeof(T);
        object? o = ToObject(json, t);

        if (t.IsArray)
        {
            if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
            {
                Type? tt = t.GetElementType();
                object oo = Array.CreateInstance(tt, 0);
                return (T?)oo;
            }

            return (T?)o;
        }

        return (T?)o;
    }

    public object ToObject(string json)
    {
        return ToObject(json, null);
    }

    public object ToObject(string json, Type? type) => ToObject(json, type, null);

    public object ToObject(string json, Type? type, IList<string> warnings)
    {
        //_params.FixValues();
        Type? t = null;
        if (type != null && type.IsGenericType())
            t = Reflection.Instance.GetGenericTypeDefinition(type);
        _usingglobals = _params.UsingGlobalTypes;
        if (typeof(IDictionary).IsAssignableFrom(t) || typeof(List<>).IsAssignableFrom(t))
            _usingglobals = false;

        object? o = new JsonParser(json, true, warnings).Decode(type);
        if (o == null)
            return null;

        if (type != null)
        {
            if (type == typeof(DataSet))
                return CreateDataset(o as Dictionary<string, object>, null);
            if (type == typeof(DataTable))
                return CreateDataTable(o as Dictionary<string, object>, null);
        }

        switch (o)
        {
            case IDictionary when type != null && typeof(Dictionary<,>).IsAssignableFrom(t):
                return RootDictionary(o, type);
            case IDictionary:
                return ParseDictionary(o as Dictionary<string, object>, null, type, null);
            case List<object> list when type != null:
            {
                if (typeof(Dictionary<,>).IsAssignableFrom(t)) // kv format
                    return RootDictionary(list, type);
                if (t == typeof(List<>)) // deserialize to generic list
                    return RootList(list, type);
                if (type.IsArray)
                    return RootArray(list, type);
                if (type == typeof(Hashtable))
                    return RootHashTable(list);
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(HashSet<>))
                    return RootSet(list, type);
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISet<>))
                    return RootSet(list, type);
                break;
            }
            //if (type == null)
            case List<object> { Count: > 0 } list when list[0].GetType() == typeof(Dictionary<string, object>):
            {
                Dictionary<string, object> globals = new Dictionary<string, object>();
                List<object> op = [];
                // try to get $types 
                foreach (object? i in list)
                    op.Add(ParseDictionary((Dictionary<string, object>)i, globals, null, null));
                return op;
            }
            case List<object> list:
                return list.ToArray();
            default:
            {
                if (type != null && o.GetType() != type)
                    return ChangeType(o, type);
                break;
            }
        }

        return o;
    }

    private object RootSet(List<object> o, Type type)
    {
        Type elementType = type.GetGenericArguments()[0];
        Type concreteSetType;

        if (type.IsInterface && type.GetGenericTypeDefinition() == typeof(ISet<>))
        {
            concreteSetType = typeof(HashSet<>).MakeGenericType(elementType);
        }
        else
        {
            concreteSetType = type;
        }

        object set = Reflection.Instance.FastCreateInstance(concreteSetType);
        MethodInfo? addMethod = concreteSetType.GetMethod("Add");

        foreach (object item in o)
        {
            object val = ToObject(JsonSerializer.Serialize(item), elementType, new List<string>());
            addMethod?.Invoke(set, [val]);
        }

        return set;
    }


    #region [   p r i v a t e   m e t h o d s   ]

    private Hashtable RootHashTable(List<object> o)
    {
        Hashtable h = new Hashtable();

        foreach (Dictionary<string, object> values in o)
        {
            object key = values["k"];
            object val = values["v"];

            if (key is Dictionary<string, object> dictionary)
                key = ParseDictionary(dictionary, null, typeof(object), null);

            if (val is Dictionary<string, object> objects)
                val = ParseDictionary(objects, null, typeof(object), null);

            h.Add(key, val);
        }

        return h;
    }

    private object ChangeType(object value, Type conversionType)
    {
        if (conversionType == typeof(object))
            return value;

        if (conversionType == typeof(int))
        {
            string s = value as string;
            if (s == null)
                return (int)((long)value);
            if (_params.AutoConvertStringToNumbers)
                return Helper.CreateInteger(s, 0, s.Length);
            throw new Exception("AutoConvertStringToNumbers is disabled for converting string : " + value);
        }

        if (conversionType == typeof(long))
        {
            string s = value as string;
            if (s == null)
                return (long)value;
            if (_params.AutoConvertStringToNumbers)
                return Helper.CreateLong(s, 0, s.Length);
            throw new Exception("AutoConvertStringToNumbers is disabled for converting string : " + value);
        }

        if (conversionType == typeof(string))
            return (string)value;
        if (conversionType.IsEnum())
            return Helper.CreateEnum(conversionType, value);
        if (conversionType == typeof(DateTime))
            return Helper.CreateDateTime((string)value, _params.UseUTCDateTime);
        if (conversionType == typeof(DateTimeOffset))
            return Helper.CreateDateTimeOffset((string)value);
        if (Reflection.Instance.IsTypeRegistered(conversionType))
            return Reflection.Instance.CreateCustom((string)value, conversionType);

        // 8-30-2014 - James Brooks - Added code for nullable types.
        if (Helper.IsNullable(conversionType))
        {
            if (value == null)
                return value;
            conversionType = Helper.UnderlyingTypeOf(conversionType);
        }

        // 8-30-2014 - James Brooks - Nullable Guid is a special case so it was moved after the "IsNullable" check.
        if (conversionType == typeof(Guid))
            return Helper.CreateGuid((string)value);

        // 2016-04-02 - Enrico Padovani - proper conversion of byte[] back from string
        if (conversionType == typeof(byte[]))
            return Convert.FromBase64String((string)value);

        if (conversionType == typeof(TimeSpan))
            return new TimeSpan((long)value);

        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }

    private object RootList(object parse, Type type)
    {
        Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
        IList? o = (IList)Reflection.Instance.FastCreateList(type, ((IList)parse).Count);
        DoParseList((IList)parse, gtypes[0], o);
        return o;
    }

    private void DoParseList(IList parse, Type it, IList o)
    {
        Dictionary<string, object> globals = new Dictionary<string, object>();

        foreach (object? k in parse)
        {
            _usingglobals = false;
            object v = k;
            if (k is Dictionary<string, object> a)
                v = ParseDictionary(a, globals, it, null);
            else
                v = ChangeType(k, it);

            o.Add(v);
        }
    }

    private object RootArray(object parse, Type type)
    {
        Type it = type.GetElementType();
        IList? o = (IList)Reflection.Instance.FastCreateInstance(typeof(List<>).MakeGenericType(it));
        DoParseList((IList)parse, it, o);
        Array? array = Array.CreateInstance(it, o.Count);
        o.CopyTo(array, 0);
        return array;
    }

    private object? RootDictionary(object parse, Type type)
    {
        Type[] gtypes = Reflection.Instance.GetGenericArguments(type);
        Type t1 = null;
        Type t2 = null;
        bool dictionary = false;
        bool isSet = false;

        if (gtypes != null)
        {
            t1 = gtypes[0];

            if (gtypes.Length > 1)
            {
                t2 = gtypes[1];

                if (t2.IsGenericType)
                {
                    Type genericTypeDef = t2.GetGenericTypeDefinition();
                    List<Type> interfaces = t2.GetInterfaces().ToList();

                    if (genericTypeDef == typeof(Dictionary<,>) || (t2.IsClass && interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
                    {
                        dictionary = true;
                    }
                    else if (genericTypeDef == typeof(HashSet<>) || (t2.IsClass && interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>))))
                    {
                        isSet = true;
                    }
                }
            }
        }


        Type? arraytype = t2.GetElementType();
        if (parse is Dictionary<string, object> objects)
        {
            IDictionary o = (IDictionary)Reflection.Instance.FastCreateInstance(type);

            foreach (KeyValuePair<string, object> kv in objects)
            {
                object v;
                object k = ChangeType(kv.Key, t1);

                if (dictionary) // deserialize a dictionary
                    v = RootDictionary(kv.Value, t2);
                else if (isSet) // deserialize a set
                    v = CreateSet(kv.Value, t2);
                else if (kv.Value is Dictionary<string, object> value)
                    v = ParseDictionary(value, null, t2, null);

                else if (t2.IsArray && t2 != typeof(byte[]))
                    v = CreateArray((List<object>)kv.Value, t2, arraytype, null);

                else if (kv.Value is IList)
                    v = CreateGenericList((List<object>)kv.Value, t2, t1, null);

                else
                    v = ChangeType(kv.Value, t2);

                o.Add(k, v);
            }

            return o;
        }

        if (parse is List<object>)
            return CreateDictionary(parse as List<object>, type, gtypes, null);

        return null;
    }

    private object CreateSet(object value, Type setType)
    {
        Type elementType = setType.GetGenericArguments()[0];
        Type concreteSetType;

        if (setType.GetGenericTypeDefinition() == typeof(HashSet<>))
        {
            concreteSetType = setType;
        }
        else
        {
            concreteSetType = typeof(HashSet<>).MakeGenericType(elementType);
        }

        object set = Reflection.Instance.FastCreateInstance(concreteSetType);
        MethodInfo? addMethod = concreteSetType.GetMethod("Add");

        switch (value)
        {
            case IList<object> list:
            {
                foreach (object item in list)
                {
                    object convertedItem = ChangeType(item, elementType);
                    addMethod.Invoke(set, [convertedItem]);
                }

                break;
            }
            case Dictionary<string, object> dict:
            {
                foreach (object item in dict.Values)
                {
                    object convertedItem = ChangeType(item, elementType);
                    addMethod.Invoke(set, [convertedItem]);
                }

                break;
            }
        }

        return set;
    }


    internal object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object>? globaltypes, Type type, object? input)
    {
        if (type == typeof(NameValueCollection))
            return Helper.CreateNV(d);
        if (type == typeof(StringDictionary))
            return Helper.CreateSD(d);

        if (d.TryGetValue("$i", out object tn))
        {
            _cirrev.TryGetValue((int)(long)tn, out object? v);
            return v;
        }

        if (d.TryGetValue("$types", out tn))
        {
            _usingglobals = true;
            globaltypes ??= [];

            foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)tn)
            {
                globaltypes.Add((string)kv.Value, kv.Key);
            }
        }

        if (globaltypes != null)
            _usingglobals = true;

        bool found = d.TryGetValue("$type", out tn);

        switch (found)
        {
            case false when type == typeof(object):
                return d; // CreateDataset(d, globaltypes);
            case true:
            {
                if (_usingglobals)
                {
                    if (globaltypes != null && globaltypes.TryGetValue((string)tn, out object tname))
                        tn = tname;
                }

                type = Reflection.Instance.GetTypeFromCache((string)tn, _params.BadListTypeChecking);
                break;
            }
        }

        if (type == null)
            throw new Exception("Cannot determine type : " + tn);

        string typename = type.FullName;
        object o = input ?? (_params.ParametricConstructorOverride ? InternalHelpers.TryGetUninitializedObject(type) : Reflection.Instance.FastCreateInstance(type));

        if (_circobj.TryGetValue(o, out int circount) == false)
        {
            circount = _circobj.Count + 1;
            _circobj.Add(o, circount);
            _cirrev.Add(circount, o);
        }

        Dictionary<string, myPropInfo>? props = Reflection.Instance.Getproperties(type, typename, _params.ShowReadOnlyProperties);
        foreach (KeyValuePair<string, object> kv in d)
        {
            string? n = kv.Key;
            object? v = kv.Value;

            if (n == "$map")
            {
                ProcessMap(o, props, (Dictionary<string, object>)d[n]);
                continue;
            }

            if (props.TryGetValue(n, out myPropInfo pi) == false)
                if (props.TryGetValue(n.ToLowerInvariant(), out pi) == false)
                    continue;

            if (pi.CanWrite)
            {
                object oset = null;
                if (v != null)
                {
                    switch (pi.Type)
                    {
                        case myPropInfoType.Int:
                            oset = (int)Helper.AutoConv(v, _params);
                            break;
                        case myPropInfoType.Long:
                            oset = Helper.AutoConv(v, _params);
                            break;
                        case myPropInfoType.String:
                            oset = v.ToString();
                            break;
                        case myPropInfoType.Bool:
                            oset = Helper.BoolConv(v);
                            break;
                        case myPropInfoType.DateTime:
                            oset = Helper.CreateDateTime((string)v, _params.UseUTCDateTime);
                            break;
                        case myPropInfoType.Enum:
                            oset = Helper.CreateEnum(pi.pt, v);
                            break;
                        case myPropInfoType.Guid:
                            oset = Helper.CreateGuid((string)v);
                            break;

                        case myPropInfoType.Array:
                            if (!pi.IsValueType)
                                oset = CreateArray((List<object>)v, pi.pt, pi.bt, globaltypes);
                            // what about 'else'?
                            break;
                        case myPropInfoType.ByteArray:
                            oset = Convert.FromBase64String((string)v);
                            break;
                        case myPropInfoType.DataSet:
                            oset = CreateDataset((Dictionary<string, object>)v, globaltypes);
                            break;
                        case myPropInfoType.DataTable:
                            oset = CreateDataTable((Dictionary<string, object>)v, globaltypes);
                            break;
                        case myPropInfoType.HashSet:
                            if (v is List<object> localList)
                            {
                                oset = CreateGenericSet(localList, pi.pt, pi.bt, globaltypes);
                            }
                            break;
                        case myPropInfoType.Hashtable: // same case as Dictionary
                        case myPropInfoType.Dictionary:
                            oset = CreateDictionary((List<object>)v, pi.pt, pi.GenericTypes, globaltypes);
                            break;
                        case myPropInfoType.StringKeyDictionary:
                            oset = CreateStringKeyDictionary((Dictionary<string, object>)v, pi.pt, pi.GenericTypes, globaltypes);
                            break;
                        case myPropInfoType.NameValue:
                            oset = Helper.CreateNV((Dictionary<string, object>)v);
                            break;
                        case myPropInfoType.StringDictionary:
                            oset = Helper.CreateSD((Dictionary<string, object>)v);
                            break;
                        case myPropInfoType.Custom:
                            oset = Reflection.Instance.CreateCustom((string)v, pi.pt);
                            break;
                        default:
                        {
                            if (pi is { IsGenericType: true, IsValueType: false } && v is List<object> list)
                                oset = CreateGenericList(list, pi.pt, pi.bt, globaltypes);

                            else if ((pi.IsClass || pi.IsStruct || pi.IsInterface) && v is Dictionary<string, object> objects)
                                oset = ParseDictionary(objects, globaltypes, pi.pt, null); // pi.getter(o));

                            else if (v is List<object> list1)
                                oset = CreateArray(list1, pi.pt, typeof(object), globaltypes);

                            else if (pi.IsValueType)
                                oset = ChangeType(v, pi.changeType);

                            else
                                oset = v;
                        }
                            break;
                    }
                }

                o = pi.setter(o, oset);
            }
        }

        return o;
    }

    private static void ProcessMap(object obj, Dictionary<string, myPropInfo> props, Dictionary<string, object> dic)
    {
        foreach (KeyValuePair<string, object> kv in dic)
        {
            myPropInfo p = props[kv.Key];
            object o = p.getter(obj);
            // blacklist checking
            Type t = //Type.GetType((string)kv.Value);
                Reflection.Instance.GetTypeFromCache((string)kv.Value, true);
            if (t == typeof(Guid))
                p.setter(obj, Helper.CreateGuid((string)o));
        }
    }

    private object CreateArray(List<object> data, Type pt, Type? bt, Dictionary<string, object> globalTypes)
    {
        bt ??= typeof(object);

        Array col = Array.CreateInstance(bt, data.Count);
        Type? arraytype = bt.GetElementType();
        // create an array of objects
        for (int i = 0; i < data.Count; i++)
        {
            object? ob = data[i];
            if (ob == null)
            {
                continue;
            }

            if (ob is IDictionary)
                col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null), i);
            else if (ob is ICollection)
                col.SetValue(CreateArray((List<object>)ob, bt, arraytype, globalTypes), i);
            else
                col.SetValue(ChangeType(ob, bt), i);
        }

        return col;
    }
    
    private object CreateGenericSet(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
    {
        if (pt != typeof(object))
        {
            object col = Reflection.Instance.FastCreateInstance(pt);
            
            MethodInfo? addMethod = pt.GetMethod("Add");
            Type? it = Reflection.Instance.GetGenericArguments(pt)[0];

            foreach (object ob in data)
            {
                object item;
                switch (ob)
                {
                    case IDictionary:
                        item = ParseDictionary((Dictionary<string, object>)ob, globalTypes, it, null);
                        break;
                    case List<object> list when bt.IsGenericType():
                        item = list;
                        break;
                    case List<object> list:
                        item = list.ToArray();
                        break;
                    default:
                        item = ChangeType(ob, it);
                        break;
                }

                addMethod?.Invoke(col, [item]);
            }

            return col;
        }
        
        return new HashSet<object>(data);
    }


    private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
    {
        if (pt != typeof(object))
        {
            IList col = (IList)Reflection.Instance.FastCreateList(pt, data.Count);
            Type? it = Reflection.Instance.GetGenericArguments(pt)[0]; // pt.GetGenericArguments()[0];
            // create an array of objects
            foreach (object ob in data)
            {
                switch (ob)
                {
                    case IDictionary:
                        col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, it, null));
                        break;
                    case List<object> list when bt.IsGenericType():
                        col.Add(list); //).ToArray());
                        break;
                    case List<object> list:
                        col.Add(list.ToArray());
                        break;
                    default:
                        col.Add(ChangeType(ob, it));
                        break;
                }
            }

            return col;
        }

        return data;
    }

    private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
    {
        IDictionary? col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
        Type arraytype = null;
        Type t2 = null;
        if (types != null)
            t2 = types[1];

        Type generictype = null;
        Type[]? ga = Reflection.Instance.GetGenericArguments(t2); // t2.GetGenericArguments();
        if (ga.Length > 0)
            generictype = ga[0];
        arraytype = t2.GetElementType();

        foreach (KeyValuePair<string, object> values in reader)
        {
            string? key = values.Key;
            object val;

            if (values.Value is Dictionary<string, object> value)
                val = ParseDictionary(value, globalTypes, t2, null);

            else if (types != null && t2.IsArray)
            {
                val = values.Value is Array ? values.Value : CreateArray((List<object>)values.Value, t2, arraytype, globalTypes);
            }
            else if (values.Value is IList)
                val = CreateGenericList((List<object>)values.Value, t2, generictype, globalTypes);

            else
                val = ChangeType(values.Value, t2);

            col.Add(key, val);
        }

        return col;
    }

    private object CreateDictionary(List<object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
    {
        IDictionary col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
        Type t1 = null;
        Type t2 = null;
        Type generictype = null;
        if (types != null)
        {
            t1 = types[0];
            t2 = types[1];
        }

        Type arraytype = t2;
        if (t2 != null)
        {
            Type[]? ga = Reflection.Instance.GetGenericArguments(t2); // t2.GetGenericArguments();
            if (ga.Length > 0)
                generictype = ga[0];
            arraytype = t2.GetElementType();
        }

        bool root = typeof(IDictionary).IsAssignableFrom(t2);

        foreach (Dictionary<string, object> values in reader)
        {
            object key = values["k"];
            object val = values["v"];

            if (key is Dictionary<string, object> objects)
                key = ParseDictionary(objects, globalTypes, t1, null);
            else
                key = ChangeType(key, t1);

            if (root)
                val = RootDictionary(val, t2);

            else if (val is Dictionary<string, object> dictionary)
                val = ParseDictionary(dictionary, globalTypes, t2, null);

            else if (types != null && t2.IsArray)
                val = CreateArray((List<object>)val, t2, arraytype, globalTypes);

            else if (val is IList)
                val = CreateGenericList((List<object>)val, t2, generictype, globalTypes);

            else
                val = ChangeType(val, t2);

            col.Add(key, val);
        }

        return col;
    }


    private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
    {
        DataSet ds = new DataSet();
        ds.EnforceConstraints = false;
        ds.BeginInit();

        // read dataset schema here
        object? schema = reader["$schema"];

        if (schema is string s)
        {
            TextReader tr = new StringReader(s);
            ds.ReadXmlSchema(tr);
        }
        else
        {
            DatasetSchema ms = (DatasetSchema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(DatasetSchema), null);
            ds.DataSetName = ms.Name;
            for (int i = 0; i < ms.Info.Count; i += 3)
            {
                if (ds.Tables.Contains(ms.Info[i]) == false)
                    ds.Tables.Add(ms.Info[i]);
                // blacklist checking
                Type? t = //Type.GetType(ms.Info[i + 2]);
                    Reflection.Instance.GetTypeFromCache(ms.Info[i + 2], true);
                ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], t);
            }
        }

        foreach (KeyValuePair<string, object> pair in reader)
        {
            if (pair.Key is "$type" or "$schema") continue;

            List<object> rows = (List<object>)pair.Value;
            if (rows == null) continue;

            DataTable dt = ds.Tables[pair.Key];
            ReadDataTable(rows, dt);
        }

        ds.EndInit();

        return ds;
    }

    private void ReadDataTable(List<object> rows, DataTable dt)
    {
        dt.BeginInit();
        dt.BeginLoadData();
        List<int> guidcols = [];
        List<int> datecol = [];
        List<int> bytearraycol = [];

        foreach (DataColumn c in dt.Columns)
        {
            if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
                guidcols.Add(c.Ordinal);
            if (_params.UseUTCDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
                datecol.Add(c.Ordinal);
            if (c.DataType == typeof(byte[]))
                bytearraycol.Add(c.Ordinal);
        }

        foreach (List<object> row in rows)
        {
            //object[] v = row.ToArray(); //new object[row.Count];
            //row.CopyTo(v, 0);
            foreach (int i in guidcols)
            {
                string s = (string)row[i];
                if (s is { Length: < 36 })
                    row[i] = new Guid(Convert.FromBase64String(s));
            }

            foreach (int i in bytearraycol)
            {
                string s = (string)row[i];
                if (s != null)
                    row[i] = Convert.FromBase64String(s);
            }

            if (_params.UseUTCDateTime)
            {
                foreach (int i in datecol)
                {
                    string s = (string)row[i];
                    if (s != null)
                        row[i] = Helper.CreateDateTime(s, _params.UseUTCDateTime);
                }
            }

            dt.Rows.Add(row.ToArray());
        }

        dt.EndLoadData();
        dt.EndInit();
    }

    DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
    {
        DataTable? dt = new DataTable();

        // read dataset schema here
        object? schema = reader["$schema"];

        if (schema is string)
        {
            TextReader tr = new StringReader((string)schema);
            dt.ReadXmlSchema(tr);
        }
        else
        {
            DatasetSchema? ms = (DatasetSchema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(DatasetSchema), null);
            dt.TableName = ms.Info[0];
            for (int i = 0; i < ms.Info.Count; i += 3)
            {
                // blacklist checking
                Type? t = //Type.GetType(ms.Info[i + 2]);
                    Reflection.Instance.GetTypeFromCache(ms.Info[i + 2], true);
                dt.Columns.Add(ms.Info[i + 1], t);
            }
        }

        foreach (KeyValuePair<string, object> pair in reader)
        {
            if (pair.Key == "$type" || pair.Key == "$schema")
                continue;

            List<object>? rows = (List<object>)pair.Value;
            if (rows == null)
                continue;

            if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                continue;

            ReadDataTable(rows, dt);
        }

        return dt;
    }

    #endregion
}