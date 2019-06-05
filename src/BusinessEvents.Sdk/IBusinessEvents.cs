﻿using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Health.Model;

namespace Nexus.Link.BusinessEvents.Sdk
{
    public interface IBusinessEvents : IResourceHealth
    {
        /// <summary>
        /// Publish a business event based on version of event and publisher client.
        /// </summary>
        /// <param name="entityName">The name of the entity</param>
        /// <param name="eventName">The name of the event</param>
        /// <param name="majorVersion">The major version</param>
        /// <param name="minorVersion">The minor version</param>
        /// <param name="clientName">The publisher client</param>
        /// <param name="eventBody">The event body to be sent.</param>
        Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody);

        /// <summary>
        /// Test publish an event to see if it meets the requirements (types and translations)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="event"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="clientName"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task<PublicationTestResult> TestBenchPublish(string entity, string @event, int major, int minor, string clientName, JToken payload);
    }
}