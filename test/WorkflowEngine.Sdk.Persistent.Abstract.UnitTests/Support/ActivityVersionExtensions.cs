using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Shouldly;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public static class ActivityVersionExtensions
    {
        public static ActivityVersionRecord MustBe(this ActivityVersionRecord actual, ActivityVersionRecord expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.MustBe((ActivityVersionRecordCreate) expected);
            actual.Id.ShouldBe(expected.Id);
            actual.RecordCreatedAt.ShouldBe(expected.RecordCreatedAt);
            actual.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(expected.RecordUpdatedAt);
            actual.Etag.ShouldBe(expected.Etag);
            return actual;
        }

        public static ActivityVersionRecord MustBe(this ActivityVersionRecord actual, ActivityVersionRecordCreate expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.WorkflowVersionId.ShouldBe(expected.WorkflowVersionId);
            actual.ActivityFormId.ShouldBe(expected.ActivityFormId);
            actual.ParentActivityVersionId.ShouldBe(expected.ParentActivityVersionId);
            actual.FailUrgency.ShouldBe(expected.FailUrgency);
            actual.Position.ShouldBe(expected.Position);
            var now = DateTimeOffset.UtcNow;
            actual.RecordCreatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.RecordUpdatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.Etag.ShouldNotBeNullOrWhiteSpace();
            return actual;
        }

        public static ActivityVersionRecord MustBe(this ActivityVersionRecord actual, Guid id, ActivityVersionRecordCreate expected)
        {
            actual.Id.ShouldBe(id);
            actual.MustBe(expected);
            return actual;
        }
    }
}