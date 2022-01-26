using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Shouldly;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public static class WorkflowInstanceExtensions
    {
        public static WorkflowInstanceRecord MustBe(this WorkflowInstanceRecord actual, WorkflowInstanceRecord expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.MustBe((WorkflowInstanceRecordCreate) expected);
            actual.Id.ShouldBe(expected.Id);
            actual.RecordCreatedAt.ShouldBe(expected.RecordCreatedAt);
            actual.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(expected.RecordUpdatedAt);
            actual.Etag.ShouldBe(expected.Etag);
            return actual;
        }

        public static WorkflowInstanceRecord MustBe(this WorkflowInstanceRecord actual, WorkflowInstanceRecordCreate expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.WorkflowVersionId.ShouldBe(expected.WorkflowVersionId);
            actual.Title.ShouldBe(expected.Title);
            actual.CancelledAt.ShouldBe(expected.CancelledAt);
            actual.FinishedAt.ShouldBe(expected.FinishedAt);
            actual.StartedAt.ShouldBe(expected.StartedAt);
            actual.InitialVersion.ShouldBe(expected.InitialVersion);
            actual.ResultAsJson.ShouldBe(expected.ResultAsJson);
            actual.ExceptionFriendlyMessage.ShouldBe(expected.ExceptionFriendlyMessage);
            actual.ExceptionTechnicalMessage.ShouldBe(expected.ExceptionTechnicalMessage);
            actual.State.ShouldBe(expected.State);;
            actual.IsComplete.ShouldBe(expected.IsComplete);
            var now = DateTimeOffset.UtcNow;
            actual.RecordCreatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.RecordUpdatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.Etag.ShouldNotBeNullOrWhiteSpace();
            return actual;
        }

        public static WorkflowInstanceRecord MustBe(this WorkflowInstanceRecord actual, Guid id, WorkflowInstanceRecordCreate expected)
        {
            actual.Id.ShouldBe(id);
            actual.MustBe(expected);
            return actual;
        }
    }
}