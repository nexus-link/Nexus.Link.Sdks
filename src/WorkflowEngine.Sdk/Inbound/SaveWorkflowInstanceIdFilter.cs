using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Inbound
{
    public class SaveWorkflowInstanceIdFilter : IAsyncActionFilter
    {
        public SaveWorkflowInstanceIdFilter()
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            if (TryGetExecutionId(context.HttpContext.Request, out var executionId))
            {
                // We will use the execution id as our workflow instance id.
                AsyncWorkflowStatic.Context.WorkflowInstanceId = executionId;
            }

            await next();
        }

        private static bool TryGetExecutionId(HttpRequest request, out string executionId)
        {
            executionId = null;
            var found = request.Headers.TryGetValue(Constants.ExecutionIdHeaderName, out var value);
            if (!found) return false;
            executionId = value.FirstOrDefault();
            return !string.IsNullOrWhiteSpace(executionId);
        }
    }

    public class SaveWorkflowInstanceIdFilterConfigureOptions : IConfigureOptions<MvcOptions>
    {
        /// <inheritdoc />
        public void Configure(MvcOptions options)
        {
            var filter = new SaveWorkflowInstanceIdFilter();
            options.Filters.Add(filter);
        }
    }

    /// <summary>
    /// Convenience method for adding the <see cref="ValueTranslatorFilter"/>.
    /// </summary>
    public static class SaveWorkflowInstanceIdFilterExtensions
    {
        /// <summary>
        /// Add the <see cref="SaveWorkflowInstanceIdFilter"/> to MVC.
        /// </summary>
        public static IServiceCollection AddSaveWorkflowInstanceIdFilter(this IServiceCollection services)
        {
            services.ConfigureOptions<SaveWorkflowInstanceIdFilterConfigureOptions>();
            return services;
        }
    }
}