    #if NETCOREAPP
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Nexus.Link.Libraries.Web.Error.Logic;
    using Nexus.Link.Libraries.Web.Serialization;

    namespace Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync.Logic
{
    

    public abstract class RequestExecutorBase : IRequestExecutor
    {
        public static string IsRunningAsynchronouslyHeader { get; protected set; } = "X-Nexus-Is-Running-Asynchronously";

        public HttpClient HttpClient { get; protected set; }

        protected RequestExecutorBase(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public virtual bool IsRunningAsynchronously(HttpRequest request)
        {
            return request.Headers.ContainsKey(IsRunningAsynchronouslyHeader);
        }

        /// <inheritdoc />
        public virtual async Task<ResponseData> 
            ExecuteRequestAsync(RequestData requestData, CancellationToken cancellationToken = default)
        {
            ResponseData responseData;
            requestData.Headers.Add(IsRunningAsynchronouslyHeader, "TRUE");
            var requestMessage = requestData.ToHttpRequestMessage();
            try
            {
                var response = await HttpClient.SendAsync(requestMessage, cancellationToken);
                // Serialize the response and make it available to the caller
                responseData = await new ResponseData().FromAsync(response);
            }
            catch (FulcrumAcceptedException)
            {
                // Forward
                throw;
            }
            catch (Exception e)
            {
                responseData = new ResponseData().From(e);
            }

            return responseData;
        }
    }
}
#endif