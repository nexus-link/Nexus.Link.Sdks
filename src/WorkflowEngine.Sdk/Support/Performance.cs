using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;
using Activity = System.Diagnostics.Activity;
// ReSharper disable ExplicitCallerInfoArgument

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class Performance
    {
        private readonly string _className;
        private readonly string _context;
        private readonly Stopwatch _stopwatch;
        private string _description1;
        private string _description2;
        private static AsyncLocal<int> _indentLevel = new() { Value = 0 };

        public Performance(string className, string context)
        {
            _className = className;
            _context = context;
            _stopwatch = new Stopwatch();
        }

        public async Task<T> MeasureMethodAsync<T>(Func<Task<T>> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _stopwatch.Start();
            _description1 = $"Performance {_className}.{memberName}";
            _description2 = $"{_context}";
            var result = await DoAndLogAsync(func, lineNumber, true);
            return result;
        }

        public async Task MeasureMethodAsync(Func<Task> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            await MeasureMethodAsync(async () =>
            {
                await func();
                return Task.FromResult(true);
            }, memberName, filePath, lineNumber);
        }

        private async Task<T> DoAndLogAsync<T>(Func<Task<T>> func, int lineNumber, bool isMethodBody = false)
        {
            var indentation = new string(' ', _indentLevel.Value);
            var stopWatch = new Stopwatch();
            try
            {
                var start = isMethodBody
                    ? "Enter" 
                    : $"{Math.Round(_stopwatch.Elapsed.TotalSeconds * 10) / 10.0} s. Start";
                Log.LogVerbose($"{indentation}{_description1} ({lineNumber}) {_description2}:  {start}");
                _indentLevel.Value += 2;
                stopWatch.Start();
                return await func();
            }
            finally
            {
                var stop  = isMethodBody
                    ? $"Leave {Math.Round(stopWatch.Elapsed.TotalSeconds * 10) / 10.0}" 
                    : $"{Math.Round(_stopwatch.Elapsed.TotalSeconds * 10) / 10.0} s. Stop {Math.Round(stopWatch.Elapsed.TotalSeconds * 10) / 10.0}";
                Log.LogVerbose($"{indentation}{_description1} ({lineNumber}) {_description2}: {stop}");
                _indentLevel.Value -= 2;
            }
        }

        private T DoAndLog<T>(Func<T> func, int lineNumber, bool isMethodBody = false)
        {
            var indentation = new string(' ', _indentLevel.Value);
            var stopWatch = new Stopwatch();
            try
            {
                var start = isMethodBody
                    ? "Enter" 
                    : $"{Math.Round(_stopwatch.Elapsed.TotalSeconds * 10) / 10.0} s. Start";
                Log.LogVerbose($"{indentation}{_description1} ({lineNumber}) {_description2}:  {start}");
                _indentLevel.Value += 2;
                stopWatch.Start();
                return func();
            }
            finally
            {
                var stop  = isMethodBody
                    ? $"Leave {Math.Round(stopWatch.Elapsed.TotalSeconds * 10) / 10.0}" 
                    : $"{Math.Round(_stopwatch.Elapsed.TotalSeconds * 10) / 10.0} s. Stop {Math.Round(stopWatch.Elapsed.TotalSeconds * 10) / 10.0}";
                Log.LogVerbose($"{indentation}{_description1} ({lineNumber}) {_description2}: {stop}");
                _indentLevel.Value -= 2;
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

        public void Measure(Func<Task> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            DoAndLog( () =>
            {
                func();
                return true;
            }, lineNumber);
        }

        public T Measure<T>(Func<T> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return DoAndLog(func, lineNumber);
        }
    }
}