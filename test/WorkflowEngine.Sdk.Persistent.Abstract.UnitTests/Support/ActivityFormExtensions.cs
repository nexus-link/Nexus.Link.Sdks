using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Shouldly;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public static class ActivityFormExtensions
    {
        public static ActivityFormRecord MustBe(this ActivityFormRecord actual, ActivityFormRecord expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.MustBe((ActivityFormRecordCreate) expected);
            actual.Id.ShouldBe(expected.Id);
            actual.RecordCreatedAt.ShouldBe(expected.RecordCreatedAt);
            actual.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(expected.RecordUpdatedAt);
            actual.Etag.ShouldBe(expected.Etag);
            return actual;
        }

        public static ActivityFormRecord MustBe(this ActivityFormRecord actual, ActivityFormRecordCreate expected)
        {
            if (expected == null)
            {
                actual.ShouldBeNull();
                return actual;
            }

            actual.ShouldNotBeNull();
            actual.WorkflowFormId.ShouldBe(expected.WorkflowFormId);
            actual.Title.ShouldBe(expected.Title);
            actual.Type.ShouldBe(expected.Type);
            var now = DateTimeOffset.UtcNow;
            actual.RecordCreatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.RecordUpdatedAt.ShouldBeLessThanOrEqualTo(now);
            actual.Etag.ShouldNotBeNullOrWhiteSpace();
            return actual;
        }

        public static ActivityFormRecord MustBe(this ActivityFormRecord actual, Guid id, ActivityFormRecordCreate expected)
        {
            actual.Id.ShouldBe(id);
            actual.MustBe(expected);
            return actual;
        }
    }
}