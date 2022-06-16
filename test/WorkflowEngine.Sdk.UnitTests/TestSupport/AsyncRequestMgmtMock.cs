using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport
{
    public class AsyncRequestMgmtMock : IAsyncRequestMgmtCapability
    {
        public Mock<IRequestService> RequestServiceMoq { get; }
        public Mock<IRequestResponseService> RequestResponseServiceMoq { get; }

        public AsyncRequestMgmtMock()
        {
            RequestServiceMoq = new Mock<IRequestService>();
            Request = RequestServiceMoq.Object;
            RequestResponseServiceMoq = new Mock<IRequestResponseService>();
            RequestResponse = RequestResponseServiceMoq.Object;
        }
        /// <inheritdoc />
        public IRequestService Request { get; }

        /// <inheritdoc />
        public IRequestResponseService RequestResponse { get; }
    }
}