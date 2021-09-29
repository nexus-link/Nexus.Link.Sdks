using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.MethodSupport
{
    public class MethodHandler
    {

        private Dictionary<int, MethodParameter> _parameters = new Dictionary<int, MethodParameter>();
        private readonly Dictionary<string, MethodArgument> _arguments = new Dictionary<string, MethodArgument>();

        public string FormTitle { get; }
        public string InstanceTitle { get; set; }

        public MethodHandler(string formTitle)
        {
            FormTitle = formTitle;
        }

        public MethodHandler NewInstance()
        {
            var instance = new MethodHandler(FormTitle)
            {
                _parameters = _parameters
            };
            return instance;
        }

        private void DefineParameter(string name, System.Type type)
        {
            var position = _parameters.Count + 1;
            _parameters.Add(position, new MethodParameter(name, type));
        }

        public void DefineParameter<TParameter>(string name)
        {
            DefineParameter(name, typeof(TParameter));
        }

        public object GetArgument(string parameterName)
        {
            if (!_arguments.TryGetValue(parameterName, out var argument))
            {
                var argumentParameters = string.Join(", ", _arguments.Values.Select(a => a.Parameter.Name));
                InternalContract.Fail($"{FormTitle} has a parameter named {parameterName}, but {InstanceTitle} had no argument for that parameter. Found these: {argumentParameters}");
                return default;
            }

            if(argument.Value != null)
            {
                FulcrumAssert.IsTrue(argument.Parameter.Type.IsInstanceOfType(argument.Value), CodeLocation.AsString());
            }
                

            return argument.Value;
        }

        public TArgument GetArgument<TArgument>(string parameterName)
        {
            return (TArgument)GetArgument(parameterName);
        }

        private MethodParameter GetMethodParameter(string parameterName)
        {
            var parameter = _parameters.Values.FirstOrDefault(p => p.Name == parameterName);
            InternalContract.RequireNotNull(parameter, nameof(parameterName),
                $"No parameter named {parameterName}. These are registered: {string.Join(", ", _parameters.Keys)}");
            return parameter;
        }

        public void SetParameter(string parameterName, object value)
        {
            var parameter = GetMethodParameter(parameterName);
            if (value == null)
            {
                InternalContract.Require(parameter!.IsNullable,
                    $"The parameter {parameter} does not accept the value null.");
            }
            else
            {
                InternalContract.Require(parameter!.Type.IsInstanceOfType(value),
                    $"Expected {nameof(value)} to be an instance of type {parameter.Type.FullName}, but was of type {value.GetType().FullName}.");
            }

            var argument = new MethodArgument(parameter, value);
            _arguments.Add(parameter.Name, argument);
        }

        public async Task PersistWorkflowParametersAsync(IWorkflowCapability capability, string workflowFormId, CancellationToken cancellationToken)
        {
            foreach (var methodParameter in _parameters)
            {
                var parameterName = methodParameter.Value.Name;
                FulcrumAssert.IsNotNullOrWhiteSpace(parameterName, CodeLocation.AsString());
                var parameter = await capability.WorkflowParameter.ReadAsync(workflowFormId, parameterName, cancellationToken);
                if (parameter != null) continue;
                var parameterCreate = new WorkflowParameterCreate
                {
                    WorkflowFormId = workflowFormId,
                    Name = parameterName
                };
                await capability.WorkflowParameter.CreateWithSpecifiedIdAsync(workflowFormId, parameterName, parameterCreate, cancellationToken);
            }
            var parameters = await StorageHelper.ReadPagesAsync(
                (o, t) => capability.WorkflowParameter.ReadChildrenWithPagingAsync(workflowFormId, o, null, t), Int32.MaxValue, cancellationToken);
            if (parameters.Count() > _parameters.Keys.Count)
            {
                throw new FulcrumNotImplementedException($"Can't currently remove parameters for a version.");
            }
        }

        public async Task PersistActivityParametersAsync(IWorkflowCapability capability, string workflowFormId, CancellationToken cancellationToken)
        {
            foreach (var methodParameter in _parameters)
            {
                var parameterName = methodParameter.Value.Name;
                FulcrumAssert.IsNotNullOrWhiteSpace(parameterName, CodeLocation.AsString());
                var parameter = await capability.ActivityParameter.ReadAsync(workflowFormId, parameterName, cancellationToken);
                if (parameter != null) continue;
                var parameterCreate = new ActivityParameterCreate
                {
                    ActivityFormId = workflowFormId,
                    Name = parameterName
                };
                await capability.ActivityParameter.CreateWithSpecifiedIdAsync(workflowFormId, parameterName, parameterCreate, cancellationToken);
            }
            var parameters = await StorageHelper.ReadPagesAsync(
                (o, t) => capability.ActivityParameter.ReadChildrenWithPagingAsync(workflowFormId, o, null, t), Int32.MaxValue, cancellationToken);
            if (parameters.Count() > _parameters.Keys.Count)
            {
                throw new FulcrumNotImplementedException($"Can't currently remove parameters for a version.");
            }
        }
    }
}