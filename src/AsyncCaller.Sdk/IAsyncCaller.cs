using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;

#pragma warning disable 1591

namespace Nexus.Link.AsyncCaller.Sdk
{
    public interface IAsyncCaller
    {
        IAsyncCall CreateCall(HttpMethod method, Uri uri);
        IAsyncCall CreateCall(HttpMethod method, Uri uri, int priority);
        IAsyncCall CreateCall(HttpMethod method, string uri);
        IAsyncCall CreateCall(HttpMethod method, string uri, int priority);
        Task<string> ExecuteAsync(RawRequest rawRequest);
    }
}