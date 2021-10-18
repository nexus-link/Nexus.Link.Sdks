using System;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
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