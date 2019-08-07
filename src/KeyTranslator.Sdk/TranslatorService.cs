using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.KeyTranslator.Sdk
{
    /// <inheritdoc />
    public class TranslatorService : ITranslatorService
    {
        private readonly ITranslateClient _translateClient;
        
        /// <inheritdoc />
        public TranslatorService(ITranslateClient translateClient)
        {
            _translateClient = translateClient;
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, string>> TranslateAsync(IEnumerable<string> conceptValuePaths, string targetClientName)
        {
            var translateRequests = conceptValuePaths
                .Select(path => new TranslateRequest(path, GetTargetContextPath(path, targetClientName)));
            
            var result = await _translateClient.TranslateBatchAsync(translateRequests);
            return result.ToDictionary(item => item.Request.SourceInstancePath, item => item.Value);
        }

        private string GetTargetContextPath(string sourceContextPath, string targetClientName)
        {
            var conceptValue = ConceptValue.Parse(sourceContextPath);
            return $"({conceptValue.ConceptName}!~{targetClientName})";
        }
    }
}
