using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Shouldly;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public static class ActivityInstanceExtensions
    {
        public static ActivityInstanceRecord MustBe(this ActivityInstanceRecord actual, ActivityInstanceRecord expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.MustBe((ActivityInstanceRecordCreate) expected);
            actual.Id.ShouldBe(expected.Id);
            actual.RecordCreatedAt.ShouldBe(expected.RecordCreatedAt);
            actual.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(expected.RecordUpdatedAt);
            actual.Etag.ShouldBe(expected.Etag);
            return actual;
        }

        public static ActivityInstanceRecord MustBe(this ActivityInstanceRecord actual, ActivityInstanceRecordCreate expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.WorkflowInstanceId.ShouldBe(expected.WorkflowInstanceId);
            actual.ActivityVersionId.ShouldBe(expected.ActivityVersionId);
            actual.ParentActivityInstanceId.ShouldBe(expected.ParentActivityInstanceId);
            actual.AsyncRequestId.ShouldBe(expected.AsyncRequestId);
            actual.ContextAsJson.ShouldBe(expected.ContextAsJson);
            actual.FinishedAt.ShouldBe(expected.FinishedAt);
            actual.StartedAt.ShouldBe(expected.StartedAt);
            actual.ExceptionCategory.ShouldBe(expected.ExceptionCategory);
            actual.ResultAsJson.ShouldBe(expected.ResultAsJson);
            actual.ExceptionFriendlyMessage.ShouldBe(expected.ExceptionFriendlyMessage);
            actual.ExceptionTechnicalMessage.ShouldBe(expected.ExceptionTechnicalMessage);
            actual.State.ShouldBe(expected.State);
            actual.ExceptionAlertHandled.ShouldBe(expected.ExceptionAlertHandled);
            actual.ParentIteration.ShouldBe(expected.ParentIteration);
            actual.Iteration.ShouldBe(expected.Iteration);
            actual.IterationTitle.ShouldBe(expected.IterationTitle);
            actual.ParentIteration.ShouldBe(expected.ParentIteration);
            var now = DateTimeOffset.UtcNow;
            actual.RecordCreatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.RecordUpdatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.Etag.ShouldNotBeNullOrWhiteSpace();
            return actual;
        }

        public static ActivityInstanceRecord MustBe(this ActivityInstanceRecord actual, Guid id, ActivityInstanceRecordCreate expected)
        {
            actual.Id.ShouldBe(id);
            actual.MustBe(expected);
            return actual;
        }
    }
}