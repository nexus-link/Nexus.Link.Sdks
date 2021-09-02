using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Core.Application;

namespace IdentityAccessManagement.Sdk.Pipe.Inbound
{
    public class AddNexusClientPrincipal
    {

        private readonly RequestDelegate _next;

        public AddNexusClientPrincipal(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            FulcrumApplication.Context.ClientPrincipal = context.User;
            await _next(context);
        }
    }
}