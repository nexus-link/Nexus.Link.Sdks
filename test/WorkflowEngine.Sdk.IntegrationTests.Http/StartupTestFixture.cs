using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;
using PoC.API;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.RestClients;

namespace WorkflowEngine.IntegrationTests.Http
{
    public class StartupTestFixture : WebApplicationFactory<Startup>
    {
        private object _lock = new object();

        private HttpClient _apiClient;

        public HttpClient ApiClient
        {
            get
            {
                lock (_lock)
                {
                    if (_apiClient == null)
                    {
                        _apiClient = CreateDefaultClient(OutboundPipeFactory.CreateDelegatingHandlers());
                        _apiClient.DefaultRequestHeaders.Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    }

                    return _apiClient;
                }
            }
        }

        private IHttpSender _httpSender;

        public IHttpSender HttpSender
        {
            get
            {
                lock (_lock)
                {
                    if (_httpSender == null)
                    {
                        _httpSender = new HttpSender(null)
                        {
                            HttpClient = new HttpClientWrapper(ApiClient)
                        };
                    }

                    return _httpSender;
                }
            }
        }

        private IAsyncRequestClient _asyncRequestClient;

        public IAsyncRequestClient AsyncManagementRestClients
        {
            get
            {
                lock (_lock)
                {
                    if (_asyncRequestClient == null)
                    {
                        _asyncRequestClient =
                            new AsyncRequestClient(FulcrumApplication.Setup.Tenant, HttpSender.CreateHttpSender("AsyncManager"));
                    }

                    return _asyncRequestClient;
                }
            }
        }

        private IWorkflowCapability _workflowRestClients;

        public IWorkflowCapability WorkflowRestClients
        {
            get
            {
                lock (_lock)
                {
                    if (_workflowRestClients == null)
                    {
                        _workflowRestClients = new WorkflowRestClients(HttpSender.CreateHttpSender("WorkflowEngine"));
                    }

                    return _workflowRestClients;
                }
            }
        }

        public IServiceCollection ServiceCollection { get; private set; }

        public StartupTestFixture()
        {
            FulcrumApplicationHelper.UnitTestSetup("IntegrationTests");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                ServiceCollection = services;

                // Make sure that we use the memory implementation of tables
                services.AddSingleton<IConfigurationTables>(p => new ConfigurationTablesMemory());
            });

            base.ConfigureWebHost(builder);
        }
    }

    /// <summary>
    /// This "lazy" implementation was required to set the rest clients in ConfigureWebHost without an urgent need for the ApiClient in StartupTestFixture
    /// </summary>
    public class LazyWorkflowRestClients : IWorkflowCapability
    {
        private readonly StartupTestFixture _fixture;

        public LazyWorkflowRestClients(StartupTestFixture fixture)
        {
            _fixture = fixture;
        }


        /// <inheritdoc />
        public IAsyncContextService AsyncContext => _fixture.WorkflowRestClients.AsyncContext;

        /// <inheritdoc />
        public IWorkflowFormService WorkflowForm => _fixture.WorkflowRestClients.WorkflowForm;

        /// <inheritdoc />
        public IWorkflowVersionService WorkflowVersion => _fixture.WorkflowRestClients.WorkflowVersion;

        /// <inheritdoc />
        public IWorkflowParameterService WorkflowParameter => _fixture.WorkflowRestClients.WorkflowParameter;


        /// <inheritdoc />
        public IActivityFormService ActivityForm => _fixture.WorkflowRestClients.ActivityForm;

        /// <inheritdoc />
        public IActivityVersionService ActivityVersion => _fixture.WorkflowRestClients.ActivityVersion;

        /// <inheritdoc />
        public IActivityInstanceService ActivityInstance => _fixture.WorkflowRestClients.ActivityInstance;

        /// <inheritdoc />
        public ITransitionService Transition => _fixture.WorkflowRestClients.Transition;

        /// <inheritdoc />
        public IActivityParameterService ActivityParameter => _fixture.WorkflowRestClients.ActivityParameter;

        /// <inheritdoc />
        public IWorkflowInstanceService WorkflowInstance => _fixture.WorkflowRestClients.WorkflowInstance;
    }
}
