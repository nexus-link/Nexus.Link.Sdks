using System;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Configurations.Sdk
{
    // TODO: This is the same as Xlent.Lever.Fundamentals.Facade.WebApi.LeverConfiguration; could live in Libraries.Core perhaps?

    /// <summary>
    /// Contains information about a tenant specific configuration for a specific service
    /// </summary>
    [Serializable] // This makes it possible to pack and unpack this type of data in the execution context.
    public class LeverConfiguration : ILeverConfiguration
    {
        private static readonly string Namespace = typeof(LeverConfiguration).Namespace;

        /// <inheritdoc />
        public Tenant Tenant { get; }
        private readonly string _service;
        /// <summary>
        /// This is where all the configuration values are stored
        /// </summary>
        public JObject JObject { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tenant">The tenant that this configuration is for.</param>
        /// <param name="service">The service that this configuration is for.</param>
        /// <param name="jObject">The configuration values.</param>
        public LeverConfiguration(Tenant tenant, string service, JObject jObject)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireValidated(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(service, nameof(service));
            InternalContract.RequireNotNull(jObject, nameof(jObject));

            Tenant = tenant;
            _service = service;
            JObject = jObject;
        }

        /// <inheritdoc />
        public T Value<T>(object key)
        {
            InternalContract.RequireNotNull(key, nameof(key));
            var token = JObject[key];
            return token == null ? default : token.ToObject<T>();
        }

        /// <inheritdoc />
        public T MandatoryValue<T>(object key)
        {
            InternalContract.RequireNotNull(key, nameof(key));
            var value = Value<T>(key);
            FulcrumAssert.IsNotNull(value, $"{Namespace}: B2D26B5A-AAAD-4283-A4D7-D96232311AC6", $"Mandatory key ({key}) not found in service {_service} configuration for tenant {Tenant}.");
#if DEBUG
            var type = typeof(T);
            InternalContract.Require(IsNullable(type), $"The type {type.FullName} was expected to be nullable. Use the method Value<T>() instead of MandatoryValue<T>().");
#endif
            return value;
        }

        private static bool IsNullable(Type type)
        {
            return type.IsClass || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
