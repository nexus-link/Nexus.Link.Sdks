using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Nexus.Link.Libraries.Web.AspNet.Serialization;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Serialization;
using WorkflowEngine.Sdk.Inbound.RespondAsync;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;

namespace WorkflowEngine.Sdk.Inbound
{
    public class GetAsyncContextFilter : IAsyncActionFilter
    {
        private readonly IWorkflowCapabilityForClient _workflowCapability;

        public GetAsyncContextFilter(IWorkflowCapabilityForClient workflowCapability)
        {
            _workflowCapability = workflowCapability;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (AsyncWorkflowStatic.Context.ExecutionIsAsynchronous)
            {
                var request = context.HttpContext.Request;
                var cancellationToken = context.HttpContext.RequestAborted;
                var requestData = await new RequestData().FromAsync(request, cancellationToken);
                var success = RespondAsyncFilterSupport.TryGetExecutionId(requestData.Headers, out var executionId);
                FulcrumAssert.IsTrue(success, CodeLocation.AsString());
                var executionContext  = await _workflowCapability.AsyncContext.GetExecutionContextAsync(executionId.ToString(), requestData, cancellationToken);
                FulcrumAssert.IsNotNull(executionContext, CodeLocation.AsString());
                AsyncWorkflowStatic.Context.AsyncExecutionContext = executionContext;
            }

            await next();
        }

        private static bool CheckIfClientPrefersAsynchronousExecution(HttpRequest request)
        {
            return request.Headers.TryGetValue(Constants.PreferHeaderName, out var preferHeader)
                   && ((ICollection<string>)preferHeader).Contains(Constants.PreferRespondAsyncHeaderValue);
        }

        private static RespondAsyncOpinionEnum GetMethodOpinion(ActionDescriptor actionDescriptor)
        {
            var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
            var methodInfo = controllerActionDescriptor?.MethodInfo;
            var preferAsyncAttribute = methodInfo?.GetCustomAttribute<RespondAsyncAttribute>();
            return preferAsyncAttribute?.Opinion ?? RespondAsyncOpinionEnum.Indifferent;
        }
    }

    /// <summary>
    /// https://stackoverflow.com/questions/55990151/is-adding-addmvc-service-twice-in-configureservices-a-good-practice-in-asp-n
    /// </summary>
    public class GetAsyncContextFilterConfigureOptions : IConfigureOptions<MvcOptions>
    {
        private readonly IWorkflowCapabilityForClient _workflowCapability;

        public GetAsyncContextFilterConfigureOptions(IWorkflowCapabilityForClient workflowCapability)
        {
            _workflowCapability = workflowCapability;
        }
        /// <inheritdoc />
        public void Configure(MvcOptions options)
        {
            var filter = new GetAsyncContextFilter(_workflowCapability);
            options.Filters.Add(filter);
        }
    }

    /// <summary>
    /// Convenience method for adding the <see cref="ValueTranslatorFilter"/>.
    /// </summary>
    public static class GetAsyncContextFilterExtensions
    {
        /// <summary>
        /// Add the <see cref="RespondAsyncFilter"/> to MVC.
        /// </summary>
        public static IServiceCollection AddGetAsyncContextFilter(this IServiceCollection services)
        {
            services.ConfigureOptions<GetAsyncContextFilterConfigureOptions>();
            return services;
        }
    }
}