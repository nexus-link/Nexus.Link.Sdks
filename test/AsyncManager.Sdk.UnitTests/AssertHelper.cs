using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Xunit;

namespace AsyncManager.Sdk.UnitTests
{
    internal static class AssertHelper
    {
        public static void AssertEqual(HttpRequestCreate expected, HttpRequestCreate actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null) Assert.Null(actual);
            else Assert.NotNull(actual);

            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Url, actual.Url);
            Assert.Equal(expected.Content, actual.Content);
            AssertEqual(expected.Headers, actual.Headers);
            AssertEqual(expected.Metadata, actual.Metadata);
            AssertEqual(expected.Metadata.Callback, actual.Metadata.Callback);
        }

        public static void AssertEqual(RequestMetadata expected, RequestMetadata actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null) Assert.Null(actual);
            else Assert.NotNull(actual);

            Assert.Equal(expected.Priority, actual.Priority);

            var precision = TimeSpan.FromSeconds(1);
            AssertEqual(expected.ExecuteAfter, actual.ExecuteAfter, precision);
            AssertEqual(expected.ExecuteBefore, actual.ExecuteBefore, precision);
        }

        public static void AssertEqual(DateTimeOffset? expected, DateTimeOffset? actual, TimeSpan precision)
        {
            if (expected == null && actual == null) return;
            if (expected == null) Assert.Null(actual);
            else Assert.NotNull(actual);

            Assert.Equal((DateTimeOffset)expected, (DateTimeOffset)actual, new DateTimeOffsetPrecisionEqualityComparer(precision));
        }

        public static void AssertEqual(RequestCallback expected, RequestCallback actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null) Assert.Null(actual);
            else Assert.NotNull(actual);
            
            Assert.Equal(expected.Url, actual.Url);
            Assert.Equal(expected.Context, actual.Context);
            AssertEqual(expected.Headers, actual.Headers);
        }

        public static void AssertEqual(Dictionary<string, StringValues> expected,
            Dictionary<string, StringValues> actual)
        {
            if (expected == null && actual == null) return;
            if (expected == null) Assert.Null(actual);
            else Assert.NotNull(actual);
            Assert.True(expected.Count == actual.Count, "The Headers attributes had different lengths.");
            Assert.True(expected.All(h1 => actual.Any(h2 =>
                    h1.Key == h2.Key && h1.Value.OrderByDescending(h => h).SequenceEqual(h2.Value.OrderByDescending(h => h)))),
                "The Headers attributes were not equal");
        }
    }
}
