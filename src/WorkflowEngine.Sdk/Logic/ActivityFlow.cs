using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support.Method;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    internal abstract class ActivityFlowBase : IInternalActivityFlow
    {
        public WorkflowInformation WorkflowInformation{ get; }
        public WorkflowCache WorkflowCache { get; }
        public string ActivityFormId { get; }
        public MethodHandler MethodHandler { get; }
        public string FormTitle { get; }
        public ActivityFailUrgencyEnum FailUrgency { get; protected set; }
        public int Position { get; }

        protected ActivityFlowBase(WorkflowInformation workflowInformation,
            WorkflowCache workflowCache, int position, string formTitle, string activityFormId)
        {
            WorkflowCache = workflowCache;
            WorkflowInformation = workflowInformation;
            Position = position;
            FormTitle = formTitle;
            ActivityFormId = activityFormId;
            MethodHandler = new MethodHandler(formTitle);
            FailUrgency = ActivityFailUrgencyEnum.Stopping;
        }
    }

    internal class ActivityFlow : ActivityFlowBase, IActivityFlow
    {
        public ActivityFlow(WorkflowInformation workflowInformation, WorkflowCache workflowCache,
            int position, string formTitle, string activityFormId)
        : base(workflowInformation, workflowCache, position, formTitle, activityFormId)
        {
        }

        public IActivityFlow SetParameter<T>(string name, T value)
        {
            MethodHandler.DefineParameter<T>(name);
            MethodHandler.SetParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow OnException(ActivityFailUrgencyEnum failUrgency)
        {
            FailUrgency = failUrgency;
            return this;
        }

        /// <inheritdoc/>
        public IActivityAction Action()
        {
            return new ActivityAction(this);
        }
        
        /// <inheritdoc/>
        public IActivityLoopUntilTrue LoopUntil()
        {
            return new ActivityLoopUntilTrue(this);
        }
        
        /// <inheritdoc/>
        public IActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TItem>(this, items);
        }
        
        /// <inheritdoc/>
        public IActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachSequential<TItem>(this, items);
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {
        public Func<CancellationToken, Task<TActivityReturns>> GetDefaultValueMethodAsync { get; private set; }

        public ActivityFlow(WorkflowInformation workflowInformation,
            WorkflowCache workflowCache, int position, string formTitle, string activityFormId)
        : base(workflowInformation, workflowCache, position, formTitle, activityFormId)
        {
        }

        public IActivityFlow<TActivityReturns> SetParameter<T>(string name, T value)
        {
            MethodHandler.DefineParameter<T>(name);
            MethodHandler.SetParameter(name, value);
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, TActivityReturns defaultValue)
        {
            return OnException(failUrgency, ct => Task.FromResult(defaultValue));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<TActivityReturns> getDefaultValueMethod)
        {
            return OnException(failUrgency, ct => Task.FromResult(getDefaultValueMethod()));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> OnException(ActivityFailUrgencyEnum failUrgency, Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
        {
            FailUrgency = failUrgency;
            GetDefaultValueMethodAsync = getDefaultValueMethodAsync;
            return this;
        }

        /// <inheritdoc/>
        public IActivityAction<TActivityReturns> Action()
        {
            return new ActivityAction<TActivityReturns>(this, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            return new ActivityLoopUntilTrue<TActivityReturns>(this, GetDefaultValueMethodAsync);
        }

        /// <inheritdoc />
        public IActivityCondition<TActivityReturns> Condition()
        {
            return new ActivityCondition<TActivityReturns>(this, GetDefaultValueMethodAsync);
        }

        /// <inheritdoc/>
        public IActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TActivityReturns, TItem, TKey>(this, items, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public IActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TActivityReturns, TItem, TItem>(this, items, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachSequential<TActivityReturns, TItem>(this, items, GetDefaultValueMethodAsync);
        }
    }
}