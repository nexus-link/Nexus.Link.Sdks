using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>
    /// </summary>
    public class ForwardExecutionInformationOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}