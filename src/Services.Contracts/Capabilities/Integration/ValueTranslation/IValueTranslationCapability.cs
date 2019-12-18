using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation
{
    /// <summary>
    /// Value Translation
    /// </summary>
    public interface IValueTranslationCapability : IControllerInjector
    {
        /// <summary>
        /// Service for translating values
        /// </summary>
        ITranslatorService TranslatorService{ get; }
    }
}
