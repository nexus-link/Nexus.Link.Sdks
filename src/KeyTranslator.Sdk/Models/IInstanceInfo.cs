namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    public interface IInstanceInfo : IContextInfo
    {
        string Value { get; set; }
        new IInstanceInfo Clone();
    }
}