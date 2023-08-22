using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support;
using Nexus.Link.WorkflowEngine.Sdk.Services.Administration;
using Shouldly;
using Xunit;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Mgmt
{
    public abstract class WorkflowServiceTests : TablesTestsBase
    {
        private readonly IInstanceService _instanceService;
        private readonly IFormOverviewService _formOverviewService;

        private WorkflowInstanceRecord _record1Success;
        private WorkflowInstanceRecord _record2Success;
        private WorkflowInstanceRecord _record3Failed;

        private readonly Guid _formId = Guid.NewGuid();

        protected WorkflowServiceTests(IConfigurationTables configurationTables, IRuntimeTables runtimeTables) : base(configurationTables, runtimeTables)
        {
            _instanceService = new InstanceService(runtimeTables);
            _formOverviewService = new FormOverviewService(configurationTables);
        }

        private async Task CreateDataSetAsync()
        {
            var workflowFormCreate1 = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm1 = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(_formId, workflowFormCreate1);
            DataGenerator.WorkflowFormId = workflowForm1.Id;
            var workflowVersionCreate1 = DataGenerator.DefaultWorkflowVersionCreate;
            var workflowVersion1 = ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowVersionCreate1).Result;

            // Note! Do not change, tests depend on it
            _record1Success = new WorkflowInstanceRecord
            {
                WorkflowVersionId = workflowVersion1.Id,
                Id = Guid.NewGuid(),
                InitialVersion = "1.0",
                State = WorkflowStateEnum.Success.ToString(),
                Title = "title is 1",
                StartedAt = DateTimeOffset.Now.AddMinutes(-120),
                FinishedAt = DateTimeOffset.Now.AddMinutes(-119),
            };
            _record2Success = new WorkflowInstanceRecord
            {
                WorkflowVersionId = workflowVersion1.Id,
                Id = Guid.NewGuid(),
                InitialVersion = "1.0",
                State = WorkflowStateEnum.Success.ToString(),
                Title = "title is 2",
                StartedAt = DateTimeOffset.Now.AddMinutes(-2),
                FinishedAt = DateTimeOffset.Now.AddMinutes(-1),
            };
            _record3Failed = new WorkflowInstanceRecord
            {
                WorkflowVersionId = workflowVersion1.Id,
                Id = Guid.NewGuid(),
                InitialVersion = "1.0",
                State = WorkflowStateEnum.Failed.ToString(),
                Title = "title is 3",
                StartedAt = DateTimeOffset.Now.AddMinutes(-60),
                FinishedAt = DateTimeOffset.Now.AddMinutes(-59),
            };
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(_record1Success.Id, _record1Success);
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(_record2Success.Id, _record2Success);
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(_record3Failed.Id, _record3Failed);


            var workflowFormCreate2 = DataGenerator.DefaultWorkflowFormCreate;
            var workflowForm2 = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowFormCreate2);
            DataGenerator.WorkflowFormId = workflowForm2.Id;
            var workflowVersionCreate2 = DataGenerator.DefaultWorkflowVersionCreate;
            var workflowVersion2 = ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), workflowVersionCreate2).Result;

            var otherRecord = new WorkflowInstanceRecord
            {
                WorkflowVersionId = workflowVersion2.Id,
                Id = Guid.NewGuid(),
                InitialVersion = "1.0",
                State = WorkflowStateEnum.Executing.ToString(),
                Title = "other",
                StartedAt = DateTimeOffset.Now.AddMinutes(-60),
                FinishedAt = DateTimeOffset.Now.AddMinutes(-59),
            };
            await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(otherRecord.Id, otherRecord);
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_Single_State()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                States = new List<WorkflowStateEnum> { WorkflowStateEnum.Success }
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(2);
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record1Success.Id.ToGuidString()).ShouldNotBeNull();
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record2Success.Id.ToGuidString()).ShouldNotBeNull();
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_Multiple_State()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                States = new List<WorkflowStateEnum> { WorkflowStateEnum.Success, WorkflowStateEnum.Failed }
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(3);
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record1Success.Id.ToGuidString()).ShouldNotBeNull();
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record2Success.Id.ToGuidString()).ShouldNotBeNull();
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record3Failed.Id.ToGuidString()).ShouldNotBeNull();
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_From()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddMinutes(-10)
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(1);
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record2Success.Id.ToGuidString()).ShouldNotBeNull();
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_To()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddMinutes(-130),
                To = DateTimeOffset.Now.AddMinutes(-110)
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(1);
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record1Success.Id.ToGuidString()).ShouldNotBeNull();
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_FormId()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                FormId = _formId.ToGuidString(),
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(3);
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_Title_Part()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                TitlePart = "title is"
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(3);
        }

        [Fact]
        public async Task Instances_Can_Be_Found_By_Multiple_Filters()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                To = DateTimeOffset.Now.AddMinutes(-110),
                FormId = _formId.ToGuidString(),
                States = new List<WorkflowStateEnum> { WorkflowStateEnum.Success },
                TitlePart = "title is"
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(1);
            result.Data.FirstOrDefault(x => x.Id.ToGuidString() == _record1Success.Id.ToGuidString()).ShouldNotBeNull();
        }

        [Fact]
        public async Task Instances_Can_Be_Ordered_By_StartedAt_Asc()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                FormId = _formId.ToGuidString(),
                Order = new WorkflowInstanceSearchOrder
                {
                    PrimaryOrderBy = WorkflowSearchOrderByEnum.StartedAt,
                    PrimaryAscendingOrder = true
                }
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(3);
            var dataList = result.Data.ToList();

            dataList[0].Id.ToGuidString().ShouldBe(_record1Success.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
            dataList[1].Id.ToGuidString().ShouldBe(_record3Failed.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
            dataList[2].Id.ToGuidString().ShouldBe(_record2Success.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
        }

        [Fact]
        public async Task Instances_Can_Be_Ordered_By_StartedAt_Desc()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                FormId = _formId.ToGuidString(),
                Order = new WorkflowInstanceSearchOrder
                {
                    PrimaryOrderBy = WorkflowSearchOrderByEnum.StartedAt,
                    PrimaryAscendingOrder = false
                }
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(3);
            var dataList = result.Data.ToList();

            dataList[0].Id.ToGuidString().ShouldBe(_record2Success.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
            dataList[1].Id.ToGuidString().ShouldBe(_record3Failed.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
            dataList[2].Id.ToGuidString().ShouldBe(_record1Success.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
        }

        [Fact]
        public async Task Instances_Can_Be_Ordered_By_State_Then_StartedAt()
        {
            // Arrange
            await CreateDataSetAsync();

            // Act
            var result = await _instanceService.SearchAsync(new WorkflowInstanceSearchDetails
            {
                From = DateTimeOffset.Now.AddDays(-1),
                FormId = _formId.ToGuidString(),
                Order = new WorkflowInstanceSearchOrder
                {
                    PrimaryOrderBy = WorkflowSearchOrderByEnum.State,
                    PrimaryAscendingOrder = true,
                    SecondaryOrderBy = WorkflowSearchOrderByEnum.StartedAt,
                    SecondaryAscendingOrder = true
                }
            }, 0, 10);

            // Assert
            result.PageInfo.Returned.ShouldBe(3);
            var dataList = result.Data.ToList();

            dataList[0].Id.ToGuidString().ShouldBe(_record3Failed.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
            dataList[1].Id.ToGuidString().ShouldBe(_record1Success.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
            dataList[2].Id.ToGuidString().ShouldBe(_record2Success.Id.ToGuidString(), JsonConvert.SerializeObject(dataList, Formatting.Indented));
        }

        [Fact]
        public async Task A_List_Of_Form_Overviews_Can_Be_Fetched()
        {
            // Arrange
            await CreateDataSetAsync();
            var from = DateTimeOffset.Now.AddDays(-1);
            var to = DateTimeOffset.Now;

            // Act
            var result = await _formOverviewService.ReadByIntervalWithPagingAsync(from, to, FormOverviewIncludeFilter.All);

            // Assert
            result.Count.ShouldBe(2);
            var overview = result.First(x => x.Id.ToGuidString() == _formId.ToGuidString()).Overview;
            overview.InstancesFrom.ShouldBe(from);
            overview.InstancesTo.ShouldBe(to);
            overview.InstanceCount.ShouldBe(3);
            overview.ErrorCount.ShouldBe(1);
            overview.WaitingCount.ShouldBe(0);
            overview.ExecutingCount.ShouldBe(0);
        }
    }
}