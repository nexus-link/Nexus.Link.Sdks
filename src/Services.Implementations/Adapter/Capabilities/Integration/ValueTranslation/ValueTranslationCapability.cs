using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc />
    public class ValueTranslationCapability : IValueTranslationCapability
    {
        /// <inheritdoc />
        public ValueTranslationCapability(IHttpSender httpSender)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            AssociationService = new AssociationsRestService(httpSender.CreateHttpSender("Associations"));
            TranslatorService = new NotImplementedTranslatorService();
        }

        /// <inheritdoc />
        public ITranslatorService TranslatorService { get; }

        /// <inheritdoc />
        public IAssociationsService AssociationService { get; }

        private class NotImplementedTranslatorService : ITranslatorService
        {
            /// <inheritdoc />
            public Task<IDictionary<string, string>> TranslateAsync(IEnumerable<string> conceptValuePaths, string targetClientName,
                CancellationToken cancellationToken = new CancellationToken())
            {
                throw new System.NotImplementedException();
            }
        }
    }

}
