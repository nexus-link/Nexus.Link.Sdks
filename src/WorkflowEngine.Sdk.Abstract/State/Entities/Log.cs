using System;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

public class Log : LogCreate, IUniquelyIdentifiable<string>, IOptimisticConcurrencyControlByETag
{
    public string Id { get; set; }
    public string Etag { get; set; }

    /// <inheritdoc />
    public override void Validate(string errorLocation, string propertyPath = "")
    {
        base.Validate(errorLocation, propertyPath);
        FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), errorLocation);
        FulcrumValidate.IsNotNullOrWhiteSpace(Etag, nameof(Etag), errorLocation);
    }
}

public class LogCreate : IValidatable
{
    public string WorkflowFormId { get; set; }
    public string WorkflowInstanceId { get; set; }
    public string ActivityFormId { get; set; }

    public LogSeverityLevel SeverityLevel { get; set; }

    public string Message { get; set; }

    public JToken Data{ get; set; }

    public DateTimeOffset TimeStamp { get; set; }

    /// <inheritdoc />
    public virtual void Validate(string errorLocation, string propertyPath = "")
    {
        FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowFormId, nameof(WorkflowFormId), errorLocation);
        if (ActivityFormId != null)
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(ActivityFormId, nameof(ActivityFormId), errorLocation);
        }
        if (WorkflowInstanceId != null)
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
        }
        FulcrumValidate.IsNotNullOrWhiteSpace(Message, nameof(Message), errorLocation);
    }
}