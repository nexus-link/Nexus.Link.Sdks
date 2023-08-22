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

public class ActivityFormServiceTests
{
    private readonly IActivityFormService _service;

    public ActivityFormServiceTests()
    {
        _service = new ActivityFormService(new ConfigurationTablesMemory());
    }

    [Fact]
    public async Task CreateAndReadAsync()
    {
        // Arrange
        var parentId = Guid.NewGuid().ToGuidString();
        var childId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityFormCreate
        {
            WorkflowFormId = parentId,
            Type = ActivityTypeEnum.Action,
            Title = Guid.NewGuid().ToGuidString()
        };

        // Act
        await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
        var readItem = await _service.ReadAsync(childId);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.Id.ShouldBeEquivalentTo(childId);
        readItem.WorkflowFormId.ShouldBeEquivalentTo(parentId);
        readItem.Type.ShouldBeEquivalentTo(itemToCreate.Type);
        readItem.Title.ShouldBeEquivalentTo(itemToCreate.Title);
    }

    [Fact]
    public async Task UpdateAsync()
    {
        // Arrange
        var parentId = Guid.NewGuid().ToGuidString();
        var childId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityFormCreate
        {
            WorkflowFormId = parentId,
            Type = ActivityTypeEnum.Action,
            Title = Guid.NewGuid().ToGuidString()
        };
        await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
        var itemToUpdate = await _service.ReadAsync(childId);

        // Act
        itemToUpdate.Title = Guid.NewGuid().ToGuidString();
        await _service.UpdateAndReturnAsync(childId, itemToUpdate);
        var readItem = await _service.ReadAsync(childId);

        // Assert
        readItem.Id.ShouldBeEquivalentTo(childId);
        readItem.Title.ShouldBeEquivalentTo(itemToUpdate.Title);
    }

    [Fact]
    public async Task Update_Given_WrongEtag_Gives_Exception()
    {
        // Arrange
        var parentId = Guid.NewGuid().ToGuidString();
        var childId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityFormCreate
        {
            WorkflowFormId = parentId,
            Type = ActivityTypeEnum.Action,
            Title = Guid.NewGuid().ToGuidString()
        };
        await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
        var itemToUpdate = await _service.ReadAsync(childId);

        // Act & Assert
        itemToUpdate.Title = Guid.NewGuid().ToGuidString();
        itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
        await Should.ThrowAsync<FulcrumConflictException>(() =>
            _service.UpdateAndReturnAsync(childId, itemToUpdate));
    }
}