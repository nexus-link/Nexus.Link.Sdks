using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic.Activities;

namespace Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic
{
    internal abstract class ActivityFlowBase : IInternalActivityFlow
    {
        public IWorkflowVersion WorkflowVersion { get; }
        public  WorkflowPersistence WorkflowPersistence { get; }
        protected IWorkflowCapability WorkflowCapability { get; }
        protected IAsyncRequestClient AsyncRequestClient { get; }
        public string ActivityFormId { get; }
        public MethodHandler MethodHandler { get; }
        public string FormTitle { get; }
        public ActivityFailUrgencyEnum FailUrgency { get; protected set; }
        public int Position { get; }

        protected ActivityFlowBase(IWorkflowVersion workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId, int position)
        {
            WorkflowVersion = workflowVersion;
            WorkflowPersistence = workflowPersistence;
            WorkflowCapability = workflowCapability;
            AsyncRequestClient = asyncRequestClient;
            ActivityFormId = activityFormId;
            Position = position;
            FormTitle = formTitle;
            MethodHandler = new MethodHandler(formTitle);
        }
    }

    internal class ActivityFlow : ActivityFlowBase, IActivityFlow
    {

        public ActivityFlow(int position, WorkflowVersionBase workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId) 
        :base(workflowVersion, workflowCapability, asyncRequestClient,workflowPersistence, formTitle, activityFormId, position)
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
        public ActivityAction Action()
        {
            return new ActivityAction(this);
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue LoopUntil()
        {
            return new ActivityLoopUntilTrue(this);
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TItem>(this, items);
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachSequential<TItem>(this, items);
        }
    }

    internal class ActivityFlow<TActivityReturns> : ActivityFlowBase, IActivityFlow<TActivityReturns>
    {
        public Func<CancellationToken, Task<TActivityReturns>> GetDefaultValueMethodAsync { get; private set; }

        public ActivityFlow(WorkflowVersionBase workflowVersion, IWorkflowCapability workflowCapability,
            IAsyncRequestClient asyncRequestClient,
            WorkflowPersistence workflowPersistence, string formTitle, string activityFormId, int position) 
        :base(workflowVersion, workflowCapability, asyncRequestClient,workflowPersistence, formTitle, activityFormId, position)
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
        public ActivityAction<TActivityReturns> Action()
        {
            return new ActivityAction<TActivityReturns>(this, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public ActivityLoopUntilTrue<TActivityReturns> LoopUntil()
        {
            return new ActivityLoopUntilTrue<TActivityReturns>(this, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem, TKey> ForEachParallel<TItem, TKey>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TActivityReturns, TItem, TKey>(this, items, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public ActivityForEachParallel<TActivityReturns, TItem, TItem> ForEachParallel<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachParallel<TActivityReturns, TItem, TItem>(this, items, GetDefaultValueMethodAsync);
        }
        
        /// <inheritdoc/>
        public ActivityForEachSequential<TActivityReturns, TItem> ForEachSequential<TItem>(IEnumerable<TItem> items)
        {
            return new ActivityForEachSequential<TActivityReturns, TItem>(this, items, GetDefaultValueMethodAsync);
        }
    }
}