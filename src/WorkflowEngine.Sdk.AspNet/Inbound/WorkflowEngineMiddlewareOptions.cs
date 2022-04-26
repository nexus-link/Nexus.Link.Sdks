using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound.Options;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Inbound
{
    [Obsolete("Please use Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddlewareOptions. Obsolete since 2022-04-07.")]
    public class WorkflowEngineMiddlewareOptions : IValidatable
    {
        public WorkflowEngineMiddlewareFeatures Features = new();


        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}
