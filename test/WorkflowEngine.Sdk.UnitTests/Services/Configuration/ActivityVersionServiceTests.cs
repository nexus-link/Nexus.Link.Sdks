using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Services.Configuration;

public class ActivityVersionServiceTests
{
    private readonly IActivityVersionService _service;

    public ActivityVersionServiceTests()
    {
        _service = new ActivityVersionService(new ConfigurationTablesMemory());
    }

    [Fact]
    public async Task CreateAndReadAsync()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var parentId = Guid.NewGuid().ToGuidString();
        var activityFormId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityVersionCreate
        {
            WorkflowVersionId = parentId,
            Position = 1,
            ActivityFormId = activityFormId,
            ParentActivityVersionId = Guid.NewGuid().ToGuidString()
        };

        // Act
        var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var childId = activityVersion.Id;
        var readItem = await _service.ReadAsync(id);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.Id.ShouldBe(childId);
        readItem.WorkflowVersionId.ShouldBe(parentId);
        readItem.Position.ShouldBe(itemToCreate.Position);
        readItem.ActivityFormId.ShouldBe(itemToCreate.ActivityFormId);
        readItem.ParentActivityVersionId.ShouldBe(itemToCreate.ParentActivityVersionId);
    }

    [Fact]
    public async Task UpdateAsync()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var parentId = Guid.NewGuid().ToGuidString();
        var activityFormId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityVersionCreate
        {
            WorkflowVersionId = parentId,
            Position = 1,
            ActivityFormId = activityFormId,
            ParentActivityVersionId = Guid.NewGuid().ToGuidString(),
            FailUrgency = ActivityFailUrgencyEnum.Stopping
        };
        var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var childId = activityVersion.Id;
        var itemToUpdate = await _service.ReadAsync(id);

        // Act
        itemToUpdate.Position = 2;
        itemToUpdate.FailUrgency = ActivityFailUrgencyEnum.Ignore;
        await _service.UpdateAndReturnAsync(childId, itemToUpdate);
        var readItem = await _service.ReadAsync(id);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.Id.ShouldBe(childId);
        readItem.Position.ShouldBe(itemToUpdate.Position);
        readItem.FailUrgency.ShouldBe(itemToUpdate.FailUrgency);
    }

    [Fact]
    public async Task Update_Given_WrongEtag_Gives_Exception()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var parentId = Guid.NewGuid().ToGuidString();
        var activityFormId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityVersionCreate
        {
            WorkflowVersionId = parentId,
            Position = 1,
            ActivityFormId = activityFormId,
            FailUrgency = ActivityFailUrgencyEnum.Stopping,
            ParentActivityVersionId = Guid.NewGuid().ToGuidString()
        };
        var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var childId = activityVersion.Id;
        var itemToUpdate = await _service.ReadAsync(id);

        // Act & Assert
        itemToUpdate.Position = 2;
        itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
        await Should.ThrowAsync<FulcrumConflictException>(() =>
            _service.UpdateAndReturnAsync(childId, itemToUpdate));
    }
}