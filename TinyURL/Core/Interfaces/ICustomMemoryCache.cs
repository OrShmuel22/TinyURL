namespace TinyURL.Core.Interfaces
{
    public interface ICustomMemoryCache<T>
    {
        bool TryGetValue(string key, out T value);
        void Add(string key, T value);
        void Remove(string key);
        void Clear();

    }
}
