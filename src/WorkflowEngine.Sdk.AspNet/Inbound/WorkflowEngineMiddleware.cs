using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.WorkflowEngine.Sdk.AspNet.Extensions;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound
{
    /// <summary>
    /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <see name="INexusLinkMiddlewareOptions"/>
    /// to specify exactly how they should behave.
    /// </summary>
    [Obsolete("Please use Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware. Obsolete since 2022-04-07.")]
    public class WorkflowEngineMiddleware
    {
        protected readonly RequestDelegate Next;
        protected readonly WorkflowEngineMiddlewareOptions Options;

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public WorkflowEngineMiddleware(RequestDelegate next, WorkflowEngineMiddlewareOptions options)
        {
            InternalContract.RequireValidated(options, nameof(options));

            Next = next;
            Options = options;
        }

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public WorkflowEngineMiddleware(RequestDelegate next, IOptions<WorkflowEngineMiddlewareOptions> options)
        {
            InternalContract.RequireNotNull(options.Value, nameof(options));
            InternalContract.RequireValidated(options.Value, nameof(options));

            Next = next;
            Options = options.Value;
        }

        // TODO: Make code example complete
        // TODO: Make callbacks in options
        // TODO: Move code into one big invoke

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// The main method for a middleware.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var cancellationToken = context.RequestAborted;
            // Enable multiple reads of the content
            context.Request.EnableBuffering();

            if (Options.Features.RedirectAsynchronousRequests.Enabled)
            {
                var parentExecutionId = ExtractManagedAsynchronousRequestIdFromHeader(context);
                FulcrumApplication.Context.ManagedAsynchronousRequestId = parentExecutionId;
            }

            try
            {
                await Next(context);
            }
            catch (Exception exception)
            {
                if (Options.Features.RedirectAsynchronousRequests.Enabled)
                {
                    if (exception is RequestPostponedException)
                    {
                        if (FulcrumApplication.Context.ManagedAsynchronousRequestId == null)
                        {
                            throw await RerouteToAsyncRequestMgmtAndCreateExceptionAsync(context);
                        }
                    }
                }
                throw;
            }
        }

        #region RedirectAsynchronousRequests
        protected static string ExtractManagedAsynchronousRequestIdFromHeader(HttpContext context)
        {
            var request = context?.Request;

            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
            if (!request.Headers.TryGetValue(Constants.ManagedAsynchronousRequestId, out var executionIds))
            {
                return null;
            }
            var executionsArray = executionIds.ToArray();
            if (!executionsArray.Any()) return null;
            if (executionsArray.Length == 1) return executionsArray[0];
            var message =
                $"There was more than one execution id in the header: {string.Join(", ", executionsArray)}. The first one was picked as the Fulcrum execution id from here on.";
            Log.LogWarning(message);
            return executionsArray[0];
        }
        private async Task<Exception> RerouteToAsyncRequestMgmtAndCreateExceptionAsync(HttpContext context)
        {
            var requestService = Options.Features.RedirectAsynchronousRequests.RequestService;
            FulcrumAssert.IsNotNull(requestService, CodeLocation.AsString());
            var cancellationToken = context.RequestAborted;
            var requestCreate = await new HttpRequestCreate().FromAsync(context.Request, 0.5, cancellationToken);
            FulcrumAssert.IsNotNull(requestCreate, CodeLocation.AsString());
            FulcrumAssert.IsValidated(requestCreate, CodeLocation.AsString());
            var requestId = await requestService.CreateAsync(requestCreate, cancellationToken);
            FulcrumAssert.IsNotNullOrWhiteSpace(requestId, CodeLocation.AsString());
            var urls = requestService.GetEndpoints(requestId);
            FulcrumAssert.IsNotNull(urls, CodeLocation.AsString());
            FulcrumAssert.IsValidated(urls, CodeLocation.AsString());
            return new RequestAcceptedException(requestId)
            {
                PollingUrl = urls.PollingUrl,
                RegisterCallbackUrl = urls.RegisterCallbackUrl
            };
        }
        #endregion
    }

    /// <summary>
    /// Convenience class for middleware
    /// </summary>
    [Obsolete("Please use AsyncManagerMiddlewareExtension. Obsolete since 2022-04-07.")]
    public static class WorkflowEngineMiddlewareExtension
    {
        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="builder">"this"</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        [Obsolete("Please use UseAsyncManagerMiddleware. Obsolete since 2022-04-07.")]
        public static IApplicationBuilder UseWorkflowEngineMiddleware(this IApplicationBuilder builder, IOptions<WorkflowEngineMiddlewareOptions> options)
        {
            return builder.UseMiddleware<WorkflowEngineMiddleware>(options);
        }

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="builder">"this"</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        [Obsolete("Please use UseAsyncManagerMiddleware. Obsolete since 2022-04-07.")]
        public static IApplicationBuilder UseWorkflowEngineMiddleware(this IApplicationBuilder builder, WorkflowEngineMiddlewareOptions options)
        {
            return builder.UseMiddleware<WorkflowEngineMiddleware>(options);
        }
    }
}