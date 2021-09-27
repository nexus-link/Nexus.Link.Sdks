namespace WorkflowEngine.Sdk.MethodSupport
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
    }
}