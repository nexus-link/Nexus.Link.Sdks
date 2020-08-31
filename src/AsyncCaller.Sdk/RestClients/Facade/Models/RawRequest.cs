using System;
using System.Net.Http;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade.Models
{
    /// <summary>
    /// Represents a request for us to try to call the specified url and come back with the result
    /// </summary>
    [Obsolete("Please use Xlent.Lever.AsyncCaller.Data.Models.RawRequest")]
    public class RawRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public byte[] Context { get; set; }
        public int? Priority { get; set; }
        public byte[] CallOut { get; set; }
        public string CallOutUriScheme { get; set; }
        public byte[] CallBack { get; set; }
        public string CallBackUriScheme { get; set; }

        public string SetTitle(HttpMethod method, Uri uri)
        {
            Title = $"{method} {uri} ({Id})";
            return Title;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
