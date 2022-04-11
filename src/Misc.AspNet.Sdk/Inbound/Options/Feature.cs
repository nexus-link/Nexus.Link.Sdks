namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Base class for features
    /// </summary>
    public abstract class Feature
    {
        /// <summary>
        /// Set this to true if you want to enable the feature
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}