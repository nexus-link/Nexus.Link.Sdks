using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IConceptsClient
    {

        Task<IEnumerable<Concept>> GetAllAsync();

        Task<Concept> GetOneAsync(string conceptId);

        Task<Concept> UpdateAsync(string conceptId, Concept concept);

        Task<Concept> CreateAsync(string technicalName);

        Task DeleteAsync(string conceptId);

        Task<IEnumerable<Context>> GetAllContextsAsync(string conceptId);

        Task<Context> GetContextAsync(string conceptId, string contextId);

        Task<Context> UpdateContextAsync(string conceptId, string contextId, Context context);

        Task<Context> CreateContextAsync(string conceptId,  string technicalName);

        Task DeleteContextAsync(string conceptId, string contextId);

        Task<IEnumerable<Form>> GetAllFormsAsync(string conceptId);

        Task<Form> GetFormAsync(string conceptId, Guid formId);

        Task<Form> UpdateFormAsync(string conceptId, Guid formId, Form form);

        Task<Form> CreateFormAsync(string conceptId,  string friendlyName);

        Task DeleteFormAsync(string conceptId, Guid formId);

        Task<IEnumerable<Instance>> GetAllInstancesByContextAsync(string conceptId, string contextId);

        Task<IEnumerable<Instance>> GetAllInstancesByFormAsync(string conceptId, Guid formId);

        Task<Instance> GetInstanceAsync(string conceptId, string contextId, string value);

        Task<Instance> CreateInstanceAsync(string conceptId, string contextId, Guid formId,  string value);
    }
}
