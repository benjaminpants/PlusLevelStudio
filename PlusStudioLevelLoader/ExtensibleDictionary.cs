using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelLoader
{
    /// <summary>
    /// An ExtensibleDictionary is like a standard dictionary with string as the key, but additional dictionaries can be appeneded onto it temporarily.
    /// This is so that other mods (primarily Studio) can temporarily add content to the loader without needing to manually keep track of every added element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtensibleDictionary<T> : IDictionary<string, T>
    {
        public List<ExtensibleDictionaryExtension<T>> extends = new List<ExtensibleDictionaryExtension<T>>();

        protected Dictionary<string, T> internalDict = new Dictionary<string, T>();

        public ICollection<string> Keys => BuildInternalCombinedDictionary().Keys;

        public ICollection<T> Values => BuildInternalCombinedDictionary().Values;

        public int Count => BuildInternalCombinedDictionary().Count;

        public bool IsReadOnly => false;

        public T this[string key] { get => Get(key); set => Set(key, value); }

        public void AddExtensionIfNotPresent(ExtensibleDictionaryExtension<T> ext)
        {
            if (extends.Contains(ext)) return;
            extends.Add(ext);
        }

        public T Get(string key)
        {
            if (TryGetValue(key, out T v))
            {
                return v;
            }
            throw new KeyNotFoundException("Key " + key + " not found!");
        }

        public void Set(string key, T value)
        {
            internalDict[key] = value;
        }

        public void Add(string key, T value)
        {
            internalDict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            if (internalDict.ContainsKey(key)) return true;
            foreach (var item in extends)
            {
                if (item.dictionary.ContainsKey(key)) return true;
            }
            return false;
        }

        public bool Remove(string key)
        {
            return internalDict.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            if (internalDict.TryGetValue(key, out T v))
            {
                foreach (var item in extends)
                {
                    if (!item.canOverride) continue;
                    if (!item.dictionary.ContainsKey(key)) continue;
                    value = item.dictionary[key];
                    return true;
                }
                value = v;
                return true;
            }
            else
            {
                foreach (var item in extends)
                {
                    if (!item.dictionary.ContainsKey(key)) continue;
                    value = item.dictionary[key];
                    return true;
                }
            }
            value = default;
            return false;
        }

        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            extends.Clear();
            internalDict.Clear();
        }

        public bool Contains(KeyValuePair<string, T> itm)
        {
            throw new NotImplementedException();
            /*if (internalDict.TryGetValue(itm.Key, out T v))
            {
                foreach (var item in extends)
                {
                    if (!item.canOverride) continue;
                    if (!item.dictionary.ContainsKey(itm.Key)) continue;
                    return (item.dictionary[itm.Key] == itm.Value);
                }
                return (v == itm.Value);
            }
            foreach (var item in extends)
            {
                if (!item.canOverride) continue;
                if (!item.dictionary.ContainsKey(itm.Key)) continue;
                return (item.dictionary[itm.Key] == itm.Value);
            }
            return false;*/
        }

        private Dictionary<string, T> BuildInternalCombinedDictionary()
        {
            Dictionary<string, T> dict = new Dictionary<string, T>(internalDict);
            foreach (var item in extends)
            {
                if (item.canOverride)
                {
                    foreach (var kvp in item.dictionary)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    foreach (var kvp in item.dictionary)
                    {
                        if (!dict.ContainsKey(kvp.Key))
                        {
                            dict.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
            return dict;
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            Dictionary<string, T> comb = BuildInternalCombinedDictionary();
            int ind = arrayIndex;
            foreach (var item in comb)
            {
                array[ind] = item;
                ind++;
            }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return internalDict.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            if (extends.Count == 0) return internalDict.GetEnumerator();
            return BuildInternalCombinedDictionary().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    /// <summary>
    /// A class that represents an extension to an ExtensibleDictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtensibleDictionaryExtension<T>
    {
        protected Dictionary<string, T> _dictionary;
        protected bool _canOverride;

        public bool canOverride => _canOverride;
        public Dictionary<string, T> dictionary => _dictionary;

        public ExtensibleDictionaryExtension(Dictionary<string, T> dictionary, bool canOverride)
        {
            _dictionary = dictionary;
            _canOverride = canOverride;
        }

        public ExtensibleDictionaryExtension()
        {
            _dictionary = new Dictionary<string, T>();
            _canOverride = false;
        }
    }
}
