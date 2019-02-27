namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    /// <summary>
    /// Information about a single translate request
    /// </summary>
    public class TranslateResponse
    {
        /// <summary>
        /// The request that was the basis for the translation.
        /// </summary>
        public TranslateRequest Request { get; set; }

        /// <summary>
        /// The translated value
        /// </summary>
        public string Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Request}: {Value}";
        }

        /// <summary>
        /// Converts between two representations of a translate request
        /// </summary>
        public static TranslateResponse FromFacade(RestClients.Facade.Models.TranslateResponse source)
        {
            if (source == null) return null;
            var target = new TranslateResponse
            {
                Request = TranslateRequest.FromFacade(source.Request),
                Value = source.Value
            };
            return target;
        }
    }
}
