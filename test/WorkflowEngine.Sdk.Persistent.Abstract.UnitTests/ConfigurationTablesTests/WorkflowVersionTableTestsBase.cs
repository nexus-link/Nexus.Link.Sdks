using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support;
using Shouldly;
using Xunit;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests
{
    public abstract class WorkflowVersionTableTestsBase : TablesTestsBase
    {

        protected WorkflowVersionTableTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
        : base(configurationTables, runtimeTables)
        {
            var workflowFormCreate = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm = ConfigurationTables.WorkflowForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate).Result;
            DataGenerator.WorkflowFormId = workflowForm.Id;
        }

        #region CreateWithSpecifiedIdAndReturn

        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;

            // Act
            var record =
                await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Assert
            record.MustBe(record.Id, recordToCreate);
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_02_Given_DuplicateId_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act & Assert
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_03_Given_NotUnique_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate);

            // Act & Assert
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Theory]
        [MemberData(nameof(DataGenerator.BadWorkflowVersionCreate), MemberType = typeof(DataGenerator))]
        public async Task CreateWithSpecifiedIdAndReturn_04_Given_BadItem_Gives_Exception(
            WorkflowVersionRecordCreate badItem, string description)
        {
            // Act & Assert
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), badItem)
                .ShouldThrowAsync<FulcrumContractException>(description);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act
            var record = await ConfigurationTables.WorkflowVersion.ReadAsync(id);

            // Assert
            record.MustBe(id, recordToCreate);
        }

        [Fact]
        public async Task Read_02_Given_UnknownId_Gives_Null()
        {
            // Act
            var record = await ConfigurationTables.WorkflowVersion.ReadAsync(Guid.NewGuid());

            // Assert
            record.ShouldBeNull();
        }

        #endregion

        #region FindByFormAndMajorAsync

        [Fact]
        public async Task FindByFormAndMajorAsync_01_Given_Existing_Gives_FindItem()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowVersionCreate;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultWorkflowVersionCreate;
            recordToCreate2.MajorVersion++;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);

            // Act
            var foundRecord =
                await ConfigurationTables.WorkflowVersion.FindByFormAndMajorAsync(recordToCreate1.WorkflowFormId,
                    recordToCreate1.MajorVersion);

            // Assert
            foundRecord.MustBe(recordToCreate1);
        }

        [Fact]
        public async Task FindByFormAndMajorAsync_02_Given_UnknownForm_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowVersionCreate;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var foundRecord =
                await ConfigurationTables.WorkflowVersion.FindByFormAndMajorAsync(Guid.NewGuid(),
                    recordToCreate1.MajorVersion);

            // Assert
            foundRecord.ShouldBeNull();
        }

        [Fact]
        public async Task FindByFormAndMajorAsync_03_Given_UnknownVersion_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowVersionCreate;
            await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var foundRecord = await ConfigurationTables.WorkflowVersion.FindByFormAndMajorAsync(Guid.NewGuid(), 1000);

            // Assert
            foundRecord.ShouldBeNull();
        }

        #endregion

        #region UpdateAndReturn

        [Fact]
        public async Task UpdateAndReturn_01_Given_MinorVersionChange_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.MinorVersion++;
            await Task.Delay(100);

            // Act
            var record = await ConfigurationTables.WorkflowVersion.UpdateAndReturnAsync(id, recordToUpdate);

            // Assert
            record.ShouldNotBeNull();
            record.MustBe(record.Id, recordToUpdate);
            record.RecordCreatedAt.ShouldBe(recordToUpdate.RecordCreatedAt);
            record.RecordUpdatedAt.ShouldBeGreaterThan(recordToUpdate.RecordUpdatedAt);
            record.Etag.ShouldNotBe(recordToUpdate.Etag);
        }

        [Fact]
        public async Task UpdateAndReturn_02_Given_WrongTag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Etag = Guid.NewGuid().ToGuidString();

            // Act & assert
            await ConfigurationTables.WorkflowVersion.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Fact]
        public async Task UpdateAndReturn_03_Given_WorkflowFormIdChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            var workflowFormCreate = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm = await ConfigurationTables.WorkflowForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate);

            recordToUpdate.WorkflowFormId = workflowForm.Id;

            // Act
            await ConfigurationTables.WorkflowVersion.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_04_Given_MajorVersionChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowVersionCreate;
            var recordToUpdate =
                await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.MajorVersion++;

            // Act
            await ConfigurationTables.WorkflowVersion.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        #endregion
    }
}