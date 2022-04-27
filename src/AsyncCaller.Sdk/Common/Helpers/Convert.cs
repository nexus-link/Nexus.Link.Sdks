using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.AsyncCaller.Sdk.Common.Helpers
{
    public static class Convert
    {
        public static byte[] ToByteArray(JToken source)
        {
            return source == null ? null : Encoding.UTF8.GetBytes(source.ToString(Formatting.None));
        }

        public static JToken ToJson(byte[] source)
        {
            if (source == null) return null;
            var s = Encoding.UTF8.GetString(source);
            try
            {
                return JToken.Parse(s);
            }
            catch (Exception e)
            {
                InternalContract.Fail($"Could not parse parameter {nameof(source)} ({s}) into a JToken: {e}");
                throw;
            }
        }
    }
}
