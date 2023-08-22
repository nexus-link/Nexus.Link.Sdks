
// ReSharper disable ClassNeverInstantiated.Global

using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;

public class ActivitySuccessResult : IValidatable
{
    public string ResultAsJson { get; set; }

    public void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(ResultAsJson, nameof(ResultAsJson), errorLocation);
    }
}