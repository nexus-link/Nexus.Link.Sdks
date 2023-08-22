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

public class WorkflowVersionServiceTests
{
    private readonly IWorkflowVersionService _workflowVersionService;

    public WorkflowVersionServiceTests()
    {
        _workflowVersionService = new WorkflowVersionService(new ConfigurationTablesMemory());
    }

    [Fact]
    public async Task CreateAndReadAsync()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var masterId = Guid.NewGuid().ToGuidString();
        var majorVersion = 1;
        var itemToCreate = new WorkflowVersionCreate
        {
            WorkflowFormId = masterId,
            MajorVersion = majorVersion,
            MinorVersion = 3,
            DynamicCreate = true
        };

        // Act
        var createdItem = await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var readItem = await _workflowVersionService.ReadAsync(id);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.WorkflowFormId.ShouldBeEquivalentTo(masterId);
        readItem.MajorVersion.ShouldBeEquivalentTo(majorVersion);
        readItem.MinorVersion.ShouldBeEquivalentTo(itemToCreate.MinorVersion);
        readItem.DynamicCreate.ShouldBeEquivalentTo(itemToCreate.DynamicCreate);
    }

    [Fact]
    public async Task Create_Given_SameVersion_Gives_Exception()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var masterId = Guid.NewGuid().ToGuidString();
        var majorVersion = 1;
        var itemToCreate = new WorkflowVersionCreate
        {
            WorkflowFormId = masterId,
            MajorVersion = majorVersion,
            MinorVersion = 3
        };
        var createdItem = await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);

        // Act && Assert
        await Should.ThrowAsync<FulcrumConflictException>(() =>
            _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid().ToGuidString(), itemToCreate));
    }

    [Fact]
    public async Task UpdateAsync()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var masterId = Guid.NewGuid().ToGuidString();
        var majorVersion = 1;
        var itemToCreate = new WorkflowVersionCreate
        {
            WorkflowFormId = masterId,
            MajorVersion = majorVersion,
            MinorVersion = 3,
            DynamicCreate = false
        };
        await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var itemToUpdate = await _workflowVersionService.ReadAsync(id);

        // Act
        itemToUpdate.MinorVersion =itemToCreate.MinorVersion+1;
        itemToUpdate.DynamicCreate = true;
        await _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate);
        var readItem = await _workflowVersionService.ReadAsync(id);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.WorkflowFormId.ShouldBeEquivalentTo(masterId);
        readItem.MajorVersion.ShouldBeEquivalentTo(majorVersion);
        readItem.MinorVersion.ShouldBeEquivalentTo(itemToUpdate.MinorVersion);
        readItem.DynamicCreate.ShouldBeEquivalentTo(itemToUpdate.DynamicCreate);
    }

    [Fact]
    public async Task Update_Given_WrongEtag_Gives_Exception()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var masterId = Guid.NewGuid().ToGuidString();
        var majorVersion = 1;
        var itemToCreate = new WorkflowVersionCreate
        {
            WorkflowFormId = masterId,
            MajorVersion = majorVersion,
            MinorVersion = 3
        };
        await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var itemToUpdate = await _workflowVersionService.ReadAsync(id);

        // Act & Assert
        itemToUpdate.MinorVersion =itemToCreate.MinorVersion+1;
        itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
        await Should.ThrowAsync<FulcrumConflictException>(() =>
            _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate));
    }

    [Fact]
    public async Task Update_Given_NewMajorVersion_Gives_Exception()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var masterId = Guid.NewGuid().ToGuidString();
        var majorVersion = 1;
        var itemToCreate = new WorkflowVersionCreate
        {
            WorkflowFormId = masterId,
            MajorVersion = majorVersion,
            MinorVersion = 3
        };
        await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var itemToUpdate = await _workflowVersionService.ReadAsync(id);

        // Act & Assert
        itemToUpdate.MajorVersion = itemToCreate.MajorVersion+1;
        itemToUpdate.MinorVersion =1;
        await Should.ThrowAsync<FulcrumContractException>(() =>
            _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate));
    }
}