using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public class ConceptsClient : BaseClient, IConceptsClient
    {
        public ConceptsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        public async Task<IEnumerable<Concept>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "/Concepts";
            var result = await RestClient.GetAsync<IEnumerable<Concept>>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Concept> GetOneAsync(string conceptId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}";
            var result = await RestClient.GetAsync<Concept>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Concept> UpdateAsync(string conceptId, Concept concept, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}";
            var result = await RestClient.PutAndReturnUpdatedObjectAsync(relativeUrl, concept, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Concept> CreateAsync(string technicalName, CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "/Concepts";
            var result = await RestClient.PostAsync<Concept, string>(relativeUrl, technicalName, cancellationToken: cancellationToken);
            return result;
        }

        public async Task DeleteAsync(string conceptId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}";
            await RestClient.DeleteAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<Context>> GetAllContextsAsync(string conceptId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts";
            var result = await RestClient.GetAsync<IEnumerable<Context>>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Context> GetContextAsync(string conceptId, string contextId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts/{contextId}";
            var result = await RestClient.GetAsync<Context>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Context> UpdateContextAsync(string conceptId, string contextId, Context context, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts/{contextId}";
            var result = await RestClient.PutAndReturnUpdatedObjectAsync(relativeUrl, context, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Context> CreateContextAsync(string conceptId, string technicalName, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts";
            var result = await RestClient.PostAsync<Context, string>(relativeUrl, technicalName, cancellationToken: cancellationToken);
            return result;
        }

        public async Task DeleteContextAsync(string conceptId, string contextId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts/{contextId}";
            await RestClient.DeleteAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<Form>> GetAllFormsAsync(string conceptId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Forms";
            var result = await RestClient.GetAsync<IEnumerable<Form>>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Form> GetFormAsync(string conceptId, Guid formId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Forms/{formId}";
            var result = await RestClient.GetAsync<Form>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Form> UpdateFormAsync(string conceptId, Guid formId, Form form, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Forms/{formId}";
            var result = await RestClient.PutAndReturnUpdatedObjectAsync(relativeUrl, form, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Form> CreateFormAsync(string conceptId, string friendlyName, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Forms";
            var result = await RestClient.PostAsync<Form, string>(relativeUrl, friendlyName, cancellationToken: cancellationToken);
            return result;
        }

        public async Task DeleteFormAsync(string conceptId, Guid formId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Forms/{formId}";
            await RestClient.DeleteAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<Instance>> GetAllInstancesByContextAsync(string conceptId, string contextId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts/{contextId}/Instances";
            var result = await RestClient.GetAsync<IEnumerable<Instance>>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<IEnumerable<Instance>> GetAllInstancesByFormAsync(string conceptId, Guid formId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Forms/{formId}/Instances";
            var result = await RestClient.GetAsync<IEnumerable<Instance>>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Instance> GetInstanceAsync(string conceptId, string contextId, string value, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts/{contextId}/Instances/{value}";
            var result = await RestClient.GetAsync<Instance>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Instance> CreateInstanceAsync(string conceptId, string contextId, Guid formId, string value, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Concepts/{conceptId}/Contexts/{contextId}/Forms/{formId}/Instances";
            var result = await RestClient.PostAsync<Instance, string>(relativeUrl, value, cancellationToken: cancellationToken);
            return result;
        }
    }
}
