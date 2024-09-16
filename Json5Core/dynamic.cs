using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Json5Core
{
    internal class DynamicJson : DynamicObject, IEnumerable
    {
        private IDictionary<string, object> _dictionary { get; set; }
        private List<object> _list { get; set; }

        public DynamicJson(string json)
        {
            object parse = Json.Parse(json);

            if (parse is IDictionary<string, object> objects)
                _dictionary = objects;
            else
                _list = (List<object>)parse;
        }

        private DynamicJson(object dictionary)
        {
            if (dictionary is IDictionary<string, object> objects)
                _dictionary = objects;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dictionary.Keys.ToList();
        }

        public override bool TryGetIndex(GetIndexBinder binder, Object[] indexes, out Object result)
        {
            object index = indexes[0];
            result = index is int i ? _list[i] : _dictionary[(string) index]; 
            if (result is IDictionary<string, object> objects)
                result = new DynamicJson(objects);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dictionary.TryGetValue(binder.Name, out result) == false)
                if (_dictionary.TryGetValue(binder.Name.ToLowerInvariant(), out result) == false)
                    return false;// throw new Exception("property not found " + binder.Name);

            switch (result)
            {
                case IDictionary<string, object> objects:
                    result = new DynamicJson(objects);
                    break;
                case List<object> objects:
                {
                    List<object> list = new List<object>();
                    foreach (object item in objects)
                    {
                        list.Add(item is IDictionary<string, object> dictionary ? new DynamicJson(dictionary) : item);
                    }
                    result = list;
                    break;
                }
            }

            return _dictionary.ContainsKey(binder.Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach(object o in _list)
            {
                yield return new DynamicJson(o as IDictionary<string, object>);
            }
        }
    }
}
