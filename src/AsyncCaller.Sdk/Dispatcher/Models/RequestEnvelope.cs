using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.Common.Models;
using ResponseContent = Nexus.Link.AsyncCaller.Sdk.Dispatcher.Logic.ResponseContent;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models
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

        public RequestEnvelope(string organization, string environment, Request request, TimeSpan defaultDeadlineInSeconds)
        {
            Organization = organization;
            Environment = environment;
            Attempts = 0;
            CreatedAt = DateTimeOffset.Now;
            LatestAttemptAt = DateTimeOffset.MinValue;
            NextAttemptAt = DateTimeOffset.MinValue;
            DeadlineAt = DateTimeOffset.Now.Add(defaultDeadlineInSeconds);
            Request = request;
        }

        public static async Task<RequestEnvelope> GetResponseAsRequestEnvelopeAsync(RequestEnvelope requestEnvelope, HttpResponseMessage response, TimeSpan defaultDeadlineInSeconds, CancellationToken cancellationToken = default)
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
            request.CallOut.Content = new StringContent(dataAsync.Serialize(), Encoding.UTF8, "application/json");
            var responseEnvelope = new RequestEnvelope(requestEnvelope.Organization, requestEnvelope.Environment, request, defaultDeadlineInSeconds)
            {
                OriginalRequestId = requestEnvelope.OriginalRequestId
            };
            return await Task.FromResult(responseEnvelope);
        }

        public static async Task<RequestEnvelope> FromRawAsync(Data.Models.RawRequestEnvelope source, TimeSpan defaultDeadlineInSeconds, CancellationToken cancellationToken = default)
        {
            if (source == null) return null;
            var request = await Request.FromRawAsync(source.RawRequest, cancellationToken);
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new RequestEnvelope(source.Organization, source.Environment, request, defaultDeadlineInSeconds)
            {
                OriginalRequestId = source.OriginalRequestId,
                Attempts = source.Attempts,
                CreatedAt = source.CreatedAt,
                LatestAttemptAt = source.LatestAttemptAt,
                LatestResponse = await serializer.DeserializeToResponseAsync(source.LatestResponse, cancellationToken),
                DeadlineAt = source.DeadlineAt,
                NextAttemptAt = source.NextAttemptAt
            };
            return target;
        }

        public async Task<Data.Models.RawRequestEnvelope> ToRawAsync(CancellationToken cancellationToken = default)
        {
            var request = await Request.ToRawAsync(cancellationToken);
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new Data.Models.RawRequestEnvelope
            {
                Organization = Organization,
                Environment = Environment,
                RawRequest = request,
                OriginalRequestId = OriginalRequestId,
                Attempts = Attempts,
                CreatedAt = CreatedAt,
                LatestAttemptAt = LatestAttemptAt,
                LatestResponse = await serializer.SerializeAsync(LatestResponse, cancellationToken),
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