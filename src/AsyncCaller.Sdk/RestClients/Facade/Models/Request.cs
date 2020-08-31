using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade.Models
{
    /// <summary>
    /// Represents a request for us to try to call the specified url and come back with the result
    /// </summary>
    public class Request
    {
        public string Id { get; set; }
        public byte[] Context { get; set; }
        public int? Priority { get; set; }
        public RequestMessage CallOut { get; set; }
        public RequestMessage CallBack { get; set; }

        public static async Task<Request> FromDataAsync(Xlent.Lever.AsyncCaller.Data.Models.RawRequest source)
        {
            if (source == null) return null;
            return new Request
            {
                Id = source.Id,
                CallOut = await RequestMessage.FromDataAsync(source.CallOut, source.CallOutUriScheme),
                CallBack = await RequestMessage.FromDataAsync(source.CallBack, source.CallBackUriScheme),
                Context = source.Context,
                Priority = source.Priority
            };
        }
        public async Task<Xlent.Lever.AsyncCaller.Data.Models.RawRequest> ToDataAsync()
        {

            return new Xlent.Lever.AsyncCaller.Data.Models.RawRequest
            {
                Id = Id,
                CallOut = CallOut == null ? null : await CallOut.ToDataAsync(),
                CallOutUriScheme = GetSchema(CallOut?.RequestUri),
                CallBack = CallBack == null ? null : await CallBack.ToDataAsync(),
                CallBackUriScheme = GetSchema(CallBack?.RequestUri),
                Context = Context,
                Priority = Priority
            };
        }

        private string GetSchema(string uri)
        {
            if (uri == null) return null;
            Regex rgx = new Regex(@"^http[s]?");
            return rgx.Match(uri).Value;
        }

        public override string ToString()
        {
            return $"{Id} {CallOut?.Method} {CallOut?.RequestUri}";
        }
    }
}

