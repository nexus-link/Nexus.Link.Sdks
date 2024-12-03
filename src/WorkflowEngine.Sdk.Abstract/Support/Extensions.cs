using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;

internal static class Extensions
{
    /// <summary>
    /// Represent an exception with type and message, suitable for logging.
    /// </summary>
    public static string ToLog(this Exception exception) => $"{exception.GetType().Name}: {exception.Message}";
}