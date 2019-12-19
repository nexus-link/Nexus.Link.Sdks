using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc cref="IBusinessEventService" />
    public class AssociationsRestService : RestClientBase, IAssociationsService
    {
        /// <inheritdoc cref="IBusinessEventService" />
        public AssociationsRestService(IHttpSender httpSender)
        :base(httpSender)
        {
        }

        /// <inheritdoc />
        public Task AssociateAsync(string sourceConceptValuePath, string [] targetConceptValuePaths,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNull(targetConceptValuePaths, nameof(targetConceptValuePaths));
            foreach (var targetConceptValuePath in targetConceptValuePaths)
            {
                InternalContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePaths), $"The individual values of {nameof(targetConceptValuePaths)} must not be null or empty.");
            }
            return RestClient.PostNoResponseContentAsync(
                $"{WebUtility.UrlEncode(sourceConceptValuePath)}", targetConceptValuePaths, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceConceptValuePath, string targetContextName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetContextName, nameof(targetContextName));
            return RestClient.GetAsync<ValueOrLockId>(
                $"{WebUtility.UrlEncode(sourceConceptValuePath)}/Context/{WebUtility.UrlEncode(targetContextName)}/ValueOrLock", null,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceConceptValuePath, string targetClientName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetClientName, nameof(targetClientName));
            return RestClient.GetAsync<ValueOrLockId>(
                $"{WebUtility.UrlEncode(sourceConceptValuePath)}/Client/{WebUtility.UrlEncode(targetClientName)}/ValueOrLock", null,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task AssociateUsingLockAsync(string sourceConceptValuePath, string lockId, string targetConceptValuePath, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            return RestClient.PostNoResponseContentAsync(
                $"{WebUtility.UrlEncode(sourceConceptValuePath)}/Locks/{WebUtility.UrlEncode(lockId)}", targetConceptValuePath, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(string conceptValuePath, string lockId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(conceptValuePath, nameof(conceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            return RestClient.DeleteAsync(
                $"Associations/{WebUtility.UrlEncode(conceptValuePath)}/Locks/{WebUtility.UrlEncode(lockId)}", null, cancellationToken);
        }
    }
}