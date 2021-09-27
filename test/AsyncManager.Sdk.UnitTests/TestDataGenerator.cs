using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Nexus.Link.AsyncManager.Sdk;

namespace AsyncManager.Sdk.UnitTests
{
    public class TestDataGenerator
    {
        public static HttpMethod DefaultMethod => HttpMethod.Get;
        public static string DefaultUrl => "https://example.com/Data";
        public static double DefaultPriority => 0.5;

        public static AsyncHttpRequest DefaultAsyncHttpRequest =>
            new AsyncHttpRequest_ForTest(DefaultMethod, DefaultUrl, DefaultPriority);

        public static AsyncHttpRequest CreateDefaultAsyncHttpRequest(IAsyncRequestClient client)
        {
            return new AsyncHttpRequest_ForTest(client, DefaultMethod, DefaultUrl, DefaultPriority);
        }

        public static IEnumerable<object[]> IncorrectUrls()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { "http:/example.com" };
        }
        public static IEnumerable<object[]> IncorrectPriorities()
        {
            yield return new object[] { -0.1 };
            yield return new object[] { 1.1 };
        }

        public static IEnumerable<object[]> CorrectRequests()
        {
            var request = DefaultAsyncHttpRequest;
            yield return new object[] { request };
        }

        public static IEnumerable<object[]> IncorrectRequests()
        {
            var request = DefaultAsyncHttpRequest;
            request.Metadata = null;
            yield return new object[] { request };
        }

        public static IEnumerable<object[]> CorrectHeaders()
        {
            var headers = new Dictionary<string, StringValues>
            {
                {"header1", "single value"}
            };
            yield return new object[] { headers };

            headers = new Dictionary<string, StringValues>
            {
                {"header1", new[] {"two", "values"}}
            };
            yield return new object[] { headers };

            headers = new Dictionary<string, StringValues>
            {
                {"header1", "single value"},
                {"header2", new[] {"two", "values"}}
            };
            yield return new object[] { headers };
        }

        public static IEnumerable<object[]> CorrectContent()
        {
            yield return new object[] {null};
            var o = new {A = "a", B = 23 };
            yield return new object[] {o};
            yield return new object[] {JToken.FromObject(o)};
            yield return new object[] {new [] {o, o}};
        }
    }
}