using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

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
                    .ReturnsAsync((HttpMethod method, string relativeUrl,
                            Dictionary<string, List<string>> customHeaders,
                        CancellationToken cancellationToken) =>
                    {
                        
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    });
                return _httpSenderMock.Object;
            }
        }

        public ActivityInstanceServiceTests()
        : base(new ActivityInstanceService(new RuntimeTablesMemory(), new AsyncRequestMgmtRestClients(HttpSender)))
        {
        }
    }
}
