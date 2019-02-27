namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    public interface IContextInfo
    {
        bool UseExclamationSyntax { get; set; }
        string ConceptName { get; set; }
        string ContextName { get; set; }
        string ClientName { get; set; }
        bool IsMissingContextName { get; }
        IContextInfo Clone();
        string ToString(bool ignoreClientName);
    }
}