#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace WorkflowEngine.Sdk.Inbound.RespondAsync.Logic
{
    /// <summary>
    /// Normally you should be able to use this class without overriding it.
    /// </summary>
    public class RespondAsyncHostedService : BackgroundService
    {
        public IExecuteAsync ExecuteAsyncSupport { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RespondAsyncHostedService(IExecuteAsync executeAsyncSupport)
        {
            ExecuteAsyncSupport = executeAsyncSupport;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var requestData = await ExecuteAsyncSupport.RequestQueue.DequeueAsync(cancellationToken);

                try
                {
                    var responseData =
                        await ExecuteAsyncSupport.RequestExecutor.ExecuteRequestAsync(requestData, cancellationToken);
                    await ExecuteAsyncSupport.ResponseHandler.AddResponse(requestData, responseData);
                }
                catch (FulcrumAcceptedException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    // The method above should never fail, and if it does we will just log that and ignore the error
                    Log.LogError($"The method {nameof(IRequestExecutor)}.{nameof(IRequestExecutor.ExecuteRequestAsync)} should never throw an exception, but it did.:\r{e.Message}", e);
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            var task = ExecuteAsyncSupport.RequestQueue.StopAsync(stoppingToken);
            await base.StopAsync(stoppingToken);
            await task;
        }
    }
}
#endif