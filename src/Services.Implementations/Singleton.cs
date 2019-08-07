using System;

namespace Nexus.Link.Services.Implementations
{
    /// <summary>
    /// Support class for handling singleton instantiation challenge
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T: class
    {
        private readonly Func<T> _factory;
        private volatile T _instance;
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="factory">How to instantiate the only instance</param>
        public Singleton(Func<T> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// True if the singleton has not yet been instantiated
        /// </summary>
        public bool IsInstantiated => _instance != null;

        /// <summary>
        /// Get the only instance.
        /// </summary>
        /// <remarks>The instance is not created until the first call to this property.</remarks>
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