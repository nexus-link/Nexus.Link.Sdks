using System;
using System.Text.RegularExpressions;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade.Models
{
    /// <summary>
    /// Represents a request for us to try to call the specified url and come back with the result
    /// </summary>
    [Obsolete("Please use Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models.Request. Obsolete since 2020-09-10", true)]
    public class Request
    {
        public string Id { get; set; }
        public byte[] Context { get; set; }
        public int? Priority { get; set; }
        public RequestMessage CallOut { get; set; }
        public RequestMessage CallBack { get; set; }

        private string GetSchema(string uri)
        {
            if (uri == null) return null;
            var rgx = new Regex(@"^http[s]?");
            return rgx.Match(uri).Value;
        }

        public override string ToString()
        {
            return $"{Id} {CallOut?.Method} {CallOut?.RequestUri}";
        }
    }
}

