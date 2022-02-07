using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support;
using Shouldly;
using Xunit;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests
{
    public abstract class WorkflowFormTableTestsBase : TablesTestsBase
    {
        protected WorkflowFormTableTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
            : base(configurationTables, runtimeTables)
        {
        }

        #region CreateWithSpecifiedIdAndReturn

        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowFormCreate;

            // Act
            var record = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Assert
            record.ShouldNotBeNull();
            record.MustBe(record.Id, recordToCreate);
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_02_Given_DuplicateId_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act & Assert
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_03_Given_NotUnique_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate);

            // Act & Assert
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Theory]
        [MemberData(nameof(DataGenerator.BadWorkflowFormCreate), MemberType = typeof(DataGenerator))]
        public async Task CreateWithSpecifiedIdAndReturn_04_Given_BadItem_Gives_Exception(WorkflowFormRecordCreate badItem, string description)
        {
            // Act & Assert
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), badItem)
                .ShouldThrowAsync<FulcrumContractException>(description);
        }
        #endregion

        #region Read
        [Fact]
        public async Task Read_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act
            var record = await ConfigurationTables.WorkflowForm.ReadAsync(id);

            // Assert
            record.ShouldNotBeNull();
            record.MustBe(id, recordToCreate);
        }

        [Fact]
        public async Task Read_02_Given_UnknownId_Gives_Null()
        {
            // Act
            var record = await ConfigurationTables.WorkflowForm.ReadAsync(Guid.NewGuid());

            // Assert
            record.ShouldBeNull();
        }
        #endregion

        #region FindByCapabilityNameAndTitle
        [Fact]
        public async Task FindByCapabilityNameAndTitle_01_Given_Existing_Gives_FindItem()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);

            // Act
            var foundRecord = await ConfigurationTables.WorkflowForm.FindByCapabilityNameAndTitleAsync(recordToCreate1.CapabilityName, recordToCreate1.Title);

            // Assert
            foundRecord.ShouldNotBeNull();
            foundRecord.MustBe(recordToCreate1);
        }

        [Fact]
        public async Task FindByCapabilityNameAndTitle_02_Given_UnknownCapabilityName_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var foundRecord = await ConfigurationTables.WorkflowForm.FindByCapabilityNameAndTitleAsync("Unknown", recordToCreate1.Title);

            // Assert
            foundRecord.ShouldBeNull();
        }

        [Fact]
        public async Task FindByCapabilityNameAndTitle_03_Given_UnknownTitle_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var foundRecord = await ConfigurationTables.WorkflowForm.FindByCapabilityNameAndTitleAsync(recordToCreate1.CapabilityName, "Unknown");

            // Assert
            foundRecord.ShouldBeNull();
        }
        #endregion

        #region UpdateAndReturn
        [Fact]
        public async Task UpdateAndReturn_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultWorkflowFormCreate;
            var recordToUpdate = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.CapabilityName = Guid.NewGuid().ToGuidString();
            recordToUpdate.Title = Guid.NewGuid().ToGuidString();

            // Act
            var record = await ConfigurationTables.WorkflowForm.UpdateAndReturnAsync(id, recordToUpdate);

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
            var recordToCreate = DataGenerator.DefaultWorkflowFormCreate;
            var recordToUpdate = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Etag = Guid.NewGuid().ToGuidString();

            // Act & assert
            await ConfigurationTables.WorkflowForm.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }


        [Fact]
        public async Task UpdateAndReturn_03_Given_NotUnique_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultWorkflowFormCreate;
            await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);
            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultWorkflowFormCreate;
            var recordToUpdate = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);
            recordToUpdate.CapabilityName = recordToCreate1.CapabilityName;
            recordToUpdate.Title = recordToCreate1.Title;

            // Act & Assert
            await ConfigurationTables.WorkflowForm.UpdateAndReturnAsync(id2, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }
        #endregion
    }
}
