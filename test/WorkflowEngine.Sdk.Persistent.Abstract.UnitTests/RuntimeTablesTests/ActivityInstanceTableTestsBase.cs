using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support;
using Shouldly;
using Xunit;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.RuntimeTablesTests
{
    public abstract class ActivityInstanceTableTestsBase : TablesTestsBase
    {
        protected ActivityInstanceTableTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
        : base(configurationTables, runtimeTables)
        {
            var workflowFormCreate = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm = ConfigurationTables.WorkflowForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate).Result;
            DataGenerator.WorkflowFormId = workflowForm.Id;
            var workflowVersionCreate = DataGenerator.DefaultWorkflowVersionCreate;
            var workflowVersion = ConfigurationTables.WorkflowVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowVersionCreate).Result;
            DataGenerator.WorkflowVersionId = workflowVersion.Id;
            var workflowInstanceCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            var workflowInstance = RuntimeTables.WorkflowInstance
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowInstanceCreate).Result;
            DataGenerator.WorkflowInstanceId = workflowInstance.Id;
            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            var activityForm = ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate).Result;
            DataGenerator.ActivityFormId = activityForm.Id;
            var activityVersionCreate = DataGenerator.DefaultActivityVersionCreate;
            var activityVersion = ConfigurationTables.ActivityVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityVersionCreate).Result;
            DataGenerator.ActivityVersionId = activityVersion.Id;
        }

        #region CreateWithSpecifiedIdAndReturn

        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;

            // Act
            var record =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Assert
            record.MustBe(record.Id, recordToCreate);
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_02_Given_DuplicateId_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act & Assert
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_03_Given_NotUnique_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate);

            // Act & Assert
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Theory]
        [MemberData(nameof(DataGenerator.BadActivityInstanceCreate), MemberType = typeof(DataGenerator))]
        public async Task CreateWithSpecifiedIdAndReturn_04_Given_BadItem_Gives_Exception(
            ActivityInstanceRecordCreate badItem, string description)
        {
            // Act & Assert
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), badItem)
                .ShouldThrowAsync<FulcrumContractException>(description);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act
            var record = await RuntimeTables.ActivityInstance.ReadAsync(id);

            // Assert
            record.MustBe(id, recordToCreate);
        }

        [Fact]
        public async Task Read_02_Given_UnknownId_Gives_Null()
        {
            // Act
            var record = await RuntimeTables.ActivityInstance.ReadAsync(Guid.NewGuid());

            // Assert
            record.ShouldBeNull();
        }

        #endregion

        #region SearchByWorkflowInstanceId

        [Fact]
        public async Task SearchByWorkflowInstanceId_01_Given_Existing_Gives_FindItem()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityInstanceCreate;
            var expectedRecord = await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            var workflowInstanceCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            var workflowInstance = await RuntimeTables.WorkflowInstance
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowInstanceCreate);
            DataGenerator.WorkflowInstanceId = workflowInstance.Id;

            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultActivityInstanceCreate;
            recordToCreate2.WorkflowInstanceId = workflowInstance.Id;
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);

            // Act
            var records =
                await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(recordToCreate1.WorkflowInstanceId);

            // Assert
            records.ShouldNotBeNull();
            var array = records.ToArray();
            array.Length.ShouldBe(1);
            array[0].MustBe(expectedRecord);
        }

        [Fact]
        public async Task SearchByWorkflowInstanceId_02_Given_UnknownVersion_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityInstanceCreate;
            await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var records =
                await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(Guid.NewGuid());

            // Assert
            records.ShouldNotBeNull();
            var array = records.ToArray();
            array.Length.ShouldBe(0);
        }

        #endregion

        #region UpdateAndReturn

        [Fact]
        public async Task UpdateAndReturn_01_Given_WrongTag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Etag = Guid.NewGuid().ToGuidString();

            // Act & assert
            await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Fact]
        public async Task UpdateAndReturn_02_Given_AcceptedChange_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.ContextAsJson = JsonConvert.SerializeObject(Guid.NewGuid());
            recordToUpdate.ResultAsJson = JsonConvert.SerializeObject(Guid.NewGuid());
            recordToUpdate.FinishedAt = DateTimeOffset.UtcNow;
            recordToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToGuidString();
            recordToUpdate.ExceptionFriendlyMessage = Guid.NewGuid().ToGuidString();
            recordToUpdate.State = ActivityStateEnum.Failed.ToString();
            recordToUpdate.AsyncRequestId = Guid.NewGuid().ToGuidString();
            recordToUpdate.ExceptionAlertHandled = true;
            recordToUpdate.ExceptionCategory = ActivityExceptionCategoryEnum.TechnicalError.ToString();

            // Act
            var record = await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id, recordToUpdate);

            // Assert
            record.ShouldNotBeNull();
            record.MustBe(record.Id, recordToUpdate);
            record.RecordCreatedAt.ShouldBe(recordToUpdate.RecordCreatedAt);
            record.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(recordToUpdate.RecordUpdatedAt);
            record.Etag.ShouldNotBe(recordToUpdate.Etag);
        }

        [Fact]
        public async Task UpdateAndReturn_03_Given_StartedAtChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            
            recordToUpdate.StartedAt = DateTimeOffset.UtcNow;

            // Act
            await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_04_Given_WorkflowInstanceIdChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            activityFormCreate.Title = Guid.NewGuid().ToGuidString();
            var activityForm = await ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate);

            recordToUpdate.WorkflowInstanceId = activityForm.Id;

            // Act
            await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_05_Given_ActivityVersionIdChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            activityFormCreate.Title = Guid.NewGuid().ToGuidString();
            var activityForm = await ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate);

            var activityVersionCreate = DataGenerator.DefaultActivityVersionCreate;
            activityVersionCreate.ActivityFormId = activityForm.Id;
            var activityVersion = await ConfigurationTables.ActivityVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityVersionCreate);

            recordToUpdate.ActivityVersionId = activityVersion.Id;

            // Act
            await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_06_Given_ParentActivityInstanceIdChange_Gives_Exception()
        {
            // Arrange
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityInstanceCreate;
            var expectedRecord = await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            activityFormCreate.Title = Guid.NewGuid().ToGuidString();
            var activityForm = await ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate);

            var activityVersionCreate = DataGenerator.DefaultActivityVersionCreate;
            activityVersionCreate.ActivityFormId = activityForm.Id;
            var activityVersion = await ConfigurationTables.ActivityVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityVersionCreate);

            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultActivityInstanceCreate;
            recordToCreate2.ActivityVersionId = activityVersion.Id;
            var recordToUpdate = await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);

            recordToUpdate.ParentActivityInstanceId = id1;

            // Act
            await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id2, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_07_Given_ParentIterationChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            activityFormCreate.Title = Guid.NewGuid().ToGuidString();
            var activityForm = await ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate);

            var workflowFormCreate = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm = await ConfigurationTables.WorkflowForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate);

            recordToUpdate.ParentIteration = (recordToUpdate.ParentIteration ?? 0) + 1;

            // Act
            await RuntimeTables.ActivityInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        #endregion
    }
}