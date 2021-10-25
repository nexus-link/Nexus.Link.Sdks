using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;
using Activity = System.Diagnostics.Activity;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class Performance
    {
        private readonly string _className;
        private readonly string _context;
        private readonly Stopwatch _stopwatch;
        private string _description;

        public Performance(string className, string context)
        {
            _className = className;
            _context = context;
            _stopwatch = new Stopwatch();
        }


        public void Measure(Func<Task> func)
        {
            throw new NotImplementedException();
        }

        public async Task<T> MeasureMethodAsync<T>(Func<Task<T>> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _stopwatch.Start();
            _description = $"Performance {_className}.{memberName} {_context}";
            return await DoAndLogAsync(func, lineNumber);
        }

        private async Task<T> DoAndLogAsync<T>(Func<Task<T>> func, int lineNumber)
        {
            var stopWatch = new Stopwatch();
            try
            {
                Log.LogVerbose($"{_description} ({lineNumber}):  {Math.Round(_stopwatch.Elapsed.TotalSeconds * 10) / 10.0} s. Start");
                return await func();
            }
            finally
            {
                Log.LogVerbose($"{_description} ({lineNumber}):  {Math.Round(_stopwatch.Elapsed.TotalSeconds * 10) / 10.0} s. Stop {Math.Round(stopWatch.Elapsed.TotalSeconds * 10) / 10.0}");
            }
        }

        public async Task MeasureAsync(Func<Task> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            await DoAndLogAsync(async () =>
            {
                await func();
                return Task.FromResult(true);
            }, lineNumber);
        }

        public Task<T> MeasureAsync<T>(Func<Task<T>> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return DoAndLogAsync(func, lineNumber);
        }
    }
}