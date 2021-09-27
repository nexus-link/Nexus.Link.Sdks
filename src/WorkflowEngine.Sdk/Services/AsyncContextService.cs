using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.WorkflowEngine.Sdk.Services
{
    public class AsyncContextService : IAsyncContextService
    {
        private readonly IAsyncRequestMgmtCapability _asyncManagementCapability;

        public Dictionary<Guid, AsyncExecutionContext> ExecutionContexts { get; } = new Dictionary<Guid, AsyncExecutionContext>();

        public AsyncContextService(IAsyncRequestMgmtCapability asyncManagementCapability)
        {
            _asyncManagementCapability = asyncManagementCapability;
        }

        /// <inheritdoc />
        public async Task<AsyncExecutionContext> GetExecutionContextAsync(string executionId, RequestData requestData, CancellationToken cancellationToken = default)
        {
            var isGuid = Guid.TryParse(executionId, out var executionIdGuid);
            FulcrumAssert.IsTrue(isGuid, CodeLocation.AsString());
            if (!ExecutionContexts.TryGetValue(executionIdGuid, out var context))
            {
                return await CreateExecutionContextAsync(executionId, requestData);
            }
            // TODO: Add transaction
            var taskList = new List<Task>();
            foreach (var subRequest in context.SubRequests.Values)
            {
                var task = UpdateResponseValueAsync(subRequest);
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
            return context.AsCopy();


            #region Local methods
            async Task UpdateResponseValueAsync(SubRequest subRequest)
            {
                if (subRequest.HasCompleted) return;
                if (!subRequest.RequestId.HasValue) return;

                var requestIdAsString = subRequest.RequestId.Value.ToString();
                var response =
                    await _asyncManagementCapability.Response.GetResponseAsync(requestIdAsString, cancellationToken);

                if (response == null) throw new PostponeException();
                // TODO: Handle response that was an exception
                subRequest.ResultValueAsJson = response.BodyAsString;
                subRequest.HasCompleted = true;
            }
            #endregion

        }

        private Task<AsyncExecutionContext> CreateExecutionContextAsync(string executionId, RequestData requestData)
        {
            var isGuid = Guid.TryParse(executionId, out var executionIdGuid);
            FulcrumAssert.IsTrue(isGuid, CodeLocation.AsString());
            InternalContract.RequireNotNull(requestData, nameof(requestData));

            var context = new AsyncExecutionContext
            {
                CurrentRequest = requestData,
                ExecutionId = executionIdGuid,
                IsAsynchronous = true
            };
            ExecutionContexts[executionIdGuid] = context;
            return Task.FromResult(context);
        }

        /// <inheritdoc />
        public Task AddSubRequestAsync(string executionId, string identifier, SubRequest subRequest, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(executionId, nameof(executionId));
            InternalContract.RequireNotNullOrWhiteSpace(identifier, nameof(identifier));
            var isGuid = Guid.TryParse(executionId, out var executionIdGuid);
            FulcrumAssert.IsTrue(isGuid, CodeLocation.AsString());
            var executionContextFound = ExecutionContexts.TryGetValue(executionIdGuid, out var context);
            FulcrumAssert.IsTrue(executionContextFound, CodeLocation.AsString(),
                $"Expected to find execution context for execution {executionId} for sub request {identifier}, {subRequest.Description}");

            // TODO: Transaction

            var success = context!.SubRequests.TryAdd(identifier, subRequest);
            FulcrumAssert.IsTrue(success, CodeLocation.AsString(),
                $"Execution {executionId} unexpectedly already had sub request {identifier}, {subRequest.Description}");
            return Task.CompletedTask;
        }


        private const string Indentation = "  ";

        /// <inheritdoc />
        public Task<string> GetStatusAsStringAsync(string requestId, CancellationToken cancellationToken = default)
        {
            var isGuid = Guid.TryParse(requestId, out var requestIdGuid);
            FulcrumAssert.IsTrue(isGuid, CodeLocation.AsString());
            return GetStatusAsStringAsync("", requestIdGuid, "", cancellationToken);
        }

        private async Task<string> GetStatusAsStringAsync(string title, Guid requestId, string indentation, CancellationToken cancellationToken = default)
        {
            var execution = await _asyncManagementCapability.Request.GetLatestExecutionAsync(requestId.ToString(), cancellationToken);
            if (execution == null) return "";
            var success = ExecutionContexts.TryGetValue(execution.Id, out var context);
            if (!success) return "";
            var builder = new StringBuilder($"{title} {context.CurrentRequest}: STATE?"); // TODO: Get state
                                                                                          // TODO: Transaction
            foreach (var subRequest in context.SubRequests.Values)
            {
                var subTitle = $"\r{indentation}{subRequest.Description}";
                if (!subRequest.RequestId.HasValue)
                {
                    builder.Append(subTitle);
                }
                else
                {
                    var subStatus = await GetStatusAsStringAsync(subTitle, subRequest.RequestId.Value,
                        indentation + Indentation);
                    builder.Append(subStatus);
                }
            }


            return builder.ToString();
        }

        public Task<JObject> GetStatusAsJsonAsync(string requestId, CancellationToken cancellationToken = default)
        {
            var isGuid = Guid.TryParse(requestId, out var requestIdGuid);
            InternalContract.Require(isGuid, $"The parameter {nameof(requestId)} must be a {nameof(Guid)}.");

            return InternalGetStatusAsJsonAsync(requestIdGuid, cancellationToken);
        }

        private async Task<JObject> InternalGetStatusAsJsonAsync(Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var waitCount = 0;
            var errorCount = 0;
            var jObject = new JObject();
            var execution = await _asyncManagementCapability.Request.GetLatestExecutionAsync(requestId.ToString(), cancellationToken);
            if (execution == null)
            {
                jObject.Add($"Wait {++waitCount}", $"Request {requestId} does not yet have an execution.");
                return jObject;
            }
            var success = ExecutionContexts.TryGetValue(execution.Id, out var context);
            if (!success)
            {
                jObject.Add($"Error {++errorCount}", $"Request {requestId} does not have an execution context.");
                return jObject;
            }
            foreach (var subRequest in context.SubRequests.Values)
            {
                var title = subRequest.Description;
                if (!subRequest.RequestId.HasValue)
                {
                    jObject.Add(title, "No request");
                }
                else
                {
                    var status = subRequest.StateAsString;//await GetStatusAsJsonAsync(subRequest.RequestId.Value);
                    jObject.Add(title, status);
                }
            }

            return await Task.FromResult(jObject);
        }
    }
}