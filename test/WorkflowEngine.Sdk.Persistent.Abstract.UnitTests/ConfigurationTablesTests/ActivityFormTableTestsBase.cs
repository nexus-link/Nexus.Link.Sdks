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
    public abstract class ActivityFormTableTestsBase : TablesTestsBase
    {

        protected ActivityFormTableTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
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
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;

            // Act
            var record =
                await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Assert
            record.MustBe(record.Id, recordToCreate);
        }


        [Fact]
        public async Task CreateWithSpecifiedIdAndReturn_02_Given_DuplicateId_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;
            await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act & Assert
            await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Theory]
        [MemberData(nameof(DataGenerator.BadActivityFormCreate), MemberType = typeof(DataGenerator))]
        public async Task CreateWithSpecifiedIdAndReturn_04_Given_BadItem_Gives_Exception(
            ActivityFormRecordCreate badItem, string description)
        {
            // Act & Assert
            await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), badItem)
                .ShouldThrowAsync<FulcrumContractException>(description);
        }

        #endregion

        #region Read

        [Fact]
        public async Task Read_01_Given_Normal_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;
            await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);

            // Act
            var record = await ConfigurationTables.ActivityForm.ReadAsync(id);

            // Assert
            record.MustBe(id, recordToCreate);
        }

        [Fact]
        public async Task Read_02_Given_UnknownId_Gives_Null()
        {
            // Act
            var record = await ConfigurationTables.ActivityForm.ReadAsync(Guid.NewGuid());

            // Assert
            record.ShouldBeNull();
        }

        #endregion

        #region SearchByWorkflowFormIdAsync

        [Fact]
        public async Task SearchByWorkflowFormIdAsync_01_Given_Existing_Gives_FindItem()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityFormCreate;
            var expectedRecord = await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);


            var workflowFormCreate = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm = await ConfigurationTables.WorkflowForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate);

            var id2 = Guid.NewGuid();
            var recordToCreate2 = DataGenerator.DefaultActivityFormCreate;
            recordToCreate2.WorkflowFormId = workflowForm.Id;
            await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id2, recordToCreate2);

            // Act
            var records =
                await ConfigurationTables.ActivityForm.SearchByWorkflowFormIdAsync(recordToCreate1.WorkflowFormId);

            // Assert
            records.ShouldNotBeNull();
            var array = records.ToArray();
            array.Length.ShouldBe(1);
            array[0].MustBe(expectedRecord);
        }

        [Fact]
        public async Task SearchByWorkflowFormIdAsync_02_Given_UnknownForm_Gives_NotFound()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var recordToCreate1 = DataGenerator.DefaultActivityFormCreate;
            await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id1, recordToCreate1);

            // Act
            var records =
                await ConfigurationTables.ActivityForm.SearchByWorkflowFormIdAsync(Guid.NewGuid());

            // Assert
            records.ShouldNotBeNull();
            var array = records.ToArray();
            array.Length.ShouldBe(0);
        }

        #endregion

        #region UpdateAndReturn

        [Fact]
        public async Task UpdateAndReturn_01_Given_TitleChange_Gives_CompleteRecord()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Title = Guid.NewGuid().ToGuidString();

            // Act
            var record = await ConfigurationTables.ActivityForm.UpdateAndReturnAsync(id, recordToUpdate);

            // Assert
            record.ShouldNotBeNull();
            record.MustBe(record.Id, recordToUpdate);
            record.RecordCreatedAt.ShouldBe(recordToUpdate.RecordCreatedAt);
            record.RecordUpdatedAt.ShouldBeGreaterThanOrEqualTo(recordToUpdate.RecordUpdatedAt);
            record.Etag.ShouldNotBe(recordToUpdate.Etag);
        }

        [Fact]
        public async Task UpdateAndReturn_02_Given_WrongTag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Etag = Guid.NewGuid().ToGuidString();

            // Act & assert
            await ConfigurationTables.ActivityForm.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumConflictException>();
        }

        [Fact]
        public async Task UpdateAndReturn_03_Given_TypeChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            recordToUpdate.Type = ActivityTypeEnum.If.ToString();

            // Act
            await ConfigurationTables.ActivityForm.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        [Fact]
        public async Task UpdateAndReturn_04_Given_WorkflowFormIdChange_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid();
            var recordToCreate = DataGenerator.DefaultActivityFormCreate;
            var recordToUpdate =
                await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, recordToCreate);
            var workflowFormCreate = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm = await ConfigurationTables.WorkflowForm
                .CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate);
            recordToUpdate.WorkflowFormId = workflowForm.Id;

            // Act
            await ConfigurationTables.ActivityForm.UpdateAndReturnAsync(id, recordToUpdate)
                .ShouldThrowAsync<FulcrumContractException>();
        }

        #endregion
    }
}