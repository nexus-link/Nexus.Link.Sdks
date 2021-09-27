using System.Net.Http;
using Moq;
using Nexus.Link.AsyncManager.Sdk;

namespace AsyncManager.Sdk.UnitTests
{
    /// <summary>
    /// Class to access protected parts of the <see cref="AsyncHttpRequest"/> class.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class AsyncHttpRequest_ForTest : AsyncHttpRequest
    {
        /// <inheritdoc />
        public AsyncHttpRequest_ForTest(IAsyncRequestClient asyncRequestClient, HttpMethod method, string url, double priority) : base(asyncRequestClient, method, url, priority)
        {
        }

        /// <inheritdoc />
        public AsyncHttpRequest_ForTest(HttpMethod method, string url, double priority) : base(new Mock<IAsyncRequestClient>().Object, method, url, priority)
        {
        }
    }
}