using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport
{
    public class AsyncRequestMgmtMock : IAsyncRequestMgmtCapability
    {
        public Mock<IRequestService> RequestServiceMock { get; }
        public Mock<IRequestResponseService> RequestResponseServiceMock { get; }

        public AsyncRequestMgmtMock()
        {
            RequestServiceMock = new Mock<IRequestService>();
            Request = RequestServiceMock.Object;
            RequestResponseServiceMock = new Mock<IRequestResponseService>();
            RequestResponse = RequestResponseServiceMock.Object;
        }
        /// <inheritdoc />
        public IRequestService Request { get; }

        /// <inheritdoc />
        public IRequestResponseService RequestResponse { get; }
    }
}