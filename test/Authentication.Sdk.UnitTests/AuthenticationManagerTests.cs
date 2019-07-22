using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;

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
    }
}
