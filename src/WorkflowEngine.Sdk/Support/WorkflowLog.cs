using System.Runtime.CompilerServices;
using Nexus.Link.Libraries.Core.Logging;

// ReSharper disable ExplicitCallerInfoArgument

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class WorkflowLog
    {
        public static void LogWarning(string technicalMessage, string friendlyMessage, object data,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogAtLevel(LogSeverityLevel.Warning, technicalMessage, friendlyMessage, data, memberName, filePath, lineNumber);
        }

        public static void LogAtLevel(LogSeverityLevel level, string technicalMessage, string friendlyMessage, object data, 
            string memberName, string filePath, int lineNumber)
        {
            var workflowInstanceId = WorkflowStatic.Context.WorkflowInstanceId;
            var activity = WorkflowStatic.Context.LatestActivity;
            Log.LogWarning($"{technicalMessage} (Workflow: {workflowInstanceId}, Activity: {activity}", data, null, memberName, filePath, lineNumber);
        }
    }
}