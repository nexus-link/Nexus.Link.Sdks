using System;
using System.Text.RegularExpressions;

namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    public class InstanceInfo : ContextInfo, IInstanceInfo
    {
        private const string ExclamationMarkSyntaxPattern = @"^\(([^!)]*)!([^!)]*)!(.*)\)$";
        private const string SlashSyntaxPattern = @"^([^/]*)/([^/]*)/(.*)$";

        public InstanceInfo(IContextInfo contextInfo, string value)
        {
            ConceptName = contextInfo.ConceptName;
            ContextName = contextInfo.ContextName;
            ClientName = contextInfo.ClientName;
            Value = value;
            UseExclamationSyntax = contextInfo.UseExclamationSyntax;
        }

        public InstanceInfo()
        {
        }

        public string Value { get; set; }
        public new IInstanceInfo Clone()
        {
            return new InstanceInfo(this, Value);
        }

        public static bool IsInstanceInfo(string path)
        {
            if (path == null) return false;
            return Regex.IsMatch(path, ExclamationMarkSyntaxPattern)
                || Regex.IsMatch(path, SlashSyntaxPattern);
        }

        public new static InstanceInfo Parse(string path)
        {
            if (path == null) return null;
            if (Regex.IsMatch(path, ExclamationMarkSyntaxPattern)) return ParseExlamationMarkSyntax(path);
            if (Regex.IsMatch(path, SlashSyntaxPattern)) return ParseSlashSyntax(path);
            throw new ArgumentException($"Could not parse \"{path}\" into an instance path.");
        }

        private static InstanceInfo ParseExlamationMarkSyntax(string path)
        {
            var match = Regex.Match(path, ExclamationMarkSyntaxPattern);
            if (match.Groups.Count < 4) throw new ArgumentException($"Could not parse \"{path}\" into a instance path with pattern \"({{concept}}!{{context}}!{{value}})\".");
            ExtractContextNameAndClientName(match.Groups[2].Value, out var contextName, out var clientName);
            var instancePath = new InstanceInfo
            {
                UseExclamationSyntax = true,
                ConceptName = match.Groups[1].Value,
                ContextName = contextName,
                ClientName = clientName,
                Value = match.Groups[3].Value
            };
            return instancePath;
        }

        private static InstanceInfo ParseSlashSyntax(string path)
        {
            var match = Regex.Match(path, SlashSyntaxPattern);
            if (match.Groups.Count < 4) throw new ArgumentException($"Could not parse \"{path}\" into a instance path with pattern \"{{concept}}/{{context}}/{{value}}\".");
            ExtractContextNameAndClientName(match.Groups[2].Value, out var contextName, out var clientName);
            var instancePath = new InstanceInfo
            {
                UseExclamationSyntax = false,
                ConceptName = match.Groups[1].Value,
                ContextName = contextName,
                ClientName = clientName,
                Value = match.Groups[3].Value
            };
            return instancePath;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public override string ToString(bool ignoreClientName)
        {
            var name = IsMissingContextName 
                ? ignoreClientName ? "" : PrefixedClientName 
                : ContextName;
            return UseExclamationSyntax
                ? $"({ConceptName}!{name}!{Value})"
                : $"{ConceptName}/{name}/{Value}";
        }

        public static InstanceInfo ToInstanceInfo(IInstanceInfo instanceInfoAsInterface)
        {
            if (instanceInfoAsInterface == null) return null;
            if (!(instanceInfoAsInterface is InstanceInfo instanceInfoAsClass))
            {
                throw new ApplicationException($"Unexpected data type for instance info \"{instanceInfoAsInterface}\".");
            }
            return instanceInfoAsClass;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return obj is InstanceInfo contextInfo && Equals(contextInfo);
        }

        protected bool Equals(InstanceInfo other)
        {
            return base.Equals(other) && string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Value?.GetHashCode() ?? 0);
            }
        }
    }
}
