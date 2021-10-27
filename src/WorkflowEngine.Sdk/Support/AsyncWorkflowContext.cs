﻿using System;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support
{
    public class AsyncWorkflowContext
    {
        public IContextValueProvider ValueProvider { get; }

        private readonly OneValueProvider<bool> _executionIsAsynchronous;
        private readonly OneValueProvider<string> _workflowInstanceId;
        private readonly OneValueProvider<string> _parentActivityInstanceId;
        private readonly OneValueProvider<string> _latestActivityInstanceId;

        public AsyncWorkflowContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _executionIsAsynchronous = new OneValueProvider<bool>(ValueProvider, "ExecutionIsAsynchronous");
            _workflowInstanceId = new OneValueProvider<string>(ValueProvider, "WorkflowInstanceId");
            _parentActivityInstanceId = new OneValueProvider<string>(ValueProvider, "ParentActivityInstanceId");
            _latestActivityInstanceId = new OneValueProvider<string>(ValueProvider, "LatestActivityInstanceId");
        }

        /// <summary>
        /// If this is true, then the current execution is in an truly asynchronous context,
        /// i.e. the client is not waiting for the response, so we are for instance
        /// free to make asynchronous calls to other servers.
        /// </summary>
        public bool ExecutionIsAsynchronous
        {
            get => _executionIsAsynchronous.GetValue();
            set => _executionIsAsynchronous.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains the information about the current execution id.
        /// </summary>
        public string WorkflowInstanceId
        {
            get => _workflowInstanceId.GetValue();
            set => _workflowInstanceId.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains information about the current parent id.
        /// </summary>
        public string ParentActivityInstanceId
        {
            get => _parentActivityInstanceId.GetValue();
            set => _parentActivityInstanceId.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains information about the current parent id.
        /// </summary>
        public string LatestActivityInstanceId
        {
            get => _latestActivityInstanceId.GetValue();
            set => _latestActivityInstanceId.SetValue(value);
        }
    }
}