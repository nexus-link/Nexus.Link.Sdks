using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace AsyncManager.Sdk.UnitTests
{
    public class AsyncHttpRequestTests
    {
        public AsyncHttpRequestTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AsyncRequestClientTests));
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CorrectRequests), MemberType = typeof(TestDataGenerator))]
        public void CorrectHttpRequest(AsyncHttpRequest correctRequest)
        {
            // act & assert
            FulcrumAssert.IsValidated(correctRequest);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.IncorrectRequests), MemberType = typeof(TestDataGenerator))]
        public void IncorrectHttpRequest(AsyncHttpRequest incorrectRequest)
        {
            // act & assert
            Assert.Throws<FulcrumAssertionFailedException>(() =>
            {
                FulcrumAssert.IsValidated(incorrectRequest);
            });
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CorrectHeaders), MemberType = typeof(TestDataGenerator))]
        public void AddHeaders(Dictionary<string, StringValues> expectedHeaders)
        {
            // arrange
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            foreach (var header in expectedHeaders)
            {
                if (header.Value.Count == 1)
                {
                    request.AddHeader(header.Key, header.Value[0]);
                }
                else
                {
                    request.AddHeader(header.Key, header.Value.ToArray());
                }
            }

            // assert
            Assert.NotNull(request);
            AssertHelper.AssertEqual(expectedHeaders, request.Headers);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CorrectContent), MemberType = typeof(TestDataGenerator))]
        public void SetContentAsJson(object content)
        {
            // arrange
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetContentAsJson(content);

            // assert
            Assert.Equal(content == null ? null : JsonConvert.SerializeObject(content, Formatting.Indented), request.Content);
        }

        [Fact]
        public void SetContentFromJToken()
        {
            // arrange
            var request = TestDataGenerator.DefaultAsyncHttpRequest;
            var o = new {A = "a", B = 23 };
            var content = JToken.FromObject(o);

            // act
            request.SetContent(content);

            // assert
            Assert.Equal(content == null ? null : JsonConvert.SerializeObject(content, Formatting.Indented), request.Content);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CorrectContent), MemberType = typeof(TestDataGenerator))]
        public void SetContentAsApplicationJson(object content)
        {
            // arrange
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetContent(content == null ? null : JsonConvert.SerializeObject(content), "application/json");

            // assert
            Assert.Equal(content == null ? null : JsonConvert.SerializeObject(content), request.Content);
        }

        [Theory]
        [InlineData("random")]
        [InlineData("text/html")]
        [InlineData("application/html")]
        public void SetContentNotSupported(string notSupportedContentType)
        {
            // arrange
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act & assert
            Assert.Throws<FulcrumNotImplementedException>(() => request.SetContent("test", notSupportedContentType));
        }

        [Fact]
        public void SetExecuteBeforeAbsolute()
        {
            // arrange
            var expectedTime = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(1);
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetExecuteBefore(expectedTime);

            // assert
            Assert.Equal(expectedTime, request.Metadata.ExecuteBefore);
        }

        [Fact]
        public void SetExecuteAfterAbsolute()
        {
            // arrange
            var expectedTime = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(1);
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetExecuteAfter(expectedTime);

            // assert
            Assert.Equal(expectedTime, request.Metadata.ExecuteAfter);
        }

        [Fact]
        public void SetExecuteBeforeRelative()
        {
            // arrange
            var relative = TimeSpan.FromMinutes(1);
            var expectedTime = DateTimeOffset.UtcNow + relative;
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetExecuteBefore(relative);

            // assert
            AssertHelper.AssertEqual(expectedTime, request.Metadata.ExecuteBefore, TimeSpan.FromSeconds(3));
        }

        [Fact]
        public void SetExecuteAfterRelative()
        {
            // arrange
            var relative = TimeSpan.FromMinutes(1);
            var expectedTime = DateTimeOffset.UtcNow + relative;
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetExecuteAfter(relative);

            // assert
            AssertHelper.AssertEqual(expectedTime, request.Metadata.ExecuteBefore, TimeSpan.FromSeconds(3));
        }

        [Fact]
        public void SetCallbackUrlString()
        {
            // arrange
            var expectedUrl = "https://example.com/3CBC6953-9D5B-4390-B1D1-283E145DB66C";
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetCallbackUrl(expectedUrl);

            // assert
            Assert.Equal(expectedUrl, request.Metadata.Callback?.Url);
        }

        [Fact]
        public void SetCallbackUrlUri()
        {
            // arrange
            var expectedUrl = "https://example.com/3CBC6953-9D5B-4390-B1D1-283E145DB66C";
            var uri = new Uri(expectedUrl);
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetCallbackUrl(uri);

            // assert
            Assert.Equal(expectedUrl, request.Metadata.Callback?.Url);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CorrectHeaders), MemberType = typeof(TestDataGenerator))]
        public void AddCallbackHeaders(Dictionary<string, StringValues> expectedHeaders)
        {
            // arrange
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            foreach (var header in expectedHeaders)
            {
                if (header.Value.Count == 1)
                {
                    request.AddCallbackHeader(header.Key, header.Value[0]);
                }
                else
                {
                    request.AddCallbackHeader(header.Key, header.Value.ToArray());
                }
            }

            // assert
            Assert.NotNull(request);
            AssertHelper.AssertEqual(expectedHeaders, request.Metadata.Callback?.Headers);
        }

        [Fact]
        public void SetCallbackContext()
        {
            // arrange
            var context = "59447406-AF34-4CB4-8A74-0391181FD126";
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetCallbackContext(context);

            // assert
            Assert.Equal(context, request.Metadata.Callback?.Context);
        }

        [Fact]
        public void SetCallbackContextAsJson()
        {
            // arrange
            var context = new {A = "a", B = 43};
            var request = TestDataGenerator.DefaultAsyncHttpRequest;

            // act
            request.SetCallbackContextAsJson(context);

            // assert
            Assert.Equal(JsonConvert.SerializeObject(context), request.Metadata.Callback?.Context);
        }
    }
}
