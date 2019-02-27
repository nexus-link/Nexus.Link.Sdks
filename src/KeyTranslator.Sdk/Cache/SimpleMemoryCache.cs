using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;

namespace Nexus.Link.KeyTranslator.Sdk.Cache
{
    /// <summary>
    /// Simple memory cache mechanism.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The type for the data that should be stored in the cache.</typeparam>
    internal class SimpleMemoryCache<TKey, TValue> : IDisposable where TValue : class
    {
        private readonly MemoryCache _cache;
        private readonly CacheItemPolicy _policy;

        /// <summary>
        /// Use <see cref="MemoryCache.Default"/> for caching objects of the current stored type..
        /// </summary>
        /// <param name="name">The name of the memory cache.</param>
        /// <param name="slidingExpiration">The maximum time span that we should cache items.</param>
        /// <param name="physicalMemoryLimitPercentage">The memory limit for the cache (in percent of available memory, 0-100).</param>
        public SimpleMemoryCache(string name, TimeSpan slidingExpiration, int physicalMemoryLimitPercentage)
        {
            if ((physicalMemoryLimitPercentage < 0) || (physicalMemoryLimitPercentage > 100))
            {
                throw new ArgumentOutOfRangeException(nameof(physicalMemoryLimitPercentage), "Value must be in the range 0-100.");
            }
            var cacheSettings = new NameValueCollection(3);
            cacheSettings.Add("physicalMemoryLimitPercentage", Convert.ToString(physicalMemoryLimitPercentage));
            cacheSettings.Add("pollingInterval", Convert.ToString("00:00:10"));
            _cache = new MemoryCache(name, cacheSettings);
            _policy = new CacheItemPolicy { SlidingExpiration = slidingExpiration,  };
        }

        public void Dispose()
        {
            _cache.ToList().ForEach(a =>
            {
                var value = a.Value as TValue;
                if (value != null) _cache.Remove(a.Key);
            });
        }

        /// <summary>
        /// Get an item with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to the cached item.</param>
        /// <returns>The found item or null.</returns>
        public TValue Get(TKey key)
        {
            var cacheItem = GetCacheItem(key);
            return (TValue) cacheItem?.Value;
        }

        public void Refresh(TKey key, TValue value)
        {
            Set(key, value);
        }

        public void Set(TKey key, TValue value)
        {
            _cache.Set(new CacheItem(key.ToString(), value), _policy);
        }
        private CacheItem GetCacheItem(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _cache.GetCacheItem(key.ToString());
        }
    }
}