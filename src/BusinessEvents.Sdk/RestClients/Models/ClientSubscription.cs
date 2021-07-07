namespace Nexus.Link.BusinessEvents.Sdk.RestClients.Models
{
    public class ClientSubscription
    {
        public string EntityName { get; set; }
        public string EventName { get; set; }
        public int MajorVersion { get; set; }
        public int? Priority { get; set; }
        public string Url { get; set; }
    }
}
