using System;

namespace Xlent.Lever.AsyncCaller.Data.Models
{
    /// <summary>
    /// Represents a request for us to try to call the specified url and come back with the result
    /// </summary>
    public class RawRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public byte[] Context { get; set; }
        public byte[] CallOut { get; set; }
        public string CallOutUriScheme { get; set; }
        public byte[] CallBack { get; set; }
        public string CallBackUriScheme { get; set; }
        public int? Priority { get; set; }

        public string SetTitle(string method, Uri uri)
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
