using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Serilog;
using WorkflowEngine.Sdk.UnitTests.Internal.Activities;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;
using Xunit.Abstractions;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

// ReSharper disable MethodSupportsCancellation

#pragma warning disable CS0618

namespace WorkflowEngine.Sdk.UnitTests.Internal.Support;

public class LogTests
{
    public LogTests(ITestOutputHelper testOutputHelper)
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(ActivitySwitchTests));
    }

    [Fact]
    public async Task Log_1_GivenVanilla_Given_Logged()
    {
        // Arrange
        SetupLogger(nameof(Log_1_GivenVanilla_Given_Logged));

        // Act
        Log.LogInformation("Vanilla log");
        await Task.Delay(TimeSpan.FromMilliseconds(300));
    }

    [Fact]
    public Task Log_2_Given_LogAfterOperationCancelled_Given_StillLogged()
    {
        // Arrange
        SetupLogger(nameof(Log_2_Given_LogAfterOperationCancelled_Given_StillLogged));
        var cancellationTokenSource = new CancellationTokenSource();
        Log.LogInformation("Before cancellation");
        cancellationTokenSource.Cancel();

        // Act
        var task = LogInBackgroundAsync(cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Log_3_WaitForSeconds()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(300));
    }

    private async Task LogInBackgroundAsync(CancellationToken cancellationToken)
    {
        Log.LogInformation("Background 1");
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Log.LogInformation("Background 2");
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Log.LogInformation("Background 3");
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
        catch (Exception ex)
        {
            Log.LogError(ex.Message, ex);
            throw;
        }
    }

    private void SetupLogger(string name)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{name}.txt");
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath); // Ensure the file is clean before the test
        }
        var logger = new LoggerConfiguration()
            .WriteTo.File(tempFilePath)
            .CreateLogger();
        FulcrumApplication.Setup.SynchronousFastLogger = new SerilogLogger(logger);
    }
}