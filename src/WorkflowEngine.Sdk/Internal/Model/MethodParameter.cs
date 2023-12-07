using Nexus.Link.Libraries.Core.Assert;
using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Model
{
    internal class MethodParameter
    {
        public MethodParameter(string name, Type type)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            Name = name;
            Type = type;
            IsNullable = IsNullableType(type);
        }
        public string Name { get; }
        public Type Type { get; }
        public bool IsNullable { get; }

        // https://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable
        private static bool IsNullableType(Type t)
        {
            if (!t.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(t) != null) return true; // Nullable<T>
            return false; // value-type
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}