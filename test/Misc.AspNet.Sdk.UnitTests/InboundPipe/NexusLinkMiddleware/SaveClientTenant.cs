#if NETCOREAPP
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;
using Nexus.Link.Misc.AspNet.Sdk.Inbound.Options;

#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    [TestClass]
    public class SaveClientTenant
    {
        private static Tenant _foundClientTenant;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SaveClientTenant).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
            FulcrumApplication.Context.LeverConfiguration = null;
        }

        [TestMethod]
        public async Task SaveClientTenantOldPrefixSuccess()
        {
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveClientTenant.Enabled = true;
            options.Features.SaveClientTenant.RegexForFindingTenantInUrl = SaveClientTenantOptions.LegacyVersion;
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(context =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);
            foreach (var entry in new Dictionary<Tenant, string>
            {
                {new Tenant("smoke-testing-company", "ver"), "https://v-mock.org/v2/smoke-testing-company/ver/"},
                {
                    new Tenant("smoke-testing-company", "local"),
                    "https://v-mock.org/api/v-pa1/smoke-testing-company/local/"
                },
                {
                    new Tenant("fulcrum", "prd"),
                    "https://prd-fulcrum-fundamentals.azurewebsites.net/api/v1/fulcrum/prd/ServiceMetas/ServiceHealth"
                },
                {
                    new Tenant("fulcrum", "ver"),
                    "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/fulcrum/ver/ServiceMetas/ServiceHealth"
                }
            })
            {
                _foundClientTenant = null;
                var expectedTenant = entry.Key;
                var context = new DefaultHttpContext();
                context.SetRequest(entry.Value);
                await handler.InvokeAsync(context);
                Assert.AreEqual(expectedTenant, _foundClientTenant,
                    $"Could not find tenant '{expectedTenant}' from url '{entry.Value}'. Found {_foundClientTenant}");
            }
        }

        [TestMethod]
        public async Task SaveClientTenantOldPrefixAcceptsFalsePositives()
        {
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveClientTenant.Enabled = true;
            options.Features.SaveClientTenant.RegexForFindingTenantInUrl = SaveClientTenantOptions.LegacyVersion;
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);

            _foundClientTenant = null;
            var url = "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/false/positive/ServiceMetas/ServiceHealth";
            var context = new DefaultHttpContext();
            context.SetRequest(url);
            await handler.InvokeAsync(context);
            Assert.IsNotNull(_foundClientTenant);
        }

        [TestMethod]
        public async Task SaveClientTenantNewPrefixSuccess()
        {
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveClientTenant.Enabled = true;
            options.Features.SaveClientTenant.RegexForFindingTenantInUrl = SaveClientTenantOptions.ApiVersionTenant;
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(context =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);

            foreach (var entry in new Dictionary<Tenant, string>
            {
                {new Tenant("smoke-testing-company", "ver"), "https://v-mock.org/api/v2/Tenant/smoke-testing-company/ver/"},
                {
                    new Tenant("smoke-testing-company", "local"),
                    "https://v-mock.org/api/v-pa1/Tenant/smoke-testing-company/local/"
                },
                {
                    new Tenant("fulcrum", "prd"),
                    "https://prd-fulcrum-fundamentals.azurewebsites.net/api/v1/Tenant/fulcrum/prd/ServiceMetas/ServiceHealth"
                },
                {
                    new Tenant("fulcrum", "ver"),
                    "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/Tenant/fulcrum/ver/ServiceMetas/ServiceHealth"
                }
            })
            {
                _foundClientTenant = null;
                var expectedTenant = entry.Key;
                var context = new DefaultHttpContext();
                context.SetRequest(entry.Value);
                await handler.InvokeAsync(context);
                Assert.AreEqual(expectedTenant, _foundClientTenant,
                    $"Could not find tenant '{expectedTenant}' from url '{entry.Value}'. Found {_foundClientTenant}");
            }
        }

        [TestMethod]
        public async Task SaveClientTenantNewPrefixDetectsFalsePositives()
        {
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveClientTenant.Enabled = true;
            options.Features.SaveClientTenant.RegexForFindingTenantInUrl = SaveClientTenantOptions.ApiVersionTenant;
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);


            _foundClientTenant = null;
            const string url = "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/false/positive/ServiceMetas/ServiceHealth";
            var context = new DefaultHttpContext();
            context.SetRequest(url);
            await handler.InvokeAsync(context);
            Assert.IsNull(_foundClientTenant);
        }

        [TestMethod]
        public async Task SaveConfigurationFail()
        {
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveClientTenant.Enabled = true;
            options.Features.SaveClientTenant.RegexForFindingTenantInUrl = SaveClientTenantOptions.LegacyVersion;
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx => Task.CompletedTask, options);
            FulcrumApplication.Context.ClientTenant = null;
            foreach (var url in new[]
            {
                "http://gooogle.com/",
                "https://anywhere.org/api/v1/eels/"
            })
            {
                var context = new DefaultHttpContext();
                context.SetRequest(url);
                await handler.InvokeAsync(context);
                Assert.IsNull(FulcrumApplication.Context.ClientTenant);
            }
        }
    }
}
#endif