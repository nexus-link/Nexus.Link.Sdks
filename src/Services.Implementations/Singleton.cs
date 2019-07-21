using System;

namespace Nexus.Link.Services.Implementations
{
    public class Singleton<T> where T: class
    {
        private readonly Func<T> _factory;
        private volatile T _instance;
        private readonly object _lock = new object();

        public Singleton(Func<T> factory)
        {
            _factory = factory;
        }

        public bool IsInstantiated => _instance != null;

        public T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock)
                {
                    if (_instance != null) return _instance;
                    _instance = _factory();
                }

                return _instance;
            }
        }
    }
}