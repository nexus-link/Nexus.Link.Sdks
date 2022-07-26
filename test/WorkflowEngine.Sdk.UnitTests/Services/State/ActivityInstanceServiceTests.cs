using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Moq;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Capabilities.WorkflowState.UnitTests.Services;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class ActivityInstanceServiceTests : ActivityInstanceServiceTestsBase
    {
        private static Mock<IHttpSender> _httpSenderMock;

        public static IHttpSender HttpSender
        {
            get
            {
                if (_httpSenderMock != null) return _httpSenderMock.Object;
                _httpSenderMock = new Mock<IHttpSender>();
                _httpSenderMock.Setup(sender => sender.SendRequestAsync(
                        It.IsAny<HttpMethod>(),
                        It.IsAny<string>(), null,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync((HttpMethod _, string _,
                        Dictionary<string, List<string>> _,
                        CancellationToken _) =>
                    {

                        return new HttpResponseMessage(HttpStatusCode.OK);
                    });
                _httpSenderMock.Setup(sender => sender.CreateHttpSender(
                        It.IsAny<string>()))
                    .Returns((string _) => _httpSenderMock.Object);
                return _httpSenderMock.Object;
            }
        }

        public ActivityInstanceServiceTests()
        : base(new ActivityInstanceService(new RuntimeTablesMemory(), new AsyncRequestMgmtRestClients(HttpSender)))
        {
        }
    }
}
