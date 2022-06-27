using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Misc.Web.Sdk.Json;
using Nexus.Link.Misc.Web.Sdk.OutboundHandlers;

namespace Nexus.Link.Misc.Web.Sdk.RestClientHelper
{
    /// <summary>
    /// Convenience client for making REST calls
    /// </summary>
    public class NexusHttpSender : IHttpSender
    {
        private static readonly HttpMethod PatchMethod = new("PATCH");

        /// <summary>
        /// The options for the <see cref="HttpClient"/> for this <see cref="NexusHttpSender"/>.
        /// The constructor will initialize it according to <see cref="NexusLinkHandlerOptions"/> SetNexusLinkDefaults().
        /// </summary>
        public NexusLinkHandlerOptions NexusLinkHandlerOptions { get; private set; }

        /// <summary>
        /// This is the <see cref="HttpClient"/> that is used for all HTTP calls.
        /// It is always initialized to a new HttpClient with <see cref="NexusLinkHandler"/> as handler.
        /// That handler has <see cref="NexusLinkHandlerOptions"/> as options.
        /// </summary>
        /// <remarks>
        /// 
        /// You can modify its behavior by modifying the <see cref="NexusLinkHandlerOptions"/> or replace
        /// this HTTP client entirely with your own HTTP client. If you replace it, <see cref="NexusLinkHandlerOptions"/>
        /// will be set to null.
        /// </remarks>
        public HttpClient HttpClient { get; private set; }

        /// <inheritdoc />
        public Uri BaseUri { get; set; }

        /// <summary>
        /// Credentials that are used when sending requests to the service.
        /// </summary>
        public ServiceClientCredentials Credentials { get; }

        /// <summary>
        /// Json settings when serializing to strings
        /// </summary>
        public JsonSerializerSettings SerializationSettings { get; set; } = new JsonSerializerSettings()
            .SetAsNexusLink()
            .AddNexusWeb();

        /// <summary>
        /// Json settings when de-serializing from strings
        /// </summary>
        public JsonSerializerSettings DeserializationSettings { get; set; } = new JsonSerializerSettings()
            .SetAsNexusLink()
            .AddNexusWeb();

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        public NexusHttpSender(string baseUri, ServiceClientCredentials credentials = null)
        {
            Credentials = credentials;
            try
            {
                if (!string.IsNullOrWhiteSpace(baseUri))
                {
                    BaseUri = new Uri(baseUri);
                }
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(baseUri)} ({baseUri}) is not correct: {e.Message}");
            }

            var options = new NexusLinkHandlerOptions().SetNexusLinkDefaults();
            var handler = new NexusLinkHandler(options);
            HttpClient = new HttpClient(handler);
            NexusLinkHandlerOptions = options;
        }

        /// <summary></summary>
        /// <param name="httpClient">Use this as the <see cref="HttpClient"/>.</param>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        public NexusHttpSender(HttpClient httpClient, string baseUri, ServiceClientCredentials credentials = null)
        {
            InternalContract.RequireNotNull(httpClient, nameof(httpClient));
            Credentials = credentials;
            try
            {
                if (!string.IsNullOrWhiteSpace(baseUri))
                {
                    BaseUri = new Uri(baseUri);
                }
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(baseUri)} ({baseUri}) is not correct: {e.Message}");
            }

            HttpClient = httpClient;
            NexusLinkHandlerOptions = null;
        }

        /// <summary>
        /// The new http sender will share the same <see cref="HttpClient"/> and the same <see cref="NexusLinkHandlerOptions"/>.
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public IHttpSender CreateHttpSender(string relativeUrl)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));

            var newUri = string.IsNullOrWhiteSpace(relativeUrl) 
                ? GetAbsoluteUrl("") 
                : GetAbsoluteUrl(relativeUrl);
            var sender = new NexusHttpSender(newUri, Credentials)
            {
                HttpClient = HttpClient,
                NexusLinkHandlerOptions = NexusLinkHandlerOptions
            };
            return sender;
        }

        /// <inheritdoc />
        public async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await SendRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken).ConfigureAwait(false);
                var request = response.RequestMessage;
                return await HandleResponseWithBody<TResponse>(method, response, request, cancellationToken);
            }
            finally
            {
                response?.Dispose();
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = null;
            try
            {
                request = await CreateRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken);
                return await SendAsync(request, cancellationToken);
            }
            finally
            {
                request?.Dispose();
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = null;
            try
            {
                request = await CreateRequestAsync(method, relativeUrl, customHeaders);
                return await SendAsync(request, cancellationToken);
            }
            finally
            {
                request?.Dispose();
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            FulcrumAssert.IsNotNull(response);
            return response;
        }

        #region Helpers

        internal async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders)
        {
            var url = GetAbsoluteUrl(relativeUrl);
            var request = new HttpRequestMessage(method, url);
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            request.Headers.TryAddWithoutValidation("Accept", new List<string> { "application/json" });

            if (Credentials == null) return request;

            await Credentials.ProcessHttpRequestAsync(request, default).ConfigureAwait(false);
            return request;
        }

        private async Task<HttpRequestMessage> CreateRequestAsync<TBody>(HttpMethod method, string relativeUrl, TBody instance, Dictionary<string, List<string>> customHeaders,
            CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var request = await CreateRequestAsync(method, relativeUrl, customHeaders);

            if (instance != null)
            {
                var requestContent = JsonConvert.SerializeObject(instance, SerializationSettings);
                request.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                request.Content.Headers.ContentType =
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

            if (Credentials == null) return request;

            cancellationToken.ThrowIfCancellationRequested();
            await Credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
            return request;
        }


        private async Task<HttpOperationResponse<TResponse>> HandleResponseWithBody<TResponse>(HttpMethod method, HttpResponseMessage response,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<TResponse>
            {
                Request = request,
                Response = response,
                Body = default
            };

            // Simple case
            if (response.StatusCode == HttpStatusCode.NoContent) return result;

            await VerifySuccessAsync(response, cancellationToken);
            if (method == HttpMethod.Get || method == HttpMethod.Put || method == HttpMethod.Post || method == PatchMethod)
            {
                if ((method == HttpMethod.Get || method == HttpMethod.Put || method == PatchMethod) && response.StatusCode != HttpStatusCode.OK)
                {
                    throw new FulcrumResourceException($"The response to request {request.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK}, but had {response.StatusCode.ToLogString()}.");
                }

                if (method == HttpMethod.Post && response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                {
                    throw new FulcrumResourceException($"The response to request {request.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK} or {HttpStatusCode.Created}, but had {response.StatusCode.ToLogString()}.");
                }
                var responseContent = await TryGetContentAsString(response.Content, false, cancellationToken);
                if (responseContent == null) return result;
                try
                {
                    result.Body = JsonConvert.DeserializeObject<TResponse>(responseContent, DeserializationSettings);
                }
                catch (Exception e)
                {
                    throw new FulcrumResourceException($"The response to request {request.ToLogString()} could not be deserialized to the type {typeof(TResponse).FullName}. The content was:\r{responseContent}.", e);
                }
            }
            return result;
        }

        private async Task VerifySuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            InternalContract.RequireNotNull(response.RequestMessage, $"{nameof(response)}.{nameof(response.RequestMessage)}");
            if (!response.IsSuccessStatusCode)
            {
                var requestContent = await TryGetContentAsString(response.RequestMessage?.Content, true, cancellationToken);
                var responseContent = await TryGetContentAsString(response.Content, true, cancellationToken);
                var message = $"{response.StatusCode} {responseContent}";
                var exception = new HttpOperationException(message)
                {
                    Response = new HttpResponseMessageWrapper(response, responseContent),
                    Request = new HttpRequestMessageWrapper(response.RequestMessage, requestContent)
                };
                throw exception;
            }
        }

        private async Task<string> TryGetContentAsString(HttpContent content, bool silentlyIgnoreExceptions, CancellationToken cancellationToken)
        {
            if (content == null) return null;
            try
            {
                await content.LoadIntoBufferAsync();
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!silentlyIgnoreExceptions) throw new FulcrumAssertionFailedException("Expected to be able to read an HttpContent.", e);
            }
            return null;
        }

        /// <inheritdoc />
        public string GetAbsoluteUrl(string relativeUrl)
        {
            string baseUri = BaseUri?.OriginalString ?? HttpClient?.BaseAddress?.OriginalString ?? "";

            if (baseUri != "" && !string.IsNullOrWhiteSpace(relativeUrl))
            {
                var relativeUrlBeginsWithSpecialCharacter = relativeUrl.StartsWith("/") || relativeUrl.StartsWith("?");
                var slashIsRequired = !string.IsNullOrWhiteSpace(relativeUrl) && !relativeUrlBeginsWithSpecialCharacter;
                if (baseUri.EndsWith("/"))
                {
                    // Maybe remove the /
                    if (relativeUrlBeginsWithSpecialCharacter) baseUri = baseUri.Substring(0, baseUri.Length - 1);
                }
                else
                {
                    if (slashIsRequired) baseUri += "/";
                }
            }

            var concatenatedUrl = baseUri + relativeUrl?.Trim(' ');

            if (!(Uri.TryCreate(concatenatedUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            {
                InternalContract.Fail($"The format of the concatenated url ({concatenatedUrl}) is not correct. BaseUrl: '{baseUri}'. RelativeUrl: '{relativeUrl}'");
            }

            return concatenatedUrl;
        }
        #endregion
    }
}
