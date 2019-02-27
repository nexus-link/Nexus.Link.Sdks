namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    /// <summary>
    /// Information about a single translate request
    /// </summary>
    public class TranslateRequest
    {
        /// <summary>
        /// The instance path to the value that should be translated
        /// </summary>
        public string SourceInstancePath { get; set; }

        /// <summary>
        /// The context path that is the target of this request
        /// </summary>
        public string TargetContextPath { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{SourceInstancePath}->{TargetContextPath}";
        }

        /// <summary>
        /// Converts between two representations of a translate request
        /// </summary>
        public static RestClients.Facade.Models.TranslateRequest ToFacade(TranslateRequest source)
        {
            if (source == null) return null;
            var target = new RestClients.Facade.Models.TranslateRequest
            {
                SourceInstancePath = source.SourceInstancePath,
                TargetContextPath = source.TargetContextPath
            };
            return target;
        }

        /// <summary>
        /// Converts between two representations of a translate request
        /// </summary>
        public static TranslateRequest FromFacade(RestClients.Facade.Models.TranslateRequest source)
        {
            if (source == null) return null;
            var target = new TranslateRequest
            {
                SourceInstancePath = source.SourceInstancePath,
                TargetContextPath = source.TargetContextPath
            };
            return target;
        }
    }
}
