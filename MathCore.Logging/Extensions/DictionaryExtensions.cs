// ReSharper disable CheckNamespace
namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> KeyValue, out TKey Key, out TValue Value)
        {
            Key = KeyValue.Key;
            Value = KeyValue.Value;
        }
    }
}
