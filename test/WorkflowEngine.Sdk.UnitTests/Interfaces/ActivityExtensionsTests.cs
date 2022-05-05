using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using WorkflowEngine.Sdk.UnitTests.Internal.Logic;
using WorkflowEngine.Sdk.UnitTests.TestSupport;

namespace WorkflowEngine.Sdk.UnitTests.Interfaces
{
    public class ActivityExtensionsTests
    {
        private readonly ActivityInformationMock _activityInformation;
        private readonly ActivityMock _activityMock;
        private readonly ActivityExecutor _executor;
        private readonly ActivityInformationMock _activityInformationResult;
        private readonly ActivityMock _activityMockResult;
        private readonly ActivityExecutor _executorResult;

        public ActivityExtensionsTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
            FulcrumApplication.Setup.SynchronousFastLogger = new ConsoleLogger();

            var workflowInformationMock = new WorkflowInformationMock(null);
            _activityInformation = new ActivityInformationMock(workflowInformationMock);
            _activityMock = new ActivityMock(_activityInformation);
            _executor = new ActivityExecutor(_activityMock);
            workflowInformationMock.Executor = _executor;

            var workflowInformationResultMock = new WorkflowInformationMock(null);
            _activityInformationResult = new ActivityInformationMock(workflowInformationResultMock);
            _activityMockResult = new ActivityMock<int>(_activityInformationResult, null);
            _executorResult = new ActivityExecutor(_activityMockResult);
            workflowInformationResultMock.Executor = _executorResult;
        }
    }
}

