using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;

namespace Nexus.Link.AsyncManager.Sdk
{
    /// <summary>
    /// Keeps the information about the response to an asynchronous request
    /// </summary>
    public class AsyncHttpResponse : HttpResponse
    {
        /// <summary>
        /// Convenience for testing if the request has completed.
        /// </summary>
        public bool HasCompleted => Metadata.RequestHasCompleted;
    }
}
