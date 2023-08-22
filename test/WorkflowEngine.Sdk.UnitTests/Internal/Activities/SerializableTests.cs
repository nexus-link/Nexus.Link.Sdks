using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Xunit;
#pragma warning disable CS0618

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class SerializableTests : ActivityTestsBase
{
    public SerializableTests() : base(nameof(SerializableTests))
    {
    }

    [Fact]
    public void ActivityAction()
    {
        // Arrange
        var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.CompletedTask);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityActionR()
    {
        // Arrange
        var activity = new ActivityAction<int>(_activityInformationMock, _ => Task.FromResult(1), (_, _) => Task.FromResult(10));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityCondition()
    {
        // Arrange
        var activity = new ActivityCondition<int>(_activityInformationMock, _ => Task.FromResult(1));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityDoWhileOrUntil()
    {
        // Arrange
        var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (_, _) => Task.CompletedTask)
            .While(false);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityDoWhileOrUntilR()
    {
        // Arrange
        var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, _ => Task.FromResult(1), (_, _) => Task.FromResult(10))
            .While(false);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityForEachParallel()
    {
        // Arrange
        var activity = new ActivityForEachParallel<int>(_activityInformationMock, new List<int>(), (_, _, _) => Task.CompletedTask);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityForEachParallelR()
    {
        // Arrange
        var activity = new ActivityForEachParallel<int, int>(_activityInformationMock, new List<int>(), _ => "test", (_, _, _) => Task.FromResult(1));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityForEachSequential()
    {
        // Arrange
        var activity = new ActivityForEachSequential<int>(_activityInformationMock, new List<int>(), _ => "test");

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityForEachSequentialR()
    {
        // Arrange
        var activity = new ActivityForEachSequential<int, int>(_activityInformationMock, new List<int>(), (_, _, _) => Task.FromResult(1), _ => "test");

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityIf()
    {
        // Arrange
        var activity = new ActivityIf(_activityInformationMock, (_, _) => Task.FromResult(false))
            .Then(_ => { })
            .Else(_ => { });

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityIfR()
    {
        // Arrange
        var activity = new ActivityIf<int>(_activityInformationMock, _ => Task.FromResult(1), (_, _) => Task.FromResult(true))
            .Then(_ => 1)
            .Else(_ => 2);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityLock()
    {
        // Arrange
        var activity = new ActivityLock(_activityInformationMock, new SemaphoreSupport("test"));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityLockR()
    {
        // Arrange
        var activity = new ActivityLock<int>(_activityInformationMock, _ => Task.FromResult(1), new SemaphoreSupport("test"));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityLoopUntilTrue()
    {
        // Arrange
        var activity = new ActivityLoopUntilTrue(_activityInformationMock, (_, _) => Task.CompletedTask);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityLoopUntilTrueR()
    {
        // Arrange
        var activity = new ActivityLoopUntilTrue<int>(_activityInformationMock, _ => Task.FromResult(1),
            (_, _) => Task.FromResult(10));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityParallel()
    {
        // Arrange
        var activity = new ActivityParallel(_activityInformationMock)
            .AddJob(1, (_, _) => Task.CompletedTask);

        // Act
        JToken.FromObject(activity);
    }

    [Fact(Skip = "Requires better mock setup, but ActivitySemaphore is obsolete since long anyway")]
    public void ActivitySemaphore()
    {
        // Arrange

        var activity = new ActivitySemaphore(_activityInformationMock, "test")
            .RaiseAsync(TimeSpan.Zero);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivitySleep()
    {
        // Arrange

        var activity = new ActivitySleep(_activityInformationMock, TimeSpan.Zero);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivitySwitch()
    {
        // Arrange
        var activity = new ActivitySwitch<int>(_activityInformationMock, (_, _) => Task.FromResult(1))
            .Case(1, (_, _) => Task.CompletedTask)
            .Default((_, _) => Task.CompletedTask);

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivitySwitchR()
    {
        // Arrange
        var activity = new ActivitySwitch<int, int>(_activityInformationMock, _ => Task.FromResult(1), (_, _) => Task.FromResult(1))
            .Case(1, (_, _) => Task.FromResult(1))
            .Default((_, _) => Task.FromResult(1));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityThrottle()
    {
        // Arrange
        var activity = new ActivityThrottle(_activityInformationMock, new SemaphoreSupport("test", 1, null));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityThrottleR()
    {
        // Arrange
        var activity = new ActivityThrottle<int>(_activityInformationMock, _ => Task.FromResult(1), new SemaphoreSupport("test", 1, null));

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityWhileDo()
    {
        // Arrange
        var activity = new ActivityWhileDo(_activityInformationMock, (_, _) => Task.FromResult(true))
            .Do(_ => { });

        // Act
        JToken.FromObject(activity);
    }

    [Fact]
    public void ActivityWhileDoR()
    {
        // Arrange
        var activity = new ActivityWhileDo<int>(_activityInformationMock, _ => Task.FromResult(1), (_, _) => Task.FromResult(true))
            .Do(_ => 1);

        // Act
        JToken.FromObject(activity);
    }
}