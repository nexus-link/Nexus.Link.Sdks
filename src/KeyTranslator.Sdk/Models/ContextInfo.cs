using System;
using System.Text.RegularExpressions;

namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    public class ContextInfo : IContextInfo
    {
        private const string ExclamationMarkSyntaxPattern = @"^\(([^!)]*)!([^!)]*)\)$";
        private const string SlashSyntaxPattern = @"^([^/]*)/([^/]*)$";

        public bool UseExclamationSyntax { get; set; }
        public string ConceptName { get; set; }
        public string ContextName { get; set; }
        public string ClientName { get; set; }
        public string PrefixedClientName => $"~{ClientName}";

        public ContextInfo()
        {
        }

        public ContextInfo(IContextInfo instanceInfo)
        {
            ConceptName = instanceInfo.ConceptName;
            ContextName = instanceInfo.ContextName;
            ClientName = instanceInfo.ClientName;
            UseExclamationSyntax = instanceInfo.UseExclamationSyntax;
        }

        public bool IsMissingContextName => string.IsNullOrEmpty(ContextName);

        public virtual IContextInfo Clone()
        {
            return new ContextInfo(this);
        }

        public static ContextInfo Parse(string path)
        {
            if (path == null) return null;
            if (Regex.IsMatch(path, ExclamationMarkSyntaxPattern)) return ParseExlamationMarkSyntax(path);
            if (Regex.IsMatch(path, SlashSyntaxPattern)) return ParseSlashSyntax(path);
            throw new ArgumentException($"Could not parse \"{path}\" into a context path.");
        }

        private static ContextInfo ParseExlamationMarkSyntax(string path)
        {
            var match = Regex.Match(path, ExclamationMarkSyntaxPattern);
            if (match.Groups.Count < 3) throw new ArgumentException($"Could not parse \"{path}\" into a context path with pattern \"({{concept}}!{{context}})\".");
            string contextName, clientName;
            ExtractContextNameAndClientName(match.Groups[2].Value, out contextName, out clientName);
            var contextPath = new ContextInfo
            {
                UseExclamationSyntax = true,
                ConceptName = match.Groups[1].Value,
                ContextName = contextName,
                ClientName = clientName
            };
            return contextPath;
        }

        protected static void ExtractContextNameAndClientName(string contextNameOrClientName, out string contextName, out string clientName)
        {
            contextName = contextNameOrClientName;
            clientName = null;
            if (contextName.StartsWith("~"))
            {
                clientName = contextName.Substring(1);
                contextName = null;
            }
        }

        private static ContextInfo ParseSlashSyntax(string path)
        {
            var match = Regex.Match(path, SlashSyntaxPattern);
            if (match.Groups.Count < 3) throw new ArgumentException($"Could not parse \"{path}\" into a context path with pattern \"{{concept}}/{{context}}\".");
            string contextName, clientName;
            ExtractContextNameAndClientName(match.Groups[2].Value, out contextName, out clientName);
            var contextPath = new ContextInfo
            {
                UseExclamationSyntax = false,
                ConceptName = match.Groups[1].Value,
                ContextName = contextName,
                ClientName = clientName
            };
            return contextPath;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public virtual string ToString(bool ignoreClientName)
        {
            var name = IsMissingContextName 
                ? ignoreClientName ? "" : PrefixedClientName
                : ContextName;
            return UseExclamationSyntax
                ? $"({ConceptName}!{name})"
                : $"{ConceptName}/{name}";
        }

        public static ContextInfo ToContextInfo(IContextInfo contextInfoAsInterface)
        {
            if (contextInfoAsInterface == null) return null;
            var contextInfoAsClass = contextInfoAsInterface as ContextInfo;
            if (contextInfoAsClass == null)
            {
                throw new ApplicationException($"Unexpected data type for context info \"{contextInfoAsInterface}\".");
            }
            return contextInfoAsClass;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            var contextInfo = obj as ContextInfo;
            return contextInfo != null && Equals(contextInfo);
        }

        protected bool Equals(ContextInfo other)
        {
            return string.Equals(ConceptName, other.ConceptName) && string.Equals(ContextName, other.ContextName) && string.Equals(ClientName, other.ClientName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ConceptName?.GetHashCode() ?? 0) * 397) ^ (ContextName?.GetHashCode() ?? 0);
            }
        }
    }
}







