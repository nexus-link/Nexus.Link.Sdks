﻿using System;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support
{
    /// <summary>
    /// Help class to setup your application
    /// </summary>
    internal static class WorkflowStatic
    {
        /// <summary>
        /// The context value provider that will be used all over the application.
        /// </summary>
        public static WorkflowContext Context{ get; } = new WorkflowContext(new AsyncLocalContextValueProvider());

        public static JToken SafeConvertToJToken(object data)
        {
            JToken jToken = null;
            if (data != null)
            {
                if (data is Exception exception)
                {
                    return SafeConvertExceptionToJToken(exception);
                }
                try
                {
                    jToken = JToken.FromObject(data);
                }
                catch (Exception e)
                {
                    var errorMessage = $"LOG ERROR: Can't serialize type {data.GetType().Name}: {e}";
                    jToken = JToken.FromObject(errorMessage);
                }
            }

            return jToken;
        }

        private static JToken SafeConvertExceptionToJToken(Exception exception)
        {
            // TODO: Make a real JToken representation
            var stringRepresentation = $"{exception}";
            return JToken.FromObject(stringRepresentation);
        }

    }
}
