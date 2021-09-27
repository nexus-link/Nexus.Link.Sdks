#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Serialization;

namespace WorkflowEngine.Sdk.Inbound.RespondAsync
{
    public interface IResponseHandler : IGetActionResult
    {
        Task AddResponse(RequestData requestData, ResponseData responseData);
        Task<ResponseData> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default);
    }
}
#endif