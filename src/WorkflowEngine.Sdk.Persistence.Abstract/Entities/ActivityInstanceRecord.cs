using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities
{
    /// <summary>
    /// The persistence record for an ActivityInstance
    /// </summary>
    public class ActivityInstanceRecord : ActivityInstanceRecordCreate, ICompleteTableItem
    {
        public Guid Id { get; set; }
        public string Etag { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordCreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset RecordUpdatedAt { get; set; }

        /// <inheritdoc />
        public byte[] RecordVersion { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            TableItemHelper.Validate(this, false, errorLocation, propertyPath);
        }
    }
    /// <summary>
    /// The persistence record for an ActivityInstance
    /// </summary>
    public class ActivityInstanceRecordCreate : ActivityInstanceRecordUnique, IValidatable
    {
        public DateTimeOffset StartedAt { get; set; }
        public string State { get; set; }

        public DateTimeOffset? FinishedAt { get; set; }

        public string ResultAsJson { get; set; }

        public string ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }

        public string AsyncRequestId { get; set; }

        public bool? ExceptionAlertHandled { get; set; }

        public string ContextAsJson { get; set; }

        /// <summary>
        /// The nested position of this activity in the activity hierarchy
        /// </summary>
        public string AbsolutePosition { get; set; }

        /// <summary>
        /// The current iteration number for this loop activity, or null if the activity is not a loop activity
        /// </summary>
        public int? Iteration { get; set; }

        /// <summary>
        /// A title associated with an individual iteration (or parallel job).
        /// </summary>
        public string IterationTitle { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotDefaultValue(WorkflowInstanceId, nameof(WorkflowInstanceId), errorLocation);
            FulcrumValidate.IsNotDefaultValue(ActivityVersionId, nameof(ActivityVersionId), errorLocation);
            if (ParentActivityInstanceId.HasValue)
            {
                FulcrumValidate.IsNotDefaultValue(ParentActivityInstanceId.Value, nameof(ParentActivityInstanceId), errorLocation);
            }
            FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, StartedAt, nameof(StartedAt), errorLocation);
            FulcrumValidate.IsNotDefaultValue(StartedAt, nameof(StartedAt), errorLocation);
            if (FinishedAt.HasValue)
            {
                FulcrumValidate.IsNotDefaultValue(FinishedAt.Value, nameof(FinishedAt), errorLocation);
            }
            FulcrumValidate.IsInEnumeration(typeof(ActivityExceptionCategoryEnum), ExceptionCategory, nameof(ExceptionCategory), errorLocation);

            FulcrumValidate.IsJson(ResultAsJson, nameof(ResultAsJson), errorLocation);
            FulcrumValidate.IsJson(ContextAsJson, nameof(ResultAsJson), errorLocation);
            if (ParentIteration.HasValue) FulcrumValidate.IsGreaterThanOrEqualTo(1, ParentIteration.Value, nameof(ParentIteration), errorLocation);
            if (FinishedAt != null)
            {
                FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.Now, FinishedAt.Value, nameof(FinishedAt), errorLocation);
                FulcrumValidate.IsGreaterThanOrEqualTo(StartedAt, FinishedAt.Value, nameof(FinishedAt), errorLocation);
            }
        }
    }

    /// <summary>
    /// The persistence record for an ActivityInstance
    /// </summary>
    public class ActivityInstanceRecordUnique : ActivityInstanceRecordSearch
    {
        /// <summary>
        /// The activity version for this activity instance
        /// </summary>
        public Guid ActivityVersionId { get; set; }

        /// <summary>
        /// The activity instance that is our parent, or null if we have no parent.
        /// </summary>
        public Guid? ParentActivityInstanceId { get; set; }

        /// <summary>
        /// The current iteration count for the parent of this activity, or null if we have no parent or if that parent has no iteration
        /// </summary>
        public int? ParentIteration { get; set; }
    }

    /// <summary>
    /// How we search for all activity instances for a specific workflow instance
    /// </summary>
    public class ActivityInstanceRecordSearch
    {
        /// <summary>
        /// The workflow instance id
        /// </summary>
        public Guid WorkflowInstanceId { get; set; }
    }
}