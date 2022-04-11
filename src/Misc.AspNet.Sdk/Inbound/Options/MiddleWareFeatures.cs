#if NETCOREAPP
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// All the features for <see cref="NexusLinkMiddleware"/>.
    /// </summary>
    public class MiddlewareFeatures : IValidatable
    {
        /// <summary>
        /// If one log message in a batch has a severity level equal to or higher than <see name="LogAllThreshold"/>,
        /// then all the logs within that batch will be logged, regardless of the value of
        /// <see cref="ApplicationSetup.LogSeverityLevelThreshold"/>.
        /// </summary>
        public BatchLogOptions BatchLog { get; } = new();

        /// <summary>
        /// The prefix before the "/{organization}/{environment}/" part of the path. This is used to pattern match where we would find the organization and environment.
        /// Here are some common patterns: <see cref="SaveClientTenantOptions.LegacyVersion"/>, <see cref="SaveClientTenantOptions.LegacyApiVersion"/>,
        /// <see cref="SaveClientTenantOptions.ApiVersionTenant"/>
        /// </summary>
        public SaveClientTenantOptions SaveClientTenant { get; } = new();

        /// <summary>
        /// Log request and response
        /// </summary>
        public LogRequestAndResponseOptions LogRequestAndResponse { get; } = new();

        /// <summary>
        /// If an API method throws an exception, then this feature will convert it into a regular HTTP response.
        /// </summary>
        public ConvertExceptionToHttpResponseOptions ConvertExceptionToHttpResponse { get; } = new();

        /// <summary>
        /// This feature retrieves the tenant configuration and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        public SaveTenantConfigurationOptions SaveTenantConfiguration { get; } = new();

        /// <summary>
        /// This feature gets the first found <see cref="Constants.FulcrumCorrelationIdHeaderName"/> header from the request and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        public SaveCorrelationIdOptions SaveCorrelationId { get; } = new();

        /// <summary>
        /// This feature gets the first found <see cref="Constants.ExecutionIdHeaderName"/> header from the request and saves it to the <see cref="FulcrumApplication.Context"/>.
        /// </summary>
        public SaveExecutionIdOptions SaveExecutionId { get; } = new();

        /// <summary>
        /// This feature reads the <see cref="Constants.NexusTestContextHeaderName"/> header from the request and saves it to the execution context.
        /// </summary>
        public SaveNexusTestContextOptions SaveNexusTestContext { get; } = new();

        /// <summary>
        /// This feature reads the <see cref="Constants.ReentryAuthenticationHeaderName"/> header from the request and saves it to the execution context.
        /// </summary>
        public SaveReentryAuthenticationOptions SaveReentryAuthentication { get; } = new();

        /// <summary>
        /// This feature redirects to Asynchronous request-response management if a requests results in a <see cref="RequestPostponedException"/>.
        /// </summary>
        public RedirectAsynchronousRequestsOptions RedirectAsynchronousRequests { get; } = new();

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(BatchLog, propertyPath, nameof(BatchLog), errorLocation);
            FulcrumValidate.IsValidated(LogRequestAndResponse, propertyPath, nameof(LogRequestAndResponse), errorLocation);
            FulcrumValidate.IsValidated(SaveClientTenant, propertyPath, nameof(SaveClientTenant), errorLocation);
            FulcrumValidate.IsValidated(ConvertExceptionToHttpResponse, propertyPath, nameof(ConvertExceptionToHttpResponse), errorLocation);
            FulcrumValidate.IsValidated(SaveTenantConfiguration, propertyPath, nameof(SaveTenantConfiguration), errorLocation);
            FulcrumValidate.IsValidated(SaveCorrelationId, propertyPath, nameof(SaveCorrelationId), errorLocation);
            FulcrumValidate.IsValidated(SaveExecutionId, propertyPath, nameof(SaveCorrelationId), errorLocation);
            FulcrumValidate.IsValidated(SaveNexusTestContext, propertyPath, nameof(SaveNexusTestContext), errorLocation);
            FulcrumValidate.IsValidated(SaveReentryAuthentication, propertyPath, nameof(SaveReentryAuthentication), errorLocation);
            FulcrumValidate.IsValidated(RedirectAsynchronousRequests, propertyPath, nameof(RedirectAsynchronousRequests), errorLocation);
        }
    }
}
#endif