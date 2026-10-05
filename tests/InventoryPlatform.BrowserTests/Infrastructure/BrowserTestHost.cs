using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace InventoryPlatform.BrowserTests.Infrastructure;

public sealed class BrowserTestHost : IAsyncDisposable
{
    private static readonly TimeSpan ReadinessTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ReadinessRequestTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ReadinessRetryDelay = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan GracefulShutdownTimeout = TimeSpan.FromSeconds(10);

    private readonly object _outputLock = new();
    private readonly StringBuilder _standardOutput = new();
    private readonly StringBuilder _standardError = new();
    private readonly HttpClient _httpClient = new(
        new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = ReadinessRequestTimeout
    };

    private Process? _process;
    private int _disposed;

    private BrowserTestHost(string repositoryRoot, string baseUrl)
    {
        RepositoryRoot = repositoryRoot;
        BaseUrl = baseUrl;
    }

    public string RepositoryRoot { get; }

    public string BaseUrl { get; }

    public string ReadinessUrl => $"{BaseUrl}/Identity/Account/Login";

    public string CapturedStandardOutput
    {
        get
        {
            lock (_outputLock)
            {
                return _standardOutput.ToString();
            }
        }
    }

    public string CapturedStandardError
    {
        get
        {
            lock (_outputLock)
            {
                return _standardError.ToString();
            }
        }
    }

    public static async Task<BrowserTestHost> StartAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var repositoryRoot = FindRepositoryRoot();
        var baseUrl = $"http://127.0.0.1:{GetAvailablePort()}";
        var host = new BrowserTestHost(repositoryRoot, baseUrl);

        try
        {
            host.StartProcess(connectionString);
            await host.WaitUntilReadyAsync(cancellationToken);
            return host;
        }
        catch (Exception startupException)
        {
            try
            {
                await host.DisposeAsync();
            }
            catch (Exception shutdownException)
            {
                throw new AggregateException(
                    "The Web child process failed to start and cleanup also failed.",
                    startupException,
                    shutdownException);
            }

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        Exception? shutdownException = null;

        try
        {
            await StopProcessAsync();
        }
        catch (Exception exception)
        {
            shutdownException = exception;
        }
        finally
        {
            _httpClient.Dispose();
            _process?.Dispose();
        }

        if (shutdownException is not null)
        {
            throw new InvalidOperationException(
                "The Web child process could not be shut down cleanly.",
                shutdownException);
        }
    }

    private void StartProcess(string connectionString)
    {
        var webProjectPath = Path.Combine(
            RepositoryRoot,
            "src",
            "InventoryPlatform",
            "InventoryPlatform.Web",
            "InventoryPlatform.Web.csproj");

        if (!File.Exists(webProjectPath))
        {
            throw new FileNotFoundException(
                "The InventoryPlatform Web project was not found.",
                webProjectPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Path.GetDirectoryName(webProjectPath)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add(GetBuildConfiguration());
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(webProjectPath);

        startInfo.Environment["ASPNETCORE_URLS"] = BaseUrl;
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        startInfo.Environment["ConnectionStrings__DefaultConnection"] = connectionString;
        startInfo.Environment.Remove("ASPNETCORE_HTTPS_PORT");
        startInfo.Environment.Remove("ASPNETCORE_HTTPS_PORTS");

        _process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
        _process.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                AppendOutput(_standardOutput, eventArgs.Data);
            }
        };
        _process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                AppendOutput(_standardError, eventArgs.Data);
            }
        };

        if (!_process.Start())
        {
            throw new InvalidOperationException(
                "Starting the InventoryPlatform Web child process returned false.");
        }

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    private async Task WaitUntilReadyAsync(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        HttpStatusCode? lastStatusCode = null;

        while (timer.Elapsed < ReadinessTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_process?.HasExited == true)
            {
                throw CreateStartupException(
                    $"The Web child process exited with code {_process.ExitCode} " +
                    "before its readiness endpoint responded.");
            }

            try
            {
                using var response = await _httpClient.GetAsync(
                    ReadinessUrl,
                    cancellationToken);

                lastStatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }

            await Task.Delay(ReadinessRetryDelay, cancellationToken);
        }

        var statusDescription = lastStatusCode is null
            ? "no HTTP response was received"
            : $"the last HTTP status was {(int)lastStatusCode} ({lastStatusCode})";
        throw CreateStartupException(
            $"The Web readiness endpoint did not succeed within " +
            $"{ReadinessTimeout.TotalSeconds:0} seconds; {statusDescription}.");
    }

    private InvalidOperationException CreateStartupException(string message) =>
        new(
            $"{message}{Environment.NewLine}" +
            $"BaseUrl: {BaseUrl}{Environment.NewLine}" +
            $"stdout:{Environment.NewLine}{CapturedStandardOutput}{Environment.NewLine}" +
            $"stderr:{Environment.NewLine}{CapturedStandardError}");

    private async Task StopProcessAsync()
    {
        if (_process is null || _process.HasExited)
        {
            return;
        }

        _process.CloseMainWindow();
        var exitTask = _process.WaitForExitAsync();
        var completedTask = await Task.WhenAny(
            exitTask,
            Task.Delay(GracefulShutdownTimeout));

        if (completedTask != exitTask && !_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }

        await _process.WaitForExitAsync();
    }

    private void AppendOutput(StringBuilder target, string line)
    {
        lock (_outputLock)
        {
            target.AppendLine(line);
        }
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string GetBuildConfiguration()
    {
        var outputDirectory = new DirectoryInfo(
            Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));
        var configuration = outputDirectory.Parent?.Name;

        if (configuration is not ("Debug" or "Release"))
        {
            throw new InvalidOperationException(
                $"Could not determine the test build configuration from " +
                $"'{AppContext.BaseDirectory}'.");
        }

        return configuration;
    }

    internal static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionPath = Path.Combine(
                directory.FullName,
                "src",
                "InventoryPlatform",
                "InventoryPlatform.slnx");

            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find the repository root containing " +
            "'src/InventoryPlatform/InventoryPlatform.slnx'.");
    }
}
