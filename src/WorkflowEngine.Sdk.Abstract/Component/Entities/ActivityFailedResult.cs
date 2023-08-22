
// ReSharper disable ClassNeverInstantiated.Global

using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;

public class ActivityFailedResult : IValidatable
{
    public ActivityExceptionCategoryEnum ExceptionCategory { get; set; }
    public string ExceptionTechnicalMessage { get; set; }
    public string ExceptionFriendlyMessage { get; set; }

    public void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(ExceptionTechnicalMessage, nameof(ExceptionTechnicalMessage), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(ExceptionFriendlyMessage, nameof(ExceptionFriendlyMessage), errorLocation);
    }
}