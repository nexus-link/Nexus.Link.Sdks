using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc />
    public class ValueTranslationCapability : IValueTranslationCapability
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ValueTranslationCapability(IHttpSender httpSender)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            AssociationService = new AssociationRestService(httpSender.CreateHttpSender("Associations"));
            TranslatorService = new NotImplementedTranslatorService();
            ConceptService = new NotImplementedConceptService();
        }

        /// <inheritdoc />
        public ITranslatorService TranslatorService { get; }

        /// <inheritdoc />
        public IAssociationService AssociationService { get; }

        /// <inheritdoc />
        public IConceptService ConceptService { get; }

        private class NotImplementedTranslatorService : ITranslatorService
        {
            /// <inheritdoc />
            public Task<IDictionary<string, string>> TranslateAsync(IEnumerable<string> conceptValuePaths, string targetClientName,
                CancellationToken cancellationToken = default)
            {
                throw new FulcrumNotImplementedException("This method is not expected to be called from a Nexus Adapter.");
            }
        }

        private class NotImplementedConceptService : IConceptService
        {
            /// <inheritdoc />
            public Task<IEnumerable<IDictionary<string, string>>> GetAllInstancesAsync(string conceptName, CancellationToken cancellationToken = default)
            {
                throw new FulcrumNotImplementedException("This method is not expected to be called from a Nexus Adapter.");
            }
        }
    }

}
