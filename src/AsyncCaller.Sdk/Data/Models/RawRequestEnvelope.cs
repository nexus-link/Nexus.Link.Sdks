using System;
using Newtonsoft.Json;
using Nexus.Link.AsyncCaller.Sdk.Common.Helpers;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.AsyncCaller.Sdk.Data.Models
{
    public class RawRequestEnvelope
    {
       public static RawRequestEnvelope Create(Tenant tenant, ILeverConfiguration config, RawRequest rawRequest)
        {
            var timeSpan = ConfigurationHelper.GetDefaultDeadlineTimeSpan(config);
            var envelope = new RawRequestEnvelope
            {
                Organization = tenant.Organization,
                Environment = tenant.Environment,
                OriginalRequestId = rawRequest.Id,
                RawRequest = rawRequest,
                Attempts = 0,
                CreatedAt = DateTimeOffset.Now,
                LatestAttemptAt = DateTimeOffset.MinValue,
                NextAttemptAt = DateTimeOffset.MinValue,
                DeadlineAt = DateTimeOffset.Now.Add(timeSpan)
            };
            return envelope;
        }

        public string Organization { get; set; }
        public string Environment { get; set; }
        public string OriginalRequestId { get; set; }
        public int Attempts { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LatestAttemptAt { get; set; }
        public byte[] LatestResponse { get; set; }
        public DateTimeOffset DeadlineAt { get; set; }
        public DateTimeOffset NextAttemptAt { get; set; }
        public RawRequest RawRequest { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static RawRequestEnvelope Deserialize(string serializedString)
        {
            return JsonConvert.DeserializeObject<RawRequestEnvelope>(serializedString);
        }

        public override string ToString()
        {
            return $"{OriginalRequestId}: {RawRequest}";
        }
    }
}