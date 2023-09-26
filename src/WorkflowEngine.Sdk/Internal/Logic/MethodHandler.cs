using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    internal class MethodHandler
    {

        private Dictionary<int, MethodParameter> _parameters = new();
        internal readonly Dictionary<string, MethodArgument> Arguments = new();

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
            if (!Arguments.TryGetValue(parameterName, out var argument))
            {
                throw new FulcrumNotFoundException($"{FormTitle} has a parameter named {parameterName}, but {InstanceTitle} had no argument for that parameter." +
                                                   $" Found these: {string.Join(", ", ArgumentNames())}");
            }

            if (argument.Value != null)
            {
                FulcrumAssert.IsTrue(argument.Parameter.Type.IsInstanceOfType(argument.Value), CodeLocation.AsString());
            }


            return argument.Value;
        }

        private IEnumerable<string> ArgumentNames()
        {
            return Arguments.Values.Select(a => a.Parameter.Name);
        }

        private IEnumerable<string> ParameterNames()
        {
            return _parameters.Values.Select(a => a.Name);
        }

        public TArgument GetArgument<TArgument>(string parameterName)
        {
            return (TArgument)GetArgument(parameterName);
        }

        private MethodParameter GetMethodParameter(string parameterName)
        {
            var parameter = _parameters.Values.FirstOrDefault(p => p.Name == parameterName);
            InternalContract.RequireNotNull(parameter, nameof(parameterName),
                $"No parameter named {parameterName}. These are registered: {string.Join(", ", ParameterNames())}");
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
            Arguments.Add(parameter.Name, argument);
        }
    }
}