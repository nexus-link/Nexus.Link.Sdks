using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    public class AsyncRequestMgmtMock : IAsyncRequestMgmtCapability
    {
        public Mock<IRequestService> RequestServiceMock { get; }
        public Mock<IRequestResponseService> RequestResponseServiceMock { get; }
        public Mock<IExecutionService> ExecutionServiceMock { get; }
        public Mock<IExecutionResponseService> ExecutionResponseServiceMock { get; }

        public AsyncRequestMgmtMock()
        {
            RequestServiceMock = new Mock<IRequestService>();
            Request = RequestServiceMock.Object;
            RequestResponseServiceMock = new Mock<IRequestResponseService>();
            RequestResponse = RequestResponseServiceMock.Object;
            ExecutionServiceMock = new Mock<IExecutionService>();
            Execution = ExecutionServiceMock.Object;
            ExecutionResponseServiceMock = new Mock<IExecutionResponseService>();
            ExecutionResponse = ExecutionResponseServiceMock.Object;
        }
        /// <inheritdoc />
        public IRequestService Request { get; }

        /// <inheritdoc />
        public IRequestResponseService RequestResponse { get; }

        /// <inheritdoc />
        public IExecutionService Execution { get; }

        /// <inheritdoc />
        public IExecutionResponseService ExecutionResponse { get; }
    }
}