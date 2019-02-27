using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.AsyncCaller.Sdk
{
    /// <summary>
    /// Methods for configuring an asynchronous call.
    /// </summary>
    public interface IAsyncCall
    {
        /// <summary>
        /// The request that is going out
        /// </summary>
        HttpRequestMessage CallOut { get; }
        /// <summary>
        /// The callback request
        /// </summary>
        HttpRequestMessage CallBack { get; }
        /// <summary>
        /// The context to use in the call back request.
        /// </summary>
        JToken Context { get; set; }
        /// <summary>
        /// Sets the URI for the callback to the value in <paramref name="uri"/>, and the method to POST.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetCallback(Uri uri);
        /// <summary>
        /// Sets the URI for the callback to the value in <paramref name="uri"/>, and the method to POST.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetCallback(string uri);
        /// <summary>
        /// Sets the URI for the callback to the value in <paramref name="uri"/>, and the method to the value in <paramref name="method"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetCallback(HttpMethod method, Uri uri);
        /// <summary>
        /// Sets the URI for the callback to the value in <paramref name="uri"/>, and the method to the value in <paramref name="method"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetCallback(HttpMethod method, string uri);
        /// <summary>
        /// Set the request content to <paramref name="content"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetJsonContent(JToken content);
        /// <summary>
        /// Set the request content to <paramref name="content"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetTextContent(string content);
        /// <summary>
        /// Set the request content to <paramref name="content"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetXmlContent(string content);
        /// <summary>
        /// Set the callback context to <paramref name="context"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetContext(JToken context);
        /// <summary>
        /// Set the id for this request to <paramref name="id"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetId(string id);
        /// <summary>
        /// Set the id for this request to <paramref name="id"/>.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall SetId(Guid id);
        /// <summary>
        /// Add a request header.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall AddOutHeader(string name, string value);
        /// <summary>
        /// Add a callback header.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall AddCallbackHeader(string name, string value);
        /// <summary>
        /// Add a request authorization header.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall AddOutAuthorization(string value);
        /// <summary>
        /// Add a callback authorization header.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall AddCallbackAuthorization(string value);
        /// <summary>
        /// Add a request bearer authorization header.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall AddOutBearerAuthorization(string token);
        /// <summary>
        /// Add a callback bearer authorization header.
        /// </summary>
        /// <returns>"this", to allow for chained calls.</returns>
        IAsyncCall AddCallbackBearerAuthorization(string token);

        /// <summary>
        /// Execute the configured asynchronous call.
        /// </summary>
        /// <returns></returns>
        Task<string> ExecuteAsync();
    }
}