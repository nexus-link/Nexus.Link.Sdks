using Nexus.Link.Libraries.Core.Decoupling;
using Nexus.Link.Logger.Sdk.MessageModel;

namespace Nexus.Link.Logger.Sdk.Helpers
{
    public static class MessageParser
    {
        private static readonly SchemaParser Parser;

        static MessageParser()
        {
            Parser = new SchemaParser()
                .Add(typeof(LogMessage))
                .Add("LogMessage", 1, typeof(LogMessageV1));
        }

        /// <summary>
        /// Try to convert a JSON string into an log message type.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <param name="obj">The converted object.</param>
        /// <returns>True if the conversion was successful.</returns>
        public static bool TryParse(string json, out object obj)
        {
            return Parser.TryParse(json, out obj);
        }

        /// <summary>
        ///  to convert a JSON string into an log message type.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <param name="schemaName">The schema name that was used for the conversion.</param>
        /// <param name="schemaVersion">The schema version that was used for the conversion.</param>
        /// <param name="obj">The converted object.</param>
        /// <returns>True if the conversion was successful.</returns>
        public static bool TryParse(string json, out string schemaName, out int schemaVersion, out object obj)
        {
            return Parser.TryParse(json, out schemaName, out  schemaVersion, out obj);
        }
    }
}
