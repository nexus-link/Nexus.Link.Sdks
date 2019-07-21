using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents.Model;
using Nexus.Link.Services.Controllers.Capabilities.Integration.BusinessEvents;

namespace Services.Controllers.UnitTests
{
    [TestClass]
    public class BusinessEventControllerBaseTests
    {
        private BusinessEventsController _businessEventsController;
        private Mock<IBusinessEventsCapability> _businessEventsCapabilityMoq;

        [TestInitialize]
        public void RunBeforeEachTestCase()
        {
            _businessEventsCapabilityMoq = new Mock<IBusinessEventsCapability>();

            _businessEventsController = new BusinessEventsController(_businessEventsCapabilityMoq.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumServiceContractException))]
        public async Task PublishNull()
        {
            await _businessEventsController.PublishAsync(null);
            _businessEventsCapabilityMoq.Verify();
        }

        [TestMethod]
        public async Task Publish()
        {
            var myEvent = new MyEvent();
            myEvent.DataField = Guid.NewGuid().ToString();
            MyEvent receivedEvent = null;
            _businessEventsCapabilityMoq
                .Setup(capability =>
                    capability.BusinessEventService.PublishAsync(It.IsAny<JToken>(), It.IsAny<CancellationToken>()))
                .Callback((JToken e, CancellationToken t) => { receivedEvent = e.ToObject<MyEvent>(); });
            var jObject = JObject.FromObject(myEvent);
            var publishableEvent = JsonHelper.SafeDeserializeObject<PublishableEvent>(jObject);
            Assert.IsNotNull(publishableEvent?.Metadata);
            await _businessEventsController.PublishAsync(jObject);
            Assert.IsNotNull(receivedEvent?.Metadata);
            Assert.AreEqual(myEvent.Metadata.EntityName, receivedEvent.Metadata.EntityName);
            Assert.AreEqual(myEvent.Metadata.EventName, receivedEvent.Metadata.EventName);
            Assert.AreEqual(myEvent.Metadata.MajorVersion, receivedEvent.Metadata.MajorVersion);
            Assert.AreEqual(myEvent.Metadata.MinorVersion, receivedEvent.Metadata.MinorVersion);
            Assert.AreEqual(myEvent.DataField, receivedEvent.DataField);
            _businessEventsCapabilityMoq.Verify();
        }

        private class MyEvent : IPublishableEvent
        {
            /// <inheritdoc />
            public EventMetadata Metadata { get; } =
                new EventMetadata(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 5, 7);

            public string DataField { get; set;  }
        }

        private class BusinessEventsController : BusinessEventsControllerBase
        {
            /// <inheritdoc />
            public BusinessEventsController(IBusinessEventsCapability capability) : base(capability)
            {
            }
        }
    }
}
