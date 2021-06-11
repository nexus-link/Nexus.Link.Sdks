using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.Cache;
using Nexus.Link.KeyTranslator.Sdk.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.KeyTranslator.Sdk
{
    /// <summary>
    /// Translate a batch of values.
    /// </summary>
    public class BatchTranslate : IBatchTranslate
    {
        private static readonly TranslateResponseCache TranslateResponseCache;
        private readonly ConcurrentDictionary<string, TranslateRequest> _translateRequests;
        private readonly ConcurrentDictionary<string, TranslateResponse> _translateResponses;
        private readonly ConcurrentDictionary<string, List<Action<string>>> _actions;

        /// <inheritdoc />
        public string SourceClientName { get; }

        /// <inheritdoc />
        public string TargetClientName { get; }

        static BatchTranslate()
        {
            var cacheMinutesString = (string)null; // TODO ConfigurationManager.AppSettings["KeyTranslatorClientCacheMinutes"];
            var cacheMinutes = string.IsNullOrEmpty(cacheMinutesString) ? 5 : int.Parse(cacheMinutesString);
            var cachePhysicalMemoryLimitPercentageString = (string)null; // TODO ConfigurationManager.AppSettings["KeyTranslatorCachePhysicalMemoryLimitPercentage"];
            var cachePhysicalMemoryLimitPercentage = string.IsNullOrEmpty(cachePhysicalMemoryLimitPercentageString) ? 10 : int.Parse(cachePhysicalMemoryLimitPercentageString);
            TranslateResponseCache = new TranslateResponseCache(cacheMinutes, cachePhysicalMemoryLimitPercentage);
        }

        /// <summary>
        /// Empty the cache.
        /// </summary>
        public static void ResetCache()
        {
            TranslateResponseCache.ResetCache();
        }

        /// <summary>
        /// Use this constructor when all batch items have specified source and target contexts
        /// </summary>
        /// <param name="translateClient">The service client that will do the actual translations.</param>
        public BatchTranslate(ITranslateClient translateClient)
        {
            InternalContract.RequireNotNull(translateClient, nameof(translateClient));
            TranslateClient = translateClient;
            _translateRequests = new ConcurrentDictionary<string, TranslateRequest>();
            _translateResponses = new ConcurrentDictionary<string, TranslateResponse>();
            _actions = new ConcurrentDictionary<string, List<Action<string>>>();
        }

        /// <summary>
        /// Use this constructor when you have a source and target client.
        /// </summary>
        /// <param name="translateClient">The service client that will do the actual translations.</param>
        /// <param name="sourceClientName">The name of the source client, i.e. the client that created the values that should be translated.</param>
        /// <param name="targetClientName">The name of the target client, i.e. the client whose domain that we will translate the values to.</param>
        public BatchTranslate(ITranslateClient translateClient, string sourceClientName, string targetClientName)
            : this(translateClient)
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceClientName, nameof(sourceClientName));
            InternalContract.RequireNotNullOrWhiteSpace(targetClientName, nameof(targetClientName));
            SourceClientName = sourceClientName;
            TargetClientName = targetClientName;
        }

        /// <summary>
        /// 
        /// </summary>
        public ITranslateClient TranslateClient { get; }

        /// <inheritdoc />
        public BatchTranslate Add(string concept, string sourceValue, Action<string> action = null)
        {
            InternalContract.RequireNotNullOrWhiteSpace(concept, nameof(concept));
            var translateRequest = new TranslateRequest
            {
                SourceInstancePath = $"({concept}!~{SourceClientName}!{sourceValue})",
                TargetContextPath = $"({concept}!~{TargetClientName})"
            };
            return AddRequest(concept, sourceValue, translateRequest, action);
        }

        /// <inheritdoc />
        public BatchTranslate AddWithContexts(string concept, string sourceContext, string sourceValue, string targetContext, Action<string> action = null)
        {
            InternalContract.RequireNotNullOrWhiteSpace(concept, nameof(concept));
            InternalContract.RequireNotNullOrWhiteSpace(sourceContext, nameof(sourceContext));
            InternalContract.RequireNotNullOrWhiteSpace(targetContext, nameof(targetContext));

            var translateRequest = new TranslateRequest
            {
                SourceInstancePath = $"({concept}!{sourceContext}!{sourceValue})",
                TargetContextPath = $"({concept}!{targetContext})"
            };
            return AddRequest(concept, sourceValue, translateRequest, action);
        }

        private BatchTranslate AddRequest(string concept, string sourceValue, TranslateRequest translateRequest,
            Action<string> action = null)
        {
            var index = GetIndex(concept, sourceValue);
            _translateRequests.TryAdd(index, translateRequest);
            if (action != null)
            {
                var actionList = _actions.GetOrAdd(index, (i) => new List<Action<string>>());
                actionList.Add(action);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (_translateRequests.IsEmpty) return;
            var serviceRequestKeys = GetRequestKeysNotInCache(_translateRequests.Keys);
            if (serviceRequestKeys.Any())
            {
                var responses = await CallKeyTranslatorAsync(serviceRequestKeys);
                CollectAndCacheResponses(serviceRequestKeys, responses);
            }
            CallActions();
        }

        /// <inheritdoc />
        public string this[string concept, string sourceValue]
        {
            get
            {
                InternalContract.RequireNotNullOrWhiteSpace(concept, nameof(concept));
                var translateResponse = GetTranslateResponse(GetIndex(concept, sourceValue));
                return translateResponse?.Value;
            }
        }

        private List<string> GetRequestKeysNotInCache(IEnumerable<string> requestKeys)
        {
            InternalContract.RequireNotNull(requestKeys, nameof(requestKeys));
            var serviceRequestKeys = new List<string>();

            foreach (var key in requestKeys)
            {
                InternalContract.RequireNotNullOrWhiteSpace(key, nameof(requestKeys), $"{nameof(requestKeys)} must not contain empty or null strings.");
                var translateRequest = GetTranslateRequest(key);
                var cacheIndex = GetCacheIndex(translateRequest);
                lock (TranslateResponseCache)
                {
                    var response = TranslateResponseCache.Get(cacheIndex);
                    if (response == null)
                    {
                        serviceRequestKeys.Add(key);
                    }
                    else
                    {
                        _translateResponses.TryAdd(key, response);
                        TranslateResponseCache.Refresh(key, response);
                    }
                }
            }
            return serviceRequestKeys;
        }

        private async Task<Dictionary<string, TranslateResponse>> CallKeyTranslatorAsync(IEnumerable<string> serviceRequestKeys, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(serviceRequestKeys, nameof(serviceRequestKeys));
            FulcrumAssert.IsNotNull(TranslateClient);
            var result =
                await TranslateClient.TranslateBatchAsync(serviceRequestKeys.Select(k => TranslateRequest.ToFacade(GetTranslateRequest(k))), cancellationToken);
            FulcrumAssert.IsNotNull(result);
            var translateResponses = new Dictionary<string, TranslateResponse>();
            foreach (var item in result)
            {
                var translateResponse = TranslateResponse.FromFacade(item);
                var key = GetIndex(translateResponse.Request.SourceInstancePath);
                translateResponses[key] = translateResponse;
            }
            return translateResponses;
        }

        private void CollectAndCacheResponses(IEnumerable<string> serviceRequestKeys, Dictionary<string, TranslateResponse> responses)
        {
            InternalContract.RequireNotNull(serviceRequestKeys, nameof(serviceRequestKeys));
            InternalContract.RequireNotNull(responses, nameof(responses));
            foreach (var key in serviceRequestKeys)
            {
                var translateRequest = GetTranslateRequest(key);
                FulcrumAssert.IsNotNull(translateRequest);
                FulcrumAssert.IsTrue(responses.ContainsKey(key));
                var response = responses[key];
                _translateResponses.TryAdd(key, response);

                if (InstanceInfo.IsInstanceInfo(response.Value))
                {
                    // Don't cache if no translation was found. See FreshDesk issue 2292.
                    continue;
                }
                lock (TranslateResponseCache)
                {
                    TranslateResponseCache.Add(GetCacheIndex(translateRequest), response);
                }
            }
        }

        private void CallActions()
        {
            FulcrumAssert.IsNotNull(_translateRequests);
            foreach (var key in _translateRequests.Keys)
            {
                if (!_actions.TryGetValue(key, out var actionList)) continue;
                var translateResponse = GetTranslateResponse(key);
                var targetValue = translateResponse?.Value;
                foreach (var action in actionList)
                {
                    action(targetValue);
                }
            }
        }

        private string GetCacheIndex(TranslateRequest translateRequest)
        {
            InternalContract.RequireNotNull(translateRequest, nameof(translateRequest));
            FulcrumAssert.IsNotNull(TranslateClient);
            var organization = TranslateClient.Organization ?? "";
            var environment = TranslateClient.Environment ?? "";
            return $"{organization}/{environment}/{translateRequest.SourceInstancePath}->{translateRequest.TargetContextPath}";
        }

        private static string GetIndex(string path)
        {
            var instanceInfo = InstanceInfo.Parse(path);
            return GetIndex(instanceInfo.ConceptName, instanceInfo.Value);
        }

        private static string GetIndex(string concept, string sourceValue)
        {
            InternalContract.RequireNotNullOrWhiteSpace(concept, nameof(concept));
            return $"({concept}!!{sourceValue})";
        }

        private TranslateRequest GetTranslateRequest(string key)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            FulcrumAssert.IsTrue(_translateRequests.TryGetValue(key, out var translateRequest));
            FulcrumAssert.IsNotNull(translateRequest);
            return translateRequest;
        }

        private TranslateResponse GetTranslateResponse(string key)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            FulcrumAssert.IsTrue(_translateResponses.TryGetValue(key, out var translateResponse));
            FulcrumAssert.IsNotNull(translateResponse);
            return translateResponse;
        }
    }
}
