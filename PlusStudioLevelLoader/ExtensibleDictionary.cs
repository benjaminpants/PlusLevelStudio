using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelLoader
{
    /// <summary>
    /// An ExtensibleDictionary is like a standard dictionary with string as the key, but additional dictionaries can be appeneded onto it temporarily.
    /// This is so that other mods (primarily Studio) can temporarily add content to the loader without needing to manually keep track of every added element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtensibleDictionary<T> where T : class
    {
        public List<ExtensibleDictionaryExtension<T>> extends = new List<ExtensibleDictionaryExtension<T>>();

        protected Dictionary<string, T> internalDict = new Dictionary<string, T>();

        public T Get(string key)
        {
            if (internalDict.TryGetValue(key, out T value))
            {
                foreach (var item in extends)
                {
                    if (!item.canOverride) continue;
                    if (!item.dictionary.ContainsKey(key)) continue;
                    return item.dictionary[key];
                }
                return value;
            }
            else
            {
                foreach (var item in extends)
                {
                    if (!item.canOverride) continue;
                    if (!item.dictionary.ContainsKey(key)) continue;
                    return item.dictionary[key];
                }
            }
            return null;
        }
    }


    /// <summary>
    /// A class that represents an extension to an ExtensibleDictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtensibleDictionaryExtension<T> where T : class
    {
        protected Dictionary<string, T> _dictionary = new Dictionary<string, T>();
        protected bool _canOverride = false;

        public bool canOverride => canOverride;
        public Dictionary<string, T> dictionary => _dictionary;

        public ExtensibleDictionaryExtension(Dictionary<string, T> dictionary, bool canOverride)
        {
            _dictionary = dictionary;
            _canOverride = canOverride;
        }
    }
}
