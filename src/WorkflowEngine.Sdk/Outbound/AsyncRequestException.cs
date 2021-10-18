using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Outbound
{
    internal class AsyncRequestException : Exception
    {
        public string RequestId { get; }

        public AsyncRequestException(string requestId)
        {
            RequestId = requestId;
        }
    }
}