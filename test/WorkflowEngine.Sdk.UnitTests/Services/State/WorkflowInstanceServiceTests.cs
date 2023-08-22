using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Services.State;

public class WorkflowInstanceServiceTests
{
    private readonly IWorkflowInstanceService _workflowInstanceService;

    public WorkflowInstanceServiceTests()
    {
        _workflowInstanceService = new WorkflowInstanceService(new RuntimeTablesMemory(), null);
    }

    [Fact]
    public async Task CreateAndReadAsync()
    {
        // Arrange
        var workflowVersionId = Guid.NewGuid().ToGuidString();
        var instanceId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new WorkflowInstanceCreate
        {
            WorkflowVersionId = workflowVersionId,
            InitialVersion = Guid.NewGuid().ToGuidString(),
            StartedAt = DateTimeOffset.Now,
            Title = Guid.NewGuid().ToGuidString()
        };

        // Act
        await _workflowInstanceService.CreateWithSpecifiedIdAsync(instanceId, itemToCreate);
        var readItem = await _workflowInstanceService.ReadAsync(instanceId);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.Id.ShouldBeEquivalentTo(instanceId);
        readItem.WorkflowVersionId.ShouldBeEquivalentTo(workflowVersionId);
        readItem.StartedAt.ShouldBeEquivalentTo(itemToCreate.StartedAt);
        readItem.Title.ShouldBeEquivalentTo(itemToCreate.Title);
        readItem.FinishedAt.ShouldBeNull();
        readItem.CancelledAt.ShouldBeNull();
    }
}