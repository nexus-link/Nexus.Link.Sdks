using System;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support;
using Shouldly;
using Xunit;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests
{
    public abstract class ActivityVersionTableTestsBase : TablesTestsBase
    {

        protected ActivityVersionTableTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
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
            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            var activityForm = ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate).Result;
            DataGenerator.ActivityFormId = activityForm.Id;
        }

        #region CreateWithSpecifiedIdAndReturn

        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;

            // Act
            var record =
                await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Assert
            record.MustBe(record.Id, recordToCreate);
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_02_Given_DuplicateId_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act & Assert
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_03_Given_NotUnique_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate);

            // Act & Assert
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Theory]
        [MemberData(nameof(DataGenerator.BadActivityVersionCreate), MemberType = typeof(DataGenerator))]
        public async Task CreateWithSpecifiedIdAndReturn_04_Given_BadItem_Gives_Exception(
            ActivityVersionRecordCreate badItem, string description)
        {
            // Act & Assert
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), badItem)
                .ShouldThrowAsync<FulcrumContractException>(description);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act
            var record = await ConfigurationTables.ActivityVersion.ReadAsync(id);

            // Assert
            record.MustBe(id, recordToCreate);
        }

        [Fact]
        public async Task Read_02_Given_UnknownId_Gives_Null()
        {
            // Act
            var record = await ConfigurationTables.ActivityVersion.ReadAsync(Guid.NewGuid());

            // Assert
            record.ShouldBeNull();
        }

        #endregion

        #region SearchByWorkflowVersionId

        [Fact]
        public async Task SearchByWorkflowVersionId_01_Given_Existing_Gives_FindItem()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityVersionCreate;
            var expectedRecord = await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            var workflowVersionCreate = DataGenerator.DefaultWorkflowVersionCreate;
            workflowVersionCreate.MajorVersion++;
            var workflowVersion = await ConfigurationTables.WorkflowVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowVersionCreate);

            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultActivityVersionCreate;
            recordToCreate2.WorkflowVersionId = workflowVersion.Id;
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);

            // Act
            var records =
                await ConfigurationTables.ActivityVersion.SearchByWorkflowVersionIdAsync(recordToCreate1.WorkflowVersionId);

            // Assert
            records.ShouldNotBeNull();
            var array = records.ToArray();
            array.Length.ShouldBe(1);
            array[0].MustBe(expectedRecord);
        }

        [Fact]
        public async Task SearchByWorkflowVersionId_02_Given_UnknownVersion_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityVersionCreate;
            await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var records =
                await ConfigurationTables.ActivityVersion.SearchByWorkflowVersionIdAsync(Guid.NewGuid());

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
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Etag = Guid.NewGuid().ToGuidString();

            // Act & assert
            await ConfigurationTables.ActivityVersion.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Fact]
        public async Task UpdateAndReturn_02_Given_AcceptedChange_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Position++;
            recordToUpdate.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow.ToString();

            // Act
            var record = await ConfigurationTables.ActivityVersion.UpdateAndReturnAsync(id, recordToUpdate);

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
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var workflowVersionCreate = DataGenerator.DefaultWorkflowVersionCreate;
            workflowVersionCreate.MajorVersion++;
            var workflowVersion = await ConfigurationTables.WorkflowVersion
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowVersionCreate);

            recordToUpdate.WorkflowVersionId = workflowVersion.Id;

            // Act
            await ConfigurationTables.ActivityVersion.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_04_Given_ActivityFormIdChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var activityFormCreate = DataGenerator.DefaultActivityFormCreate;
            activityFormCreate.Title = Guid.NewGuid().ToGuidString();
            var activityForm = await ConfigurationTables.ActivityForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), activityFormCreate);

            recordToUpdate.ActivityFormId = activityForm.Id;

            // Act
            await ConfigurationTables.ActivityVersion.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        #endregion
    }
}