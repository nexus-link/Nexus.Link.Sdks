using System;
using System.Text.RegularExpressions;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.KeyTranslator.Sdk
{
    /// <summary>
    /// A class for representing a concept value
    /// </summary>
    public class ConceptValue : IConceptValue
    {
        private const string ExclamationMarkSyntaxPattern = @"^\(([^!)]*)!([^!)]*)!(.*)\)$";
        private string _clientName;
        private string _contextName;

        /// <summary>
        /// Try to parse a path into a concept value.
        /// </summary>
        /// <param name="path">The path to parse</param>
        /// <param name="conceptValue">The result, or null if we failed.</param>
        /// <returns>True if the paring was successful.</returns>
        public static bool TryParse(string path, out ConceptValue conceptValue)
        {
            if (!IsPath(path))
            {
                conceptValue = null;
                return false;
            }
            conceptValue = Parse(path);
            return true;
        }

        /// <summary>
        /// Parse a <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to parse</param>
        /// <returns>The result.</returns>
        public static ConceptValue Parse(string path)
        {
            var match = Regex.Match(path, ExclamationMarkSyntaxPattern);
            InternalContract.Require(IsPath(path), $"{nameof(path)} ({path})  must have the pattern \"({{concept}}!{{context}}!{{value}})\".");
            FulcrumAssert.IsTrue(match.Groups.Count >= 4);
            ExtractContextNameAndClientName(match.Groups[2].Value, out var contextName, out var clientName);
            var conceptKey = new ConceptValue
            {
                ConceptName = match.Groups[1].Value,
                ContextName = contextName,
                ClientName = clientName,
                Value = match.Groups[3].Value
            };
            return conceptKey;

        }

        /// <summary>
        /// Verfifies if a <paramref name="path"/> has the correct pattern.
        /// </summary>
        /// <param name="path">The path to verify</param>
        /// <returns>true if <paramref name="path"/> has the correct pattern</returns>
        public static bool IsPath(string path)
        {
            return Regex.IsMatch(path, ExclamationMarkSyntaxPattern);
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ConceptValue()
        {
        }

        /// <summary>
        /// Construct a new concept value from the parts. If <paramref name="valueOrPath"/> is a path, it will be entirely constructed based on that. 
        /// If not, it will see it as a value and use it with the other parameters to construct the concept value. 
        /// </summary>
        /// <param name="conceptName">The name of the concept.</param>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="valueOrPath">A value or a path.</param>
        [Obsolete("Please use one of the other constructors, this will eventually be removed.")]
        public ConceptValue(string conceptName, string clientName, string valueOrPath)
        {
            if (!TryParse(valueOrPath, out var conceptKey))
            {
                ConceptName = conceptName;
                ClientName = clientName;
                Value = valueOrPath;
            }
            else
            {
                ConceptName = conceptKey.ConceptName;
                ContextName = conceptKey.ContextName;
                ClientName = conceptKey.ClientName;
                Value = conceptKey.Value;
            }
        }

        /// <summary>
        /// Construct a new concept value from the parts.
        /// </summary>
        /// <param name="conceptName">The name of the concept.</param>
        /// <param name="contextName">The name of the context.</param>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="value">The value.</param>
        /// <remarks>One of <paramref name="contextName"/> and <paramref name="clientName"/> must be null.</remarks>
        public ConceptValue(string conceptName, string contextName, string clientName, string value)
        {
            InternalContract.RequireNotNullOrWhiteSpace(conceptName, nameof(conceptName));
            if (string.IsNullOrWhiteSpace(clientName))
            {
                InternalContract.RequireNotNullOrWhiteSpace(contextName, nameof(contextName));
            }
            else
            {
                InternalContract.Require(contextName == null, $"One of {nameof(contextName)} ({conceptName}) and {nameof(clientName)} ({clientName}) must be null.");
            }
            ConceptName = conceptName;
            ContextName = contextName;
            ClientName = clientName;
            Value = value;
        }

        /// <inheritdoc />
        public string ToPath()
        {
            return $"({ConceptName}!{ContextOrClientForPath()}!{Value})";
        }

        /// <inheritdoc />
        public string ConceptName { get; set; }

        /// <inheritdoc />
        public string ClientName
        {
            get => _clientName;
            set
            {
                if (_clientName == null && value != null) ContextName = null;
                _clientName = value;
            }
        }

        /// <inheritdoc />
        public string ContextName
        {
            get => _contextName;
            set
            {
                if (_contextName == null && value != null) ClientName = null;
                _contextName = value;
            }
        }

        /// <inheritdoc />
        public string Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({ConceptName}!{ContextOrClientForPath()}!{Value})";
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(ConceptName, nameof(ConceptName), errorLocation);
            if (string.IsNullOrWhiteSpace(ClientName)) FulcrumValidate.IsNotNullOrWhiteSpace(ContextName, nameof(ContextName), errorLocation);
        }

        private static void ExtractContextNameAndClientName(string contextNameOrClientName, out string contextName, out string clientName)
        {
            if (contextNameOrClientName.StartsWith("~"))
            {
                contextName = null;
                clientName = contextNameOrClientName.Substring(1);
            }
            else
            {
                contextName = contextNameOrClientName;
                clientName = null;
            }
        }

        private string ContextOrClientForPath()
        {
            return ContextName ?? $"~{ClientName}";
        }
    }
}
