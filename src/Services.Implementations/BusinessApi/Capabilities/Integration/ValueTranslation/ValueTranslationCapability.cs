using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc />
    public class ValueTranslationCapability : IValueTranslationCapability
    {
        /// <inheritdoc />
        public ValueTranslationCapability(string serviceBaseUrl, ServiceClientCredentials serviceClientCredentials)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serviceBaseUrl, nameof(serviceBaseUrl));
            InternalContract.RequireNotNull(serviceClientCredentials, nameof(serviceClientCredentials));
            var translatorClient = new TranslateClient(serviceBaseUrl, FulcrumApplication.Setup.Tenant, serviceClientCredentials);
            TranslatorService = new TranslatorService(translatorClient);
            var associationsClient = new AssociationsClient(serviceBaseUrl, FulcrumApplication.Setup.Tenant,
                serviceClientCredentials);
            AssociationService = new AssociationsService(translatorClient, associationsClient);
        }

        /// <inheritdoc />
        public ITranslatorService TranslatorService { get; }

        /// <inheritdoc />
        public IAssociationsService AssociationService { get; }
    }
}
