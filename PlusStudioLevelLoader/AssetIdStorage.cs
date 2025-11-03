using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlusStudioLevelLoader
{
    public class AssetIdStorage<T> : IEnumerable where T : class
    {
        protected Dictionary<string, T> entries = new Dictionary<string, T>();

        protected List<AssetIdStorageModifier<T>> modifiers = new List<AssetIdStorageModifier<T>>();

        public void AddModifier(AssetIdStorageModifier<T> modifier)
        {
            modifiers.Add(modifier);
        }

        public bool ContainsKey(string id)
        {
            if (entries.ContainsKey(id)) return true;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].ContainsKey(id)) return true;
            }
            return false;
        }

        public T Get(string id)
        {
            if (entries.TryGetValue(id, out T value))
            {
                return value;
            }
            for (int i = 0; i < modifiers.Count; i++)
            {
                T returnedValue = modifiers[i].Get(id);
                if (returnedValue != null)
                {
                    return returnedValue;
                }
            }
            return null;
        }

        public void Add(string id, T value)
        {
            entries.Add(id, value);
        }

        public KeyValuePair<string, T>[] GetPairs()
        {
            List<KeyValuePair<string, T>> kvpList = new List<KeyValuePair<string, T>>();
            kvpList.AddRange(entries.ToArray());
            for (int i = 0; i < modifiers.Count; i++)
            {
                kvpList.AddRange(modifiers[i].GetEntries());
            }
            return kvpList.ToArray();
        }

        public T[] GetValues()
        {
            List<T> valueList = new List<T>();
            valueList.AddRange(entries.Values);
            for (int i = 0; i < modifiers.Count; i++)
            {
                valueList.AddRange(modifiers[i].GetEntries().Select(x => x.Value));
            }
            return valueList.ToArray();
        }

        public IEnumerator GetEnumerator()
        {
            return GetPairs().GetEnumerator();
        }

        public T this[string index]
        {
            get
            {
                return Get(index);
            }
        }
    }

    public abstract class AssetIdStorageModifier<T> where T : class
    {
        public abstract bool ContainsKey(string id);

        public abstract T Get(string id);

        public abstract List<KeyValuePair<string, T>> GetEntries();
    }
}
