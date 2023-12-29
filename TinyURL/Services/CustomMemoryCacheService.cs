using System.Collections.Concurrent;
using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;

namespace TinyURL.Services.Caching
{
    /// <summary>
    /// Custom memory cache with LRU (Least Recently Used) eviction policy.
    /// </summary>
    /// <typeparam name="T">The type of the values in the cache.</typeparam>
    public class CustomMemoryCacheService<T> : ICustomMemoryCache<T>
    {
        private class Node
        {
            public string Key { get; set; }
            public T Value { get; set; }
            public Node? Previous { get; set; }
            public Node? Next { get; set; }
        }

        private readonly int _capacity;
        private readonly ConcurrentDictionary<string, Node> _cacheMap;
        private readonly Node _head;
        private readonly Node _tail;
        private readonly object _lock = new object();

        public CustomMemoryCacheService(MemoryCacheSetting settings)
        {
            _capacity = settings.Capacity > 0 ? settings.Capacity : throw new ArgumentException("Capacity must be greater than zero.", nameof(settings.Capacity));
            _cacheMap = new ConcurrentDictionary<string, Node>();
            _head = new Node();
            _tail = new Node();
            _head.Next = _tail;
            _tail.Previous = _head;
        }

        public void Add(string key, T value)
        {
            ValidateKey(key);

            lock (_lock)
            {
                if (!_cacheMap.TryGetValue(key, out Node node))
                {
                    node = new Node { Key = key, Value = value };
                    _cacheMap[key] = node;
                    AddToHead(node);
                    if (_cacheMap.Count > _capacity) RemoveLRUItem();
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _cacheMap.Clear();
                _head.Next = _tail;
                _tail.Previous = _head;
            }
        }

        public void Remove(string key)
        {
            ValidateKey(key);

            lock (_lock)
            {
                if (_cacheMap.TryRemove(key, out Node node)) RemoveNode(node);
            }
        }

        public bool TryGetValue(string key, out T value)
        {
            lock (_lock)
            {
                if (_cacheMap.TryGetValue(key, out Node node))
                {
                    MoveToHead(node);
                    value = node.Value;
                    return true;
                }
                value = default;
                return false;
            }
        }

        private void AddToHead(Node node)
        {
            node.Previous = _head;
            node.Next = _head.Next;
            _head.Next.Previous = node;
            _head.Next = node;
        }

        private void RemoveNode(Node node)
        {
            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;
        }

        private void MoveToHead(Node node)
        {
            RemoveNode(node);
            AddToHead(node);
        }

        private void RemoveLRUItem()
        {
            Node lru = _tail.Previous;
            RemoveNode(lru);
            _cacheMap.TryRemove(lru.Key, out _);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }
    }
}
