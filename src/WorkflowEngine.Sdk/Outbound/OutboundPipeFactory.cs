using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace Nexus.Link.WorkflowEngine.Sdk.Outbound
{
    /// <summary>
    /// A factory class to create delegating handlers for outgoing HTTP requests.
    /// </summary>
    public static class OutboundPipeFactory
    {

        /// <summary>
        /// Creates handlers to deal with Fulcrum specifics around making HTTP requests.
        /// </summary>
        /// <seealso cref="ThrowFulcrumExceptionOnFail"/>
        /// <seealso cref="AddCorrelationId"/>
        /// <seealso cref="LogRequestAndResponse"/>
        /// <returns>A list of recommended handlers.</returns>
        public static DelegatingHandler[] CreateDelegatingHandlers(IAsyncRequestClient asyncRequestClient)
        {
            return CreateDelegatingHandlers(true, asyncRequestClient);
        }

        /// <summary>
        /// Creates handlers to deal with Fulcrum specifics around making HTTP requests, but without any logging
        /// </summary>
        /// <seealso cref="ThrowFulcrumExceptionOnFail"/>
        /// <seealso cref="AddCorrelationId"/>
        /// <returns>A list of recommended handlers.</returns>
        public static DelegatingHandler[] CreateDelegatingHandlersWithoutLogging(IAsyncRequestClient asyncRequestClient)
        {
            return CreateDelegatingHandlers(false, asyncRequestClient);
        }


        private static DelegatingHandler[] CreateDelegatingHandlers(bool withLogging, IAsyncRequestClient asyncRequestClient)
        {
            var handlers = new List<DelegatingHandler>
            {
                new ThrowFulcrumExceptionOnFail(),
                new AddCorrelationId(),
                new PropagateNexusTestHeader(),
                new CallAsyncManagerForAsynchronousRequests(asyncRequestClient)
            };
            if (withLogging)
            {
                handlers.Add(new LogRequestAndResponse());
            }
            return handlers.ToArray();
        }
    }
}
