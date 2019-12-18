using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation
{
    /// <summary>
    /// Value Translation
    /// </summary>
    public interface IValueTranslationCapability : Libraries.Core.Platform.Services.IControllerInjector
    {
        /// <summary>
        /// Service for translating values
        /// </summary>
        ITranslatorService TranslatorService{ get; }
        
        /// <summary>
        /// Service for associating values with each other.
        /// </summary>
        IValueAssociationsService AssociationService { get; }
    }
}
