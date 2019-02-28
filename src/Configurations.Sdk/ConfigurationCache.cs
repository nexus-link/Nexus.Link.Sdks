using System;
using System.Configuration;
using System.Runtime.Caching;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
namespace Nexus.Link.Configurations.Sdk
{
    /// <summary>
    /// A cache of configurations for different tenants.
    /// </summary>
    public class ConfigurationCache
    {
        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly Guid _uniqueId = Guid.NewGuid();

        /// <summary>
        /// Get the configuration for a specific tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public ILeverConfiguration Get(Tenant tenant)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            var cacheKey = GetCacheKey(tenant);
            lock (_cache)
            {
                return _cache[cacheKey] as ILeverConfiguration;
            }
        }

        /// <summary>
        /// Add a configuration for a specific tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="configuration"></param>
        public void Add(Tenant tenant, ILeverConfiguration configuration)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNull(configuration, nameof(configuration));

            var cacheKey = GetCacheKey(tenant);
            lock (_cache)
            {
                _cache.Set(cacheKey, configuration, GetCacheExpiration());
            }
        }

        private string GetCacheKey(Tenant tenant)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            return $"{tenant.Organization}|{tenant.Environment}{_uniqueId}";
        }

        private static DateTimeOffset GetCacheExpiration()
        {
            var cacheTime = ConfigurationManager.AppSettings["LeverServiceConfiguration.Cache.TimeSpan"];
            if (!string.IsNullOrWhiteSpace(cacheTime))
            {
                try
                {
                    return DateTimeOffset.Now.Add(TimeSpan.Parse(cacheTime));
                }
                catch
                {
                    // ignored
                }
            }

            try
            {
                switch (FulcrumApplication.Setup.RunTimeLevel)
                {
                    case RunTimeLevelEnum.Development:
                    case RunTimeLevelEnum.Test:
                    case RunTimeLevelEnum.ProductionSimulation:
                        return DateTimeOffset.Now.AddMinutes(1);

                    case RunTimeLevelEnum.Production:
                        return DateTimeOffset.Now.AddMinutes(15);
                }
            }
            catch
            {
                // ignored
            }

            return DateTimeOffset.Now.AddMinutes(15);
        }
    }
}
