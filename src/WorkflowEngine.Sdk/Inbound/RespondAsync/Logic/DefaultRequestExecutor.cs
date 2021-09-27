using System.Net.Http;

#if NETCOREAPP
namespace Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync.Logic
{
    public class DefaultRequestExecutor : RequestExecutorBase
    {
        public DefaultRequestExecutor(HttpClient httpClient) : base(httpClient)
        {
#if false
            // I would like to do something like this
            var webApplicationFactory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Startup>();
            HttpClient = webApplicationFactory.CreateDefaultClient();
#endif
        }
    }
}
#endif