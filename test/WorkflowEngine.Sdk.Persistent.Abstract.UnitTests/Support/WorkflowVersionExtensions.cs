using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Shouldly;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public static class WorkflowVersionExtensions
    {
        public static WorkflowVersionRecord MustBe(this WorkflowVersionRecord actual, WorkflowVersionRecord expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.MustBe((WorkflowVersionRecordCreate) expected);
            actual.Id.ShouldBe(expected.Id);
            actual.RecordCreatedAt.ShouldBe(expected.RecordCreatedAt);
            actual.RecordUpdatedAt.ShouldBeGreaterThan(expected.RecordUpdatedAt);
            actual.Etag.ShouldBe(expected.Etag);
            return actual;
        }

        public static WorkflowVersionRecord MustBe(this WorkflowVersionRecord actual, WorkflowVersionRecordCreate expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.WorkflowFormId.ShouldBe(expected.WorkflowFormId);
            actual.MajorVersion.ShouldBe(expected.MajorVersion);
            actual.MinorVersion.ShouldBe(expected.MinorVersion);
            var now = DateTimeOffset.UtcNow;
            actual.RecordCreatedAt.ShouldBeLessThan(now);
            actual.RecordUpdatedAt.ShouldBeLessThan(now);
            actual.Etag.ShouldNotBeNullOrWhiteSpace();
            return actual;
        }

        public static WorkflowVersionRecord MustBe(this WorkflowVersionRecord actual, Guid id, WorkflowVersionRecordCreate expected)
        {
            actual.Id.ShouldBe(id);
            actual.MustBe(expected);
            return actual;
        }
    }
}