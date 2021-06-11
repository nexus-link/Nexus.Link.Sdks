using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IConceptsClient
    {

        Task<IEnumerable<Concept>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Concept> GetOneAsync(string conceptId, CancellationToken cancellationToken = default);

        Task<Concept> UpdateAsync(string conceptId, Concept concept, CancellationToken cancellationToken = default);

        Task<Concept> CreateAsync(string technicalName, CancellationToken cancellationToken = default);

        Task DeleteAsync(string conceptId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Context>> GetAllContextsAsync(string conceptId, CancellationToken cancellationToken = default);

        Task<Context> GetContextAsync(string conceptId, string contextId, CancellationToken cancellationToken = default);

        Task<Context> UpdateContextAsync(string conceptId, string contextId, Context context, CancellationToken cancellationToken = default);

        Task<Context> CreateContextAsync(string conceptId,  string technicalName, CancellationToken cancellationToken = default);

        Task DeleteContextAsync(string conceptId, string contextId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Form>> GetAllFormsAsync(string conceptId, CancellationToken cancellationToken = default);

        Task<Form> GetFormAsync(string conceptId, Guid formId, CancellationToken cancellationToken = default);

        Task<Form> UpdateFormAsync(string conceptId, Guid formId, Form form, CancellationToken cancellationToken = default);

        Task<Form> CreateFormAsync(string conceptId,  string friendlyName, CancellationToken cancellationToken = default);

        Task DeleteFormAsync(string conceptId, Guid formId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Instance>> GetAllInstancesByContextAsync(string conceptId, string contextId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Instance>> GetAllInstancesByFormAsync(string conceptId, Guid formId, CancellationToken cancellationToken = default);

        Task<Instance> GetInstanceAsync(string conceptId, string contextId, string value, CancellationToken cancellationToken = default);

        Task<Instance> CreateInstanceAsync(string conceptId, string contextId, Guid formId,  string value, CancellationToken cancellationToken = default);
    }
}
