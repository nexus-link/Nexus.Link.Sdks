#if NETCOREAPP
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Contracts.Misc.AspNet.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
using Shouldly;

namespace Authentication.AspNet.Sdk.UnitTests
{
    [TestClass]
    public class TokenValidationHandlerTests
    {
        private Mock<IReentryAuthenticationService> _tokenServiceMock;
        private TokenValidationHandler _handler;
        private Support _support;
        private IPrincipal _actualClientPrincipal;

        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TokenValidationHandlerTests));
            _support = new Support();
            
            _tokenServiceMock = new Mock<IReentryAuthenticationService>();
            var publicKey = new RsaSecurityKey(_support.CryptoServiceProvider.ExportParameters(false));
            _handler = new TokenValidationHandler(ctx =>
            {
                _actualClientPrincipal = FulcrumApplication.Context.ClientPrincipal;
                return Task.CompletedTask;
            }, publicKey, _tokenServiceMock.Object);
            _actualClientPrincipal = null;
        }

        [TestMethod]
        public async Task Invoke_01_Given_Expired_Gives_ClientPrincipalNotSet()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var expectedKey = Guid.NewGuid().ToGuidString();
            var expiredToken = _support.CreateToken(FulcrumApplication.Setup.Tenant, "clientId", new[] { "Buyer" }, true, false);
            context.SetRequestWithReentryAuthentication(expectedKey, expiredToken);

            // Act
            await _handler.InvokeAsync(context);

            // Assert
            _actualClientPrincipal.ShouldBeNull();
        }

        [TestMethod]
        public async Task Invoke_02_Given_Valid_Gives_ClientPrincipalSet()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var expectedKey = Guid.NewGuid().ToGuidString();
            var validToken = _support.CreateToken(FulcrumApplication.Setup.Tenant, "clientId", new[] { "Buyer" }, false, false);
            context.SetRequestWithReentryAuthentication(expectedKey, validToken);

            // Act
            await _handler.InvokeAsync(context);

            // Assert
            _actualClientPrincipal.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task Invoke_03_Given_ReentryTokenNotSet_Gives_ValidateReentryIsNotCalled()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var expectedKey = Guid.NewGuid().ToGuidString();
            var expiredToken = _support.CreateToken(FulcrumApplication.Setup.Tenant, "clientId", new[] { "Buyer" }, true, false);
            context.SetRequestWithReentryAuthentication(expectedKey, expiredToken);

            // Act
            await _handler.InvokeAsync(context);

            // Assert
            _tokenServiceMock.Verify(ts => ts.ValidateAsync(It.IsAny<string>(), It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Invoke_04_Given_ReentryTokenSet_Gives_ValidateReentryIsCalled()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var expectedKey = Guid.NewGuid().ToGuidString();
            var expiredToken = _support.CreateToken(FulcrumApplication.Setup.Tenant, "clientId", new[] { "Buyer" }, true, false);
            context.SetRequestWithReentryAuthentication(expectedKey, expiredToken);
            FulcrumApplication.Context.ReentryAuthentication = expectedKey;

            // Act
            await _handler.InvokeAsync(context);

            // Assert
            _tokenServiceMock.Verify(ts => ts.ValidateAsync(It.IsAny<string>(), It.IsAny<HttpContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
#endif