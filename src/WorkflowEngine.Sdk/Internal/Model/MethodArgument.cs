namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Model
{
    internal class MethodArgument
    {
        public MethodArgument(MethodParameter parameter, object value)
        {
            Parameter = parameter;
            Value = value;
        }

        public MethodParameter Parameter { get; }
        public object Value { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Parameter} {Value}";
    }
}