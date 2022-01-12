using Newtonsoft.Json;
using Nexus.Link.Libraries.Azure.Storage.Queue;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Logger.Sdk.Helpers
{
    public class TruncatingAzureStorageQueue<T> : IWritableQueue<T>
    {
        private readonly IWritableQueue<T> _baseQueue;

        public TruncatingAzureStorageQueue(string connectionString, string name)
        {
            // TODO: If we are going to move to V12, then we probably need to use the constructor with queue options and set Base64 encoding.
            // https://stackoverflow.com/questions/63023481/azure-functions-queue-trigger-is-expecting-base-64-messages-and-doesnt-process
            _baseQueue = new AzureStorageQueue<T>(connectionString, name);
        }

        public TruncatingAzureStorageQueue(IWritableQueue<T> baseQueue)
        {
            _baseQueue = baseQueue;
        }

        /// <summary>
        /// Theoretical maximum string length is 65,536 characters
        /// .NET Storage API uses base64 encoding for string and binary payloads as a default, so the
        /// overhead of the necessary base64 encoding (+ 33 %) leads to a maximum useful payload of just 49,152 bytes (48 KB)
        /// </summary>
        public Task AddMessageAsync(T message, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(message, nameof(message));

            if (message is LogMessage logMessage)
            {
                const int maxLength = 48000; // 65536;

                // Threshold reached?
                if (Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(logMessage)) > maxLength)
                {
                    LogHelper.FallbackSafeLog(LogSeverityLevel.Critical,
                        "Azure Storage Queue messages cannot be larger than 65536 bytes, so the text message part in the logmessage was truncated.");

                    // SafeLog the full original message prior to truncating
                    LogHelper.FallbackSafeLog(LogSeverityLevel.Warning, JsonConvert.SerializeObject(logMessage));

                    // Truncate to accommodate Azure Storage Queue object maxsize
                    TruncateLogMessageToMaxCharacters(logMessage, maxLength);
                }
            }

            return _baseQueue.AddMessageAsync(message, timeSpanToWait, cancellationToken);
        }

        /// <summary>
        /// Truncate LogMessage.Message attribute to cap total length of serialized object to maxNumberOfBytes
        /// </summary>
        /// <param name="logMessage">The logdocument</param>
        /// <param name="maxNumberOfBytes">capped size of serialized logdocument</param>
        /// <returns></returns>
        private static bool TruncateLogMessageToMaxCharacters(LogMessage logMessage, int maxNumberOfBytes)
        {
            var isTruncated = false;
            var processDepth = 0;

            while (true && processDepth++ < 10)
            {
                var byteCount = Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(logMessage));
                if (byteCount <= maxNumberOfBytes)
                {
                    if (isTruncated)
                        return true;
                }

                if (logMessage.Message == null) return false;
                var currentMessageByteCount = Encoding.UTF8.GetByteCount(logMessage.Message);
                var newMessageByteCount = currentMessageByteCount - byteCount + maxNumberOfBytes;
                if (newMessageByteCount < 0) return false;
                if (newMessageByteCount >= currentMessageByteCount) return false;

                logMessage.Message = LimitByteLength(logMessage.Message, newMessageByteCount);
                isTruncated = true;
            }

            return isTruncated;
        }

        /// <summary>
        /// Answer by canton7 in https://stackoverflow.com/questions/1225052/best-way-to-shorten-utf8-string-based-on-byte-length
        /// </summary>
        private static string LimitByteLength(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || Encoding.UTF8.GetByteCount(input) <= maxLength)
            {
                return input;
            }

            var encoder = Encoding.UTF8.GetEncoder();
            var buffer = new byte[maxLength];
            var messageChars = input.ToCharArray();
            encoder.Convert(
                messageChars,
                0,
                messageChars.Length,
                buffer,
                0,
                buffer.Length,
                false,
                out _,
                out var bytesUsed,
                out _);

            // I don't think we can return message.Substring(0, charsUsed)
            // as that's the number of UTF-16 chars, not the number of codepoints
            // (think about surrogate pairs). Therefore I think we need to
            // actually convert bytes back into a new string
            return Encoding.UTF8.GetString(buffer, 0, bytesUsed);
        }

        public Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return _baseQueue.GetResourceHealthAsync(tenant, cancellationToken);
        }

        public string Name => _baseQueue.Name;
    }
}