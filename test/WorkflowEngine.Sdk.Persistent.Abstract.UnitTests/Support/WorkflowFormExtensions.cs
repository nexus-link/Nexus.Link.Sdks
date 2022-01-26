using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Shouldly;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public static class WorkflowFormExtensions
    {
        public static WorkflowFormRecord MustBe(this WorkflowFormRecord actual, WorkflowFormRecord expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.MustBe((WorkflowFormRecordCreate) expected);
            actual.Id.ShouldBe(expected.Id);
            actual.RecordCreatedAt.ShouldBe(expected.RecordCreatedAt);
            actual.RecordUpdatedAt.ShouldBeGreaterThan(expected.RecordUpdatedAt);
            actual.Etag.ShouldBe(expected.Etag);
            return actual;
        }

        public static WorkflowFormRecord MustBe(this WorkflowFormRecord actual, WorkflowFormRecordCreate expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.CapabilityName.ShouldBe(expected.CapabilityName);
            actual.Title.ShouldBe(expected.Title);
            var now = DateTimeOffset.UtcNow;
            actual.RecordCreatedAt.ShouldBeLessThan(now);
            actual.RecordUpdatedAt.ShouldBeLessThan(now);
            actual.Etag.ShouldNotBeNullOrWhiteSpace();
            return actual;
        }

        public static WorkflowFormRecord MustBe(this WorkflowFormRecord actual, Guid id, WorkflowFormRecordCreate expected)
        {
            actual.Id.ShouldBe(id);
            actual.MustBe(expected);
            return actual;
        }
    }
}