using System.Collections.Generic;
using System.Net.Http;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
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
        public static DelegatingHandler[] CreateDelegatingHandlers(IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            return CreateDelegatingHandlers(true, asyncRequestMgmtCapability);
        }

        /// <summary>
        /// Creates handlers to deal with Fulcrum specifics around making HTTP requests, but without any logging
        /// </summary>
        /// <seealso cref="ThrowFulcrumExceptionOnFail"/>
        /// <seealso cref="AddCorrelationId"/>
        /// <returns>A list of recommended handlers.</returns>
        public static DelegatingHandler[] CreateDelegatingHandlersWithoutLogging(IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            return CreateDelegatingHandlers(false, asyncRequestMgmtCapability);
        }


        private static DelegatingHandler[] CreateDelegatingHandlers(bool withLogging, IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            var handlers = new List<DelegatingHandler>
            {
                new ThrowFulcrumExceptionOnFail(),
                new AddCorrelationId(),
                new PropagateNexusTestHeader(),
                new CallAsyncManagerForAsynchronousRequests(asyncRequestMgmtCapability)
            };
            if (withLogging)
            {
                handlers.Add(new LogRequestAndResponse());
            }
            return handlers.ToArray();
        }
    }
}
