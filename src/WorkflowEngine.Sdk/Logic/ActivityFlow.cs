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
        /// <inheritdoc />
        public WorkflowInformation WorkflowInformation{ get; }

        /// <inheritdoc />
        public WorkflowCache WorkflowCache { get; }

        /// <inheritdoc />
        public string ActivityFormId { get; }

        /// <inheritdoc />
        public MethodHandler MethodHandler { get; }

        /// <inheritdoc />
        public string FormTitle { get; }

        /// <inheritdoc />
        public ActivityFailUrgencyEnum FailUrgency { get; protected set; }

        /// <inheritdoc />
        public double AsyncRequestPriority { get; protected set; }

        /// <inheritdoc />
        public ActivityExceptionAlertHandler ExceptionAlertHandler { get; protected set; }
        
        /// <inheritdoc />
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
            FailUrgency = workflowInformation.DefaultFailUrgency;
            AsyncRequestPriority = workflowInformation.DefaultAsyncRequestPriority;
            ExceptionAlertHandler = workflowInformation.DefaultExceptionAlertHandler;
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
        public IActivityFlow SetAsyncRequestPriority(double priority)
        {
            AsyncRequestPriority = priority;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            FailUrgency = failUrgency;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow SetExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            ExceptionAlertHandler = alertHandler;
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
        public Func<CancellationToken, Task<TActivityReturns>> GetDefaultValueForNotUrgentFail { get; private set; }

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
        public IActivityFlow<TActivityReturns> SetAsyncRequestPriority(double priority)
        {
            AsyncRequestPriority = priority;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            FailUrgency = failUrgency;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            ExceptionAlertHandler = alertHandler;
            return this;
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(TActivityReturns defaultValue)
        {
            return SetDefaultValueForNotUrgentFail(ct => Task.FromResult(defaultValue));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(Func<TActivityReturns> getDefaultValueMethod)
        {
            return SetDefaultValueForNotUrgentFail(ct => Task.FromResult(getDefaultValueMethod()));
        }

        /// <inheritdoc />
        public IActivityFlow<TActivityReturns> SetDefaultValueForNotUrgentFail(Func<CancellationToken, Task<TActivityReturns>> getDefaultValueMethodAsync)
        {
            GetDefaultValueForNotUrgentFail = getDefaultValueMethodAsync;
            return this;
        }

        /// <inheritdoc/>
        public IActivityAction<TActivityReturns> Action()
        {
            return new ActivityAction<TActivityReturns>(this, GetDefaultValueForNotUrgentFail);
        }
        
        /// <inheritdoc/>
        public IActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            return new ActivityLoopUntilTrue<TActivityReturns>(this, GetDefaultValueForNotUrgentFail);
        }

        /// <inheritdoc />
        public IActivityCondition<TActivityReturns> Condition()
        {
            return new ActivityCondition<TActivityReturns>(this, GetDefaultValueForNotUrgentFail);
        }

        /// <inheritdoc/>
        public IActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TActivityReturns, TItem, TKey>(this, items, GetDefaultValueForNotUrgentFail);
        }
        
        /// <inheritdoc/>
        public IActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TActivityReturns, TItem, TItem>(this, items, GetDefaultValueForNotUrgentFail);
        }
        
        /// <inheritdoc/>
        public IActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachSequential<TActivityReturns, TItem>(this, items, GetDefaultValueForNotUrgentFail);
        }
    }
}