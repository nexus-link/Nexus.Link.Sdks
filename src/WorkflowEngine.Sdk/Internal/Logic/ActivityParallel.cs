using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class ActivityParallel : Activity, IActivityParallel
    {
        private readonly Dictionary<int, Func<IActivityParallel, CancellationToken, Task>> _objectJobs = new();
        private readonly Dictionary<int, Type> _objectTypes = new();
        private readonly Dictionary<int, Task> _objectTasks = new();
        private readonly Dictionary<int, Func<IActivityParallel, CancellationToken, Task>> _voidJobs = new();
        private readonly Dictionary<int, Task> _voidTasks = new();

        public ActivityParallel(IInternalActivityFlow activityFlow)
            : base(ActivityTypeEnum.Parallel, activityFlow)
        {
            Iteration = 0;
        }

        /// <inheritdoc />
        public IActivityParallel AddJob(int index, Func<IActivityParallel, CancellationToken, Task> job)
        {
            InternalContract.RequireGreaterThan(0, index, nameof(index));
            InternalContract.RequireNotNull(job, nameof(job));
            InternalContract.Require(!_voidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
            InternalContract.Require(!_objectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
            _voidJobs.Add(index, job);
            return this;
        }

        /// <inheritdoc />
        public IActivityParallel AddJob<TMethodReturns>(int index, 
            Func<IActivityParallel, CancellationToken, Task<TMethodReturns>> job, 
            Func<CancellationToken, Task<TMethodReturns>> getDefaultValueMethodAsync = null)
        {
            InternalContract.RequireGreaterThan(0, index, nameof(index));
            InternalContract.RequireNotNull(job, nameof(job));
            InternalContract.Require(!_voidJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
            InternalContract.Require(!_objectJobs.Keys.Contains(index), $"{nameof(index)} {index} already exists.");
            _objectJobs.Add(index, job);
            _objectTypes.Add(index, typeof(TMethodReturns));
            return this;
        }

        public async Task<IJobResults> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var result = await InternalExecuteAsync(
                (_, ct) => ExecuteJobsAsync(ct),
                _ => null,
                cancellationToken);
            return result;
        }

        private async Task<JobResults> ExecuteJobsAsync(CancellationToken cancellationToken = default)
        {
            WorkflowStatic.Context.ParentActivityInstanceId = Instance.Id;
            foreach (var (index, job) in _voidJobs)
            {
                Iteration = index;
                WorkflowCache.LatestActivity = this;
                var task = MapMethodAsync(job, this, cancellationToken);
                _voidTasks.Add(index, task);
            }
            foreach (var (index, job) in _objectJobs)
            {
                Iteration = index;
                WorkflowCache.LatestActivity = this;
                var task = MapMethodAsync(job, this, cancellationToken);
                _objectTasks.Add(index, task);
            }
            WorkflowCache.LatestActivity = this;
            var taskList = new List<Task>(_voidTasks.Values);
            taskList.AddRange(_objectTasks.Values);
            await WorkflowHelper.WhenAllActivities(taskList);
            var jobResults = new JobResults();
            foreach (var (index, task) in _objectTasks)
            {
                // https://stackoverflow.com/questions/48033760/cast-taskt-to-taskobject-in-c-sharp-without-having-t
                var result =  (object)((dynamic)task).Result;
                jobResults.Add(index, result, _objectTypes[index]);
            }
            return jobResults;
        }

        private static Task MapMethodAsync(
            Func<IActivityParallel, CancellationToken, Task> method,
            IActivity instance, CancellationToken cancellationToken)
        {
            var activity = instance as IActivityParallel;
            FulcrumAssert.IsNotNull(activity, CodeLocation.AsString());
            return method(activity, cancellationToken);
        }
    }
}