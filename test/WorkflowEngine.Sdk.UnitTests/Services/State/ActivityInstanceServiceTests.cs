using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Services.State;

public class ActivityInstanceServiceTests
{
    private static Mock<IHttpSender> _httpSenderMock;

    private readonly IActivityInstanceService _service; 
    
    public static IHttpSender HttpSender
    {
        get
        {
            if (_httpSenderMock != null) return _httpSenderMock.Object;
            _httpSenderMock = new Mock<IHttpSender>();
            _httpSenderMock.Setup(sender => sender.SendRequestAsync(
                    It.IsAny<HttpMethod>(),
                    It.IsAny<string>(), null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpMethod _, string _,
                    Dictionary<string, List<string>> _,
                    CancellationToken _) =>
                {

                    return new HttpResponseMessage(HttpStatusCode.OK);
                });
            _httpSenderMock.Setup(sender => sender.CreateHttpSender(
                    It.IsAny<string>()))
                .Returns((string _) => _httpSenderMock.Object);
            return _httpSenderMock.Object;
        }
    }

    public ActivityInstanceServiceTests()
    {
        _service = new ActivityInstanceService(new RuntimeTablesMemory(), new AsyncRequestMgmtRestClients(HttpSender));
    }

    [Fact]
    public async Task CreateAndReadAsync()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var workflowInstanceId = Guid.NewGuid().ToGuidString();
        var activityVersionId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityInstanceCreate
        {
            WorkflowInstanceId = workflowInstanceId,
            ActivityVersionId = activityVersionId,
            StartedAt = DateTimeOffset.UtcNow,
            ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
            ParentIteration = 1, 
        };

        // Act
        var created = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var findUnique = new ActivityInstanceUnique
        {
            WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
            ActivityVersionId = itemToCreate.ActivityVersionId,
            ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
            ParentIteration = itemToCreate.ParentIteration
        };
        var readItem = await _service.ReadAsync(id);

        // Assert
        readItem.ShouldNotBeNull();
        readItem.Id.ShouldBeEquivalentTo(created.Id);
        readItem.WorkflowInstanceId.ShouldBeEquivalentTo(workflowInstanceId);
        readItem.ActivityVersionId.ShouldBeEquivalentTo(itemToCreate.ActivityVersionId);
        readItem.ParentActivityInstanceId.ShouldBeEquivalentTo(itemToCreate.ParentActivityInstanceId);
        readItem.ParentIteration.ShouldBeEquivalentTo(itemToCreate.ParentIteration);
    }

    [Fact]
    public async Task UpdateAsync()
    {
        // Arrange
        var id = Guid.NewGuid().ToGuidString();
        var workflowInstanceId = Guid.NewGuid().ToGuidString();
        var activityVersionId = Guid.NewGuid().ToGuidString();
        var itemToCreate = new ActivityInstanceCreate
        {
            WorkflowInstanceId = workflowInstanceId,
            ActivityVersionId = activityVersionId,
            StartedAt = DateTimeOffset.UtcNow,
            ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
            ParentIteration = 1, 
        };
        var created = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
        var findUnique = new ActivityInstanceUnique
        {
            WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
            ActivityVersionId = itemToCreate.ActivityVersionId,
            ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
            ParentIteration = itemToCreate.ParentIteration
        };
        var itemToUpdate = await _service.ReadAsync(id);

        // Act
        itemToUpdate.FinishedAt = DateTimeOffset.Now;
        itemToUpdate.State = ActivityStateEnum.Failed;
        itemToUpdate.ExceptionCategory = ActivityExceptionCategoryEnum.TechnicalError;
        itemToUpdate.ExceptionFriendlyMessage =  Guid.NewGuid().ToGuidString();
        itemToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToGuidString();
        var updatedItem = await _service.UpdateAndReturnAsync(created.Id, itemToUpdate);

        // Assert
        updatedItem.ShouldNotBeNull();
        updatedItem.Id.ShouldBeEquivalentTo(created.Id);
        updatedItem.WorkflowInstanceId.ShouldBeEquivalentTo(created.WorkflowInstanceId);
        updatedItem.ActivityVersionId.ShouldBeEquivalentTo(created.ActivityVersionId);
        updatedItem.ParentActivityInstanceId.ShouldBeEquivalentTo(itemToUpdate.ParentActivityInstanceId);
        updatedItem.ParentIteration.ShouldBeEquivalentTo(itemToUpdate.ParentIteration);
        updatedItem.FinishedAt.ShouldBeEquivalentTo(itemToUpdate.FinishedAt);
        updatedItem.State.ShouldBeEquivalentTo(itemToUpdate.State);
        updatedItem.ExceptionCategory.ShouldBeEquivalentTo(itemToUpdate.ExceptionCategory);
        updatedItem.ExceptionFriendlyMessage.ShouldBeEquivalentTo(itemToUpdate.ExceptionFriendlyMessage);
        updatedItem.ExceptionTechnicalMessage.ShouldBeEquivalentTo(itemToUpdate.ExceptionTechnicalMessage);
    }
}