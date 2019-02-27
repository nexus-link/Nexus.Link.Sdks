using System;
using Nexus.Link.KeyTranslator.Sdk.Models;

namespace Nexus.Link.KeyTranslator.Sdk.Cache
{
    internal class TranslateResponseCache
    {
        private SimpleMemoryCache<string, TranslateResponse> _cache;

        private readonly int _cacheMinutes;
        private readonly int _cachePhysicalMemoryLimitPercentage;

        public TranslateResponseCache(int cacheMinutes, int cachePhysicalMemoryLimitPercentage)
        {
            _cacheMinutes= cacheMinutes;
            _cachePhysicalMemoryLimitPercentage = cachePhysicalMemoryLimitPercentage;
            ResetCache();
        }

        public void ResetCache()
        {
            _cache?.Dispose();
            _cache = new SimpleMemoryCache<string, TranslateResponse>("KeyTranslator.ClientUtilities", TimeSpan.FromMinutes(_cacheMinutes), _cachePhysicalMemoryLimitPercentage);
        }

        public void Add(string key, TranslateResponse response)
        {
            _cache.Set(key, response);
        }

        public void Refresh(string key, TranslateResponse response)
        {
            _cache.Refresh(key, response);
        }

        public TranslateResponse Get(string key)
        {
            return _cache.Get(key);
        }
    }
}
