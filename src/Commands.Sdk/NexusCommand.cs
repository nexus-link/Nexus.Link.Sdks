using System;
using Newtonsoft.Json;

namespace Nexus.Link.Commands.Sdk
{
    public class NexusCommand
    {
        public string Id { get; set; }
        public long SequenceNumber { get; set; }
        public string Command { get; set; }
        public string Originator { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}