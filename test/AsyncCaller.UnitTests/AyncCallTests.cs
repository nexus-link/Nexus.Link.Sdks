using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.AsyncCaller.Sdk;
using Nexus.Link.AsyncCaller.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Application;

namespace Sdk.UnitTests
{
    [TestClass]
    public class AyncCallTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AyncCallTests));
        }

        [TestMethod]
        public async Task MinimalAsyncCall()
        {
            RawRequest foundRawRequest = null;
            var asyncCallerMock = new Mock<IAsyncCaller>();
            var asyncCall = new AsyncCall(asyncCallerMock.Object, HttpMethod.Get, new Uri("http://ignore"));
            asyncCallerMock
                .Setup(ac => ac.ExecuteAsync(It.IsAny<RawRequest>()))
                .Callback((RawRequest r) => foundRawRequest = r)
                .ReturnsAsync("ignore");
            //asyncCall
            //    .SetCallback(HttpMethod.Get, "http://ignore");
            await asyncCall.ExecuteAsync();
            Assert.IsNotNull(foundRawRequest);
            Assert.IsNull(foundRawRequest.CallBack);
            Assert.IsNull(foundRawRequest.CallBackUriScheme);
            Assert.AreEqual("http", foundRawRequest.CallOutUriScheme);
            Assert.IsNull(foundRawRequest.Context);
            var expectedTitle = $"GET {asyncCall.CallOut.RequestUri} ({foundRawRequest.Id})";
            Assert.AreEqual(expectedTitle, foundRawRequest.Title);
        }
    }
}
