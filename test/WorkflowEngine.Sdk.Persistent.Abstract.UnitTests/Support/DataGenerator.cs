using System;
using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    internal static class DataGenerator
    {
        #region WorkflowForm
        public static Guid WorkflowFormId { get; set; }

        public static WorkflowFormRecordCreate DefaultWorkflowFormCreate => new WorkflowFormRecordCreate
        {
            CapabilityName = Guid.NewGuid().ToString(),
            Title = Guid.NewGuid().ToString()
        };

        public static IEnumerable<object[]> BadWorkflowFormCreate()
        {
            var item = DefaultWorkflowFormCreate;
            item.CapabilityName = null;
            yield return new object[] { item, "Capability column does not allow null" };
            item = DefaultWorkflowFormCreate;
            item.CapabilityName = "";
            yield return new object[] { item, "Capability column does not allow empty" };
            item = DefaultWorkflowFormCreate;
            item.CapabilityName = " ";
            yield return new object[] { item, "Capability column does not allow whitespace" };
            item = DefaultWorkflowFormCreate;
            item.Title = null;
            yield return new object[] { item, "Title column does not allow nulls" };
            item = DefaultWorkflowFormCreate;
            item.Title = "";
            yield return new object[] { item, "Title column does not allow empty" };
            item = DefaultWorkflowFormCreate;
            item.Title = " ";
            yield return new object[] { item, "Title column does not allow whitespace" };
        }
        #endregion

        #region WorkflowVersion
        public static Guid WorkflowVersionId { get; set; }

        public static WorkflowVersionRecordCreate DefaultWorkflowVersionCreate => new WorkflowVersionRecordCreate
        {
            WorkflowFormId = WorkflowFormId,
            MajorVersion = 3,
            MinorVersion = 2
        };

        public static IEnumerable<object[]> BadWorkflowVersionCreate()
        {
            var item = DefaultWorkflowVersionCreate;
            item.WorkflowFormId = Guid.Empty;
            yield return new object[] { item, "WorkflowFormId can't be Guid.Empty" };
            item = DefaultWorkflowVersionCreate;
            item.MajorVersion = -1;
            yield return new object[] { item, "MajorVersion can't be a negative number" };
            item = DefaultWorkflowVersionCreate;
            item.MinorVersion = -1;
            yield return new object[] { item, "MinorVersion can't be a negative number" };
        }
        #endregion

        #region WorkflowInstance
        public static Guid WorkflowInstanceId { get; set; }

        public static WorkflowInstanceRecordCreate DefaultWorkflowInstanceCreate => new WorkflowInstanceRecordCreate
        {
            WorkflowVersionId = WorkflowVersionId,
            Title = Guid.NewGuid().ToString(),
            InitialVersion = "1.1",
            StartedAt = DateTimeOffset.UtcNow,
            State = WorkflowStateEnum.Waiting.ToString(),
            FinishedAt = null,
            CancelledAt = null,
            IsComplete = false,
            ExceptionFriendlyMessage = null,
            ExceptionTechnicalMessage = null,
            ResultAsJson = null
        };

        public static IEnumerable<object[]> BadWorkflowInstanceCreate()
        {
            var item = DefaultWorkflowInstanceCreate;
            item.WorkflowVersionId = Guid.Empty;
            yield return new object[] { item, $"{nameof(item.WorkflowVersionId)} can't be Guid.Empty" };
            item = DefaultWorkflowInstanceCreate;
            item.Title = null;
            yield return new object[] { item, $"{nameof(item.Title)} can't be null" };
            item = DefaultWorkflowInstanceCreate;
            item.Title = "";
            yield return new object[] { item, $"{nameof(item.Title)} can't be empty" };
            item = DefaultWorkflowInstanceCreate;
            item.Title = " ";
            yield return new object[] { item, $"{nameof(item.Title)} can't be whitespace" };
            item = DefaultWorkflowInstanceCreate;
            item.InitialVersion = null;
            yield return new object[] { item, $"{nameof(item.InitialVersion)} can't be null" };
            item = DefaultWorkflowInstanceCreate;
            item.InitialVersion = "";
            yield return new object[] { item, $"{nameof(item.InitialVersion)} can't be empty" };
            item = DefaultWorkflowInstanceCreate;
            item.InitialVersion = " ";
            yield return new object[] { item, $"{nameof(item.InitialVersion)} can't be whitespace" };
            item = DefaultWorkflowInstanceCreate;
            item.StartedAt = default(DateTimeOffset);
            yield return new object[] { item, $"{nameof(item.StartedAt)} can't be default" };
            item = DefaultWorkflowInstanceCreate;
            item.StartedAt = DateTimeOffset.UtcNow.AddMinutes(1);
            yield return new object[] { item, $"{nameof(item.StartedAt)} can't be in the future" };
            item = DefaultWorkflowInstanceCreate;
            item.FinishedAt = default(DateTimeOffset);
            yield return new object[] { item, $"{nameof(item.FinishedAt)} can't be default" };
            item = DefaultWorkflowInstanceCreate;
            item.FinishedAt = DateTimeOffset.UtcNow.AddMinutes(1);
            yield return new object[] { item, $"{nameof(item.FinishedAt)} can't be in the future" };
            item = DefaultWorkflowInstanceCreate;
            item.CancelledAt = default(DateTimeOffset);
            yield return new object[] { item, $"{nameof(item.CancelledAt)} can't be default" };
            item = DefaultWorkflowInstanceCreate;
            item.CancelledAt = DateTimeOffset.UtcNow.AddMinutes(1);
            yield return new object[] { item, $"{nameof(item.CancelledAt)} can't be in the future" };
            item = DefaultWorkflowInstanceCreate;
            item.ResultAsJson = "This is not json";
            yield return new object[] { item, $"{nameof(item.ResultAsJson)} must be JSON" };
        }
        #endregion

        #region ActivityForm
        public static Guid ActivityFormId { get; set; }

        public static ActivityFormRecordCreate DefaultActivityFormCreate => new ActivityFormRecordCreate
        {
            WorkflowFormId = WorkflowFormId,
            Title = Guid.NewGuid().ToString(),
            Type = ActivityTypeEnum.Action.ToString()
        };

        public static IEnumerable<object[]> BadActivityFormCreate()
        {
            var item = DefaultActivityFormCreate;
            item = DefaultActivityFormCreate;
            item.Title = null;
            yield return new object[] { item, "Title column does not allow nulls" };
            item = DefaultActivityFormCreate;
            item.Title = "";
            yield return new object[] { item, "Title column does not allow empty" };
            item = DefaultActivityFormCreate;
            item.Title = " ";
            yield return new object[] { item, "Title column does not allow whitespace" };
            item = DefaultActivityFormCreate;
            item.Type = null;
            yield return new object[] { item, "Type column does not allow nulls" };
            item = DefaultActivityFormCreate;
            item.Type = "_";
            yield return new object[] { item, $"Type column must have an enum value from enum {nameof(ActivityTypeEnum)}" };
        }
        #endregion

        #region ActivityVersion
        public static Guid ActivityVersionId { get; set; }

        public static ActivityVersionRecordCreate DefaultActivityVersionCreate => new ActivityVersionRecordCreate
        {
            WorkflowVersionId = WorkflowVersionId,
            ActivityFormId = ActivityFormId,
            FailUrgency = ActivityFailUrgencyEnum.Ignore.ToString(),
            ParentActivityVersionId = null,
            Position = 1
        };

        public static IEnumerable<object[]> BadActivityVersionCreate()
        {
            var item = DefaultActivityVersionCreate;
            item.WorkflowVersionId = Guid.Empty;
            yield return new object[] { item, "WorkflowVersionId can't be Guid.Empty" };
            item = DefaultActivityVersionCreate;
            item.ActivityFormId = Guid.Empty;
            yield return new object[] { item, "ActivityFormId can't be Guid.Empty" };
            item = DefaultActivityVersionCreate;
            item.ParentActivityVersionId = Guid.Empty;
            yield return new object[] { item, "ParentActivityVersionId can't be Guid.Empty" };
            item = DefaultActivityVersionCreate;
            item.FailUrgency = null;
            yield return new object[] { item, "Type column does not allow nulls" };
            item = DefaultActivityVersionCreate;
            item.FailUrgency = "_";
            yield return new object[] { item, $"Type column must have an enum value from enum {nameof(ActivityTypeEnum)}" };
            item = DefaultActivityVersionCreate;
            item.Position = -1;
            yield return new object[] { item, "Position can't be a negative number" };
        }
        #endregion

        #region ActivityInstance
        public static Guid ActivityInstanceId { get; set; }

        public static ActivityInstanceRecordCreate DefaultActivityInstanceCreate => new ActivityInstanceRecordCreate
        {
            WorkflowInstanceId = WorkflowInstanceId,
            ActivityVersionId = ActivityVersionId,
            ParentActivityInstanceId = null,
            AsyncRequestId = null,
            ContextAsJson = null,
            ExceptionAlertHandled = null,
            ExceptionCategory = null,
            ExceptionFriendlyMessage = null,
            ExceptionTechnicalMessage = null,
            StartedAt = DateTimeOffset.UtcNow,
            FinishedAt = null,
            ParentIteration = null,
            ResultAsJson = null,
            State = ActivityStateEnum.Executing.ToString()
        };

        public static IEnumerable<object[]> BadActivityInstanceCreate()
        {
            var item = DefaultActivityInstanceCreate;
            item.WorkflowInstanceId = Guid.Empty;
            yield return new object[] { item, $"{nameof(item.WorkflowInstanceId)} can't be Guid.Empty" };
            item = DefaultActivityInstanceCreate;
            item.ActivityVersionId = Guid.Empty;
            yield return new object[] { item, $"{nameof(item.ActivityVersionId)} can't be Guid.Empty" };
            item = DefaultActivityInstanceCreate;
            item.ParentActivityInstanceId = Guid.Empty;
            yield return new object[] { item, $"{nameof(item.ParentActivityInstanceId)} can't be Guid.Empty" };
            item = DefaultActivityInstanceCreate;
            item.ContextAsJson = "This is not json";
            yield return new object[] { item, $"{nameof(item.ContextAsJson)} must be JSON" };
            item = DefaultActivityInstanceCreate;
            item.ExceptionCategory = "_";
            yield return new object[] { item, $"{nameof(item.ExceptionCategory)} must be enum {nameof(ActivityExceptionCategoryEnum)}" };
            item = DefaultActivityInstanceCreate;
            item.StartedAt = default(DateTimeOffset);
            yield return new object[] { item, $"{nameof(item.StartedAt)} can't be default" };
            item = DefaultActivityInstanceCreate;
            item.StartedAt = DateTimeOffset.UtcNow.AddMinutes(1);
            yield return new object[] { item, $"{nameof(item.StartedAt)} can't be in the future" };
            item = DefaultActivityInstanceCreate;
            item.FinishedAt = default(DateTimeOffset);
            yield return new object[] { item, $"{nameof(item.FinishedAt)} can't be default" };
            item = DefaultActivityInstanceCreate;
            item.FinishedAt = DateTimeOffset.UtcNow.AddMinutes(1);
            yield return new object[] { item, $"{nameof(item.FinishedAt)} can't be in the future" };
            item.ParentIteration = -1;
            yield return new object[] { item, $"{nameof(item.ParentIteration)} can't be a negative number" };
            item = DefaultActivityInstanceCreate;
            item.ResultAsJson = "This is not json";
            yield return new object[] { item, $"{nameof(item.ResultAsJson)} must be JSON" };
        }
        #endregion
    }
}