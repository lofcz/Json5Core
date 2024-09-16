using System;
using System.Collections;
using System.Collections.Generic;

namespace Json5Core;

public static class Json5
    {
        /// <summary>
        /// Globally set-able parameters for controlling the serializer
        /// </summary>
        public static Json5Parameters Parameters = new Json5Parameters
        {
            UseExtensions = false
        };
        
        /// <summary>
        /// Create a formatted json string (beautified) from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonPretty(object obj)
        {
            string s = ToJson(obj, Parameters); // use default params

            return Beautify(s);
        }
        
        /// <summary>
        /// Create a formatted json string (beautified) from an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ToJsonPretty(object obj, Json5Parameters param)
        {
            string s = ToJson(obj, param);

            return Beautify(s, param.FormatterIndentSpaces);
        }
        
        /// <summary>
        /// Create a json representation for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            return ToJson(obj, Parameters);
        }
        
        /// <summary>
        /// Serialize given object.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="formatting">Formatting to use</param>
        /// <returns></returns>
        public static string Serialize(object? obj, JsonFormatting formatting = JsonFormatting.None)
        {
            return ToJson(obj, Parameters, formatting);
        }

        /// <summary>
        /// Serialize given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <param name="formatting">Formatting to use</param>
        /// <returns></returns>
        public static string Serialize(object? obj, Json5Parameters param, JsonFormatting formatting = JsonFormatting.None)
        {
            return ToJson(obj, param, formatting);
        }

        /// <summary>
        /// Create a json representation for an object with parameter override on this call
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string ToJson(object? obj, Json5Parameters param, JsonFormatting formatting = JsonFormatting.None)
        {
            param.FixValues();
            param = param.MakeCopy();
            Type? t = null;

            if (obj == null)
                return "null";

            if (obj.GetType().IsGenericType())
                t = Reflection.Instance.GetGenericTypeDefinition(obj.GetType());
            if (typeof(IDictionary).IsAssignableFrom(t) || typeof(List<>).IsAssignableFrom(t))
                param.UsingGlobalTypes = false;

            // FEATURE : enable extensions when you can deserialize anon types
            if (param.EnableAnonymousTypes) { param.UseExtensions = false; param.UsingGlobalTypes = false; }
            string data = new JSONSerializer(param).ConvertToJSON(obj);

            return formatting is JsonFormatting.Intended ? Beautify(data) : data;
        }
        
        /// <summary>
        /// Parse a json string and generate a Dictionary&lt;string,object&gt; or List&lt;object&gt; structure
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Parse(string json, IList<string> warnings)
        {
            return new JsonParser(json, true, warnings).Decode(null);
        }
        
        /// <summary>
        /// Parse a json string and generate a Dictionary&lt;string,object&gt; or List&lt;object&gt; structure
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Parse(string json) => Parse(json, null);
        
        /// <summary>
        /// Create a .net4 dynamic object from the json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(string json)
        {
            return new DynamicJson(json);
        }
        
        /// <summary>
        /// Create a typed generic object from the json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            return new Deserializer(Parameters).ToObject<T>(json);
        }
        
        /// <summary>
        /// Create a typed generic object from the json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json)
        {
            return new Deserializer(Parameters).ToObject<T>(json);
        }
        
        /// <summary>
        /// Create a typed generic object from the json with parameter override on this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json, Json5Parameters param)
        {
            return new Deserializer(param).ToObject<T>(json);
        }
        
        /// <summary>
        /// Create an object from the json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object ToObject(string json)
        {
            return new Deserializer(Parameters).ToObject(json, null);
        }
        
        /// <summary>
        /// Create an object from the json with parameter override on this call
        /// </summary>
        /// <param name="json"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object ToObject(string json, Json5Parameters param)
        {
            return new Deserializer(param).ToObject(json, null);
        }
        
        /// <summary>
        /// Create an object of type from the json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToObject(string json, Type type)
        {
            return new Deserializer(Parameters).ToObject(json, type);
        }
        
        /// <summary>
        /// Create an object of type from the json with parameter override on this call
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public static object ToObject(string json, Type type, Json5Parameters par)
        {
            return new Deserializer(par).ToObject(json, type);
        }
        
        /// <summary>
        /// Fill a given object with the json represenation
        /// </summary>
        /// <param name="input"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object FillObject(object input, string json, IList<string> warnings)
        {
            Dictionary<string, object> ht = new JsonParser(json, true, warnings).Decode(input.GetType()) as Dictionary<string, object>;
            return ht == null ? null : new Deserializer(Parameters).ParseDictionary(ht, null, input.GetType(), input);
        }
        
        /// <summary>
        /// Fill a given object with the json represenation
        /// </summary>
        /// <param name="input"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object FillObject(object input, string json) => FillObject(input, json, null);
        
        /// <summary>
        /// Deep copy an object i.e. clone to a new object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DeepCopy(object obj)
        {
            return new Deserializer(Parameters).ToObject(ToJson(obj));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T obj)
        {
            return new Deserializer(Parameters).ToObject<T>(ToJson(obj));
        }

        /// <summary>
        /// Create a human-readable string from the json 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Beautify(string input)
        {
            string? i = new string(' ', Json5.Parameters.FormatterIndentSpaces);
            return Formatter.PrettyPrint(input, i);
        }
        
        /// <summary>
        /// Create a human-readable string from the json with specified indent spaces
        /// </summary>
        /// <param name="input"></param>
        /// <param name="spaces"></param>
        /// <returns></returns>
        public static string Beautify(string input, byte spaces)
        {
            string i = new string(' ', spaces);
            return Formatter.PrettyPrint(input, i);
        }
        
        /// <summary>
        /// Register custom type handlers for your own types not natively handled by Json5Core
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serializer"></param>
        /// <param name="deserializer"></param>
        public static void RegisterCustomType(Type type, Reflection.Serialize serializer, Reflection.Deserialize deserializer)
        {
            Reflection.Instance.RegisterCustomType(type, serializer, deserializer);
        }
        
        /// <summary>
        /// Clear the internal reflection cache so you can start from new (you will loose performance)
        /// </summary>
        public static void ClearReflectionCache()
        {
            Reflection.Instance.ClearReflectionCache();
        }
    }