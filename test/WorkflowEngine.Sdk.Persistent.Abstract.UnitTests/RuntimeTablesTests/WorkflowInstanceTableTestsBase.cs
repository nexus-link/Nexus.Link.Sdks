using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support;
using Shouldly;
using Xunit;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.RuntimeTablesTests
{
    public abstract class WorkflowInstanceTableTestsBase : TablesTestsBase
    {

        protected WorkflowInstanceTableTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
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
        }

        #region CreateWithSpecifiedIdAndReturn

        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowInstanceCreate;

            // Act
            var record =
                await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Assert
            record.MustBe(record.Id, recordToCreate);
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_02_Given_DuplicateId_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act & Assert
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Theory]
        [MemberData(nameof(DataGenerator.BadWorkflowInstanceCreate), MemberType = typeof(DataGenerator))]
        public async Task CreateWithSpecifiedIdAndReturn_03_Given_BadItem_Gives_Exception(
            WorkflowInstanceRecordCreate badItem, string description)
        {
            // Act & Assert
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), badItem)
                .ShouldThrowAsync<FulcrumContractException>(description);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act
            var record = await RuntimeTables.WorkflowInstance.ReadAsync(id);

            // Assert
            record.MustBe(id, recordToCreate);
        }

        [Fact]
        public async Task Read_02_Given_UnknownId_Gives_Null()
        {
            // Act
            var record = await RuntimeTables.WorkflowInstance.ReadAsync(Guid.NewGuid());

            // Assert
            record.ShouldBeNull();
        }

        #endregion

        #region UpdateAndReturn

        [Fact]
        public async Task UpdateAndReturn_01_Given_WrongTag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Etag = Guid.NewGuid().ToGuidString();

            // Act & assert
            await RuntimeTables.WorkflowInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Fact]
        public async Task UpdateAndReturn_02_Given_AcceptedChanges_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.StartedAt = DateTimeOffset.UtcNow;
            recordToUpdate.CancelledAt = DateTimeOffset.UtcNow;
            recordToUpdate.FinishedAt = DateTimeOffset.UtcNow;
            recordToUpdate.Title = Guid.NewGuid().ToGuidString();
            recordToUpdate.InitialVersion = Guid.NewGuid().ToGuidString();
            recordToUpdate.ExceptionFriendlyMessage = Guid.NewGuid().ToGuidString();
            recordToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToGuidString();
            recordToUpdate.IsComplete = !recordToUpdate.IsComplete;
            recordToUpdate.ResultAsJson = JsonConvert.SerializeObject(Guid.NewGuid());
            recordToUpdate.State = WorkflowStateEnum.Failed.ToString();

            // Act
            var record = await RuntimeTables.WorkflowInstance.UpdateAndReturnAsync(id, recordToUpdate);
            
            // Assert
            record.ShouldNotBeNull();
            record.MustBe(record.Id, recordToUpdate);
            record.RecordCreatedAt.ShouldBe(recordToUpdate.RecordCreatedAt);
            record.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(recordToUpdate.RecordUpdatedAt);
            record.Etag.ShouldNotBe(recordToUpdate.Etag);
        }

        [Fact]
        public async Task UpdateAndReturn_03_Given_WorkflowVersionIdChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowInstanceCreate;
            var recordToUpdate =
                await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var workflowVersionCreate = DataGenerator.DefaultWorkflowVersionCreate;
            workflowVersionCreate.MajorVersion++;
            var workflowVersion = await ConfigurationTables.WorkflowVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowVersionCreate);

            recordToUpdate.WorkflowVersionId = workflowVersion.Id;

            // Act
            await RuntimeTables.WorkflowInstance.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        #endregion
    }
}