using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Shouldly;
using AuthenticationManager = Nexus.Link.Authentication.Sdk.AuthenticationManager;

namespace Authentication.Sdk.UnitTests
{
    [TestClass]
    public class AuthenticationManagerTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AuthenticationManagerTests));
        }

        [TestMethod]
        public void CreateRsaSecurityKeyFromXmlString()
        {
            const string publicKeyXml =
                "<RSAKeyValue><Modulus>uOnkn1I4mESAEk1PWIRrL3fOLq+rVXvP6rUKI9+pZSKQ7Uv7+RfGczZNd5mHzGyD2YO+Ml0AjgPKhOjibvKSe97gLEsKsQoOExiXcA5YHmTT5f9/Of5PueoEHE24XJAQxawnQqsdz/Pjt+W7FXweU8ToK3/nsLHy2frCjo8KS8m9PaoM2kXIyKF2Yt8zBOJBup1/Coabs/k5UekXBqG3EKYI/OS29we7WVvc3v83hK92ANMEPjt1HPKU23ys+HlH9H+6IODybBChy3JdZrvbmpyiqcY0Vaz7k3P1xw1H87JXUA8W0KTdO9f4bbmTKAeBsMj1d11lk+Nuov7t5S99kQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            var rsaSecurityKey = AuthenticationManager.CreateRsaSecurityKeyFromXmlString(publicKeyXml);
            Assert.IsNotNull(rsaSecurityKey);
        }

        [TestMethod]
        public async Task HttpClientException_For_PublicKey_Throws_And_Tracks()
        {
            // Arrange
            var tenant = new Tenant("amce", "local");
            const string errorMessage = "dns problem";
            var httpClientMock = new Mock<IHttpClient>();
            AuthenticationManager.HttpClient = httpClientMock.Object;

            httpClientMock
                .Setup(_ => _.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new WebException(errorMessage));

            // Act & Assert
            var exception = await AuthenticationManager
                .GetPublicKeyXmlAsync(tenant, "", "", CancellationToken.None)
                .ShouldThrowAsync<FulcrumResourceException>();
            exception.InnerException.ShouldBeOfType<WebException>();
            exception.InnerException.Message.ShouldBe(errorMessage);

            var problems = FulcrumApplication.Setup.HealthTracker.GetHealthProblems(tenant);
            problems.ShouldNotBeNull();
            var problem = problems.FirstOrDefault();
            problem.ShouldNotBeNull();
            problem.Tenant.ShouldBe(tenant);
        }
    }
}
