using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Feature for converting exceptions into HTTP responses
    /// </summary>
    public class ConvertExceptionToHttpResponseOptions : Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}