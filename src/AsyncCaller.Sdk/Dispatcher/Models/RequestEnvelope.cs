using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Common.Configuration;
using Nexus.Link.AsyncCaller.Common.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Threads;
using ResponseContent = Nexus.Link.AsyncCaller.Dispatcher.Logic.ResponseContent;

namespace Nexus.Link.AsyncCaller.Dispatcher.Models
{
    public class RequestEnvelope
    {
        public string Organization { get; set; }
        public string Environment { get; set; }
        public string OriginalRequestId { get; set; }
        public int Attempts { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LatestAttemptAt { get; set; }
        public HttpResponseMessage LatestResponse { get; set; }
        public DateTimeOffset DeadlineAt { get; set; }
        public DateTimeOffset NextAttemptAt { get; set; }
        public Request Request { get; set; }

        public RequestEnvelope(string organization, string environment, Request request)
        {
            Organization = organization;
            Environment = environment;
            Attempts = 0;
            CreatedAt = DateTimeOffset.Now;
            LatestAttemptAt = DateTimeOffset.MinValue;
            NextAttemptAt = DateTimeOffset.MinValue;
            var tenant = new Tenant(organization, environment);
            //var timeSpan = ConfigurationHandler.GetDefaultDeadlineTimeSpanAsync(tenant).Result;
            var timeSpan = ThreadHelper.CallAsyncFromSync(async () => await ConfigurationHandler.GetDefaultDeadlineTimeSpanAsync(tenant));
            DeadlineAt = DateTimeOffset.Now.Add(timeSpan);
            Request = request;
        }

        public static async Task<RequestEnvelope> GetResponseAsRequestEnvelopeAsync(RequestEnvelope requestEnvelope, HttpResponseMessage response)
        {
            var request = new Request
            {
                Id = Guid.NewGuid().ToString(),
                CallOut = requestEnvelope.Request.CallBack,
                CallBack = null
            };
            var responseContent = new ResponseContent
            {
                Id = requestEnvelope.OriginalRequestId,
                StatusCode = response.StatusCode,
                Context = requestEnvelope.Request.Context,
                Payload = response.Content
            };
            var dataAsync = await responseContent.ToDataAsync();
            request.CallOut.Content = new StringContent(dataAsync.Serialize(), Encoding.UTF8, dataAsync.PayloadMediaType);
            var responseEnvelope = new RequestEnvelope(requestEnvelope.Organization, requestEnvelope.Environment, request)
            {
                OriginalRequestId = requestEnvelope.OriginalRequestId
            };
            return await Task.FromResult(responseEnvelope);
        }

        public static async Task<RequestEnvelope> FromDataAsync(Xlent.Lever.AsyncCaller.Data.Models.RequestEnvelope source)
        {
            if (source == null) return null;
            var request = await Request.FromDataAsync(source.RawRequest);
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new RequestEnvelope(source.Organization, source.Environment, request)
            {
                OriginalRequestId = source.OriginalRequestId,
                Attempts = source.Attempts,
                CreatedAt = source.CreatedAt,
                LatestAttemptAt = source.LatestAttemptAt,
                LatestResponse = await serializer.DeserializeToResponseAsync(source.LatestResponse),
                DeadlineAt = source.DeadlineAt,
                NextAttemptAt = source.NextAttemptAt
            };
            return target;
        }

        public async Task<Xlent.Lever.AsyncCaller.Data.Models.RequestEnvelope> ToDataAsync()
        {
            var request = await Request.ToDataAsync();
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new Xlent.Lever.AsyncCaller.Data.Models.RequestEnvelope
            {
                Organization = Organization,
                Environment = Environment,
                RawRequest = request,
                OriginalRequestId = OriginalRequestId,
                Attempts = Attempts,
                CreatedAt = CreatedAt,
                LatestAttemptAt = LatestAttemptAt,
                LatestResponse = await serializer.SerializeAsync(LatestResponse),
                DeadlineAt = DeadlineAt,
                NextAttemptAt = NextAttemptAt
            };
            return target;
        }

        public override string ToString()
        {
            return $"{OriginalRequestId}: {Request}";
        }
    }
}