using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
using Xunit.Abstractions;

namespace UnitTests.Support
{
    public class XUnitFulcrumLogger : ISyncLogger
    {
        private readonly ITestOutputHelper _output;

        public XUnitFulcrumLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public void LogSync(LogRecord logRecord)
        {
            _output.WriteLine($"[{logRecord.SeverityLevel}]" +
                              $" {logRecord.TimeStamp:O}" +
                              $" {logRecord.Message}" +
                              (logRecord.Data != null ? $" {JsonConvert.SerializeObject(logRecord.Data, Formatting.Indented)}" : "") +
                              (logRecord.Exception!= null ? $" [Exception] {logRecord.Exception.Message}" : ""));
        }
    }
}
