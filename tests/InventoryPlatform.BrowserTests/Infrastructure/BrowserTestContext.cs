using System.Text.Json;
using Microsoft.Playwright;

namespace InventoryPlatform.BrowserTests.Infrastructure;

public sealed class BrowserTestContext : IAsyncDisposable
{
    private readonly DatabaseFixture _database;
    private readonly BrowserTestHost _host;
    private readonly PlaywrightFixture _playwright;
    private int _disposed;

    private BrowserTestContext(
        string runId,
        string artifactDirectory,
        DatabaseFixture database,
        BrowserTestHost host,
        PlaywrightFixture playwright)
    {
        RunId = runId;
        ArtifactDirectory = artifactDirectory;
        _database = database;
        _host = host;
        _playwright = playwright;
    }

    public string RunId { get; }

    public string BaseUrl => _host.BaseUrl;

    public string DatabaseName => _database.DatabaseName;

    public string ArtifactDirectory { get; }

    public string StandardOutput => _host.CapturedStandardOutput;

    public string StandardError => _host.CapturedStandardError;

    public static async Task<BrowserTestContext> StartAsync(
        CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid().ToString("N");
        var repositoryRoot = BrowserTestHost.FindRepositoryRoot();
        var artifactDirectory = Path.Combine(
            repositoryRoot,
            "artifacts",
            "browser-tests",
            runId);
        Directory.CreateDirectory(artifactDirectory);

        DatabaseFixture? database = null;
        BrowserTestHost? host = null;
        PlaywrightFixture? playwright = null;

        try
        {
            database = await DatabaseFixture.CreateAsync(runId, cancellationToken);
            host = await BrowserTestHost.StartAsync(
                database.ConnectionString,
                cancellationToken);
            await BrowserSeedData.SeedAsync(database, cancellationToken);
            playwright = await PlaywrightFixture.CreateAsync();

            return new BrowserTestContext(
                runId,
                artifactDirectory,
                database,
                host,
                playwright);
        }
        catch (Exception startupException)
        {
            var cleanupFailures = new List<Exception>();

            await TryCleanupAsync(playwright, cleanupFailures);
            await TryCleanupAsync(host, cleanupFailures);
            await TryCleanupAsync(database, cleanupFailures);

            if (cleanupFailures.Count > 0)
            {
                cleanupFailures.Insert(0, startupException);
                throw new AggregateException(
                    $"Starting browser test run '{runId}' failed and cleanup " +
                    "reported additional failures.",
                    cleanupFailures);
            }

            throw;
        }
    }

    public Task<IBrowserContext> CreateJourneyBrowserContextAsync()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        return _playwright.CreateBrowserContextAsync();
    }

    public async Task CaptureFailureArtifactsAsync(
        string journeyName,
        IPage? page,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(journeyName);
        cancellationToken.ThrowIfCancellationRequested();

        var safeJourneyName = MakeSafeFileName(journeyName);
        var diagnostics = new List<string>();
        var failures = new List<Exception>();

        if (page is not null)
        {
            var tracePath = GetArtifactPath($"{safeJourneyName}.zip");
            await CaptureArtifactAsync(
                "Playwright trace",
                tracePath,
                diagnostics,
                failures,
                () => _playwright.SaveTraceAsync(page.Context, tracePath));

            var screenshotPath = GetArtifactPath($"{safeJourneyName}.png");
            await CaptureArtifactAsync(
                "full-page screenshot",
                screenshotPath,
                diagnostics,
                failures,
                async () =>
                {
                    await page.ScreenshotAsync(
                        new PageScreenshotOptions
                        {
                            Path = screenshotPath,
                            FullPage = true
                        });
                });

            var htmlPath = GetArtifactPath($"{safeJourneyName}.html");
            await CaptureArtifactAsync(
                "HTML dump",
                htmlPath,
                diagnostics,
                failures,
                async () =>
                {
                    var html = await page.ContentAsync();
                    await File.WriteAllTextAsync(
                        htmlPath,
                        html,
                        cancellationToken);
                });
        }

        var stdoutPath = GetArtifactPath($"{safeJourneyName}.stdout.log");
        await CaptureArtifactAsync(
            "Web stdout",
            stdoutPath,
            diagnostics,
            failures,
            () => File.WriteAllTextAsync(
                stdoutPath,
                StandardOutput,
                cancellationToken));

        var stderrPath = GetArtifactPath($"{safeJourneyName}.stderr.log");
        await CaptureArtifactAsync(
            "Web stderr",
            stderrPath,
            diagnostics,
            failures,
            () => File.WriteAllTextAsync(
                stderrPath,
                StandardError,
                cancellationToken));

        var metadataPath = GetArtifactPath($"{safeJourneyName}.json");
        var metadata = new
        {
            RunId,
            DatabaseName,
            BaseUrl,
            Journey = journeyName,
            CapturedAtUtc = DateTime.UtcNow,
            Artifacts = diagnostics,
            CaptureErrors = failures.Select(failure => failure.Message).ToArray()
        };

        try
        {
            await File.WriteAllTextAsync(
                metadataPath,
                JsonSerializer.Serialize(
                    metadata,
                    new JsonSerializerOptions { WriteIndented = true }),
                cancellationToken);
            diagnostics.Add(metadataPath);
        }
        catch (Exception exception)
        {
            failures.Add(new InvalidOperationException(
                $"Could not write failure metadata to '{metadataPath}'.",
                exception));
        }

        if (failures.Count > 0)
        {
            throw new AggregateException(
                $"One or more failure artifacts could not be captured for " +
                $"journey '{journeyName}'.",
                failures);
        }
    }

    public string GetArtifactPath(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (!string.Equals(
                Path.GetFileName(fileName),
                fileName,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Artifact names must be file names, not paths.",
                nameof(fileName));
        }

        return Path.Combine(ArtifactDirectory, fileName);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var failures = new List<Exception>();

        await TryCleanupAsync(_playwright, failures);
        await TryCleanupAsync(_host, failures);
        await TryCleanupAsync(_database, failures);

        if (failures.Count > 0)
        {
            throw new AggregateException(
                $"Tearing down browser test run '{RunId}' failed.",
                failures);
        }
    }

    private static async Task TryCleanupAsync(
        IAsyncDisposable? resource,
        ICollection<Exception> failures)
    {
        if (resource is null)
        {
            return;
        }

        try
        {
            await resource.DisposeAsync();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
    }

    private static string MakeSafeFileName(string value)
    {
        var characters = value
            .Select(character =>
                char.IsAsciiLetterOrDigit(character) || character is '-' or '_'
                    ? character
                    : '_')
            .ToArray();

        return new string(characters);
    }

    private async Task CaptureArtifactAsync(
        string description,
        string path,
        ICollection<string> capturedArtifacts,
        ICollection<Exception> captureFailures,
        Func<Task> capture)
    {
        try
        {
            await capture();
            capturedArtifacts.Add(path);
        }
        catch (Exception exception)
        {
            captureFailures.Add(new InvalidOperationException(
                $"Could not capture {description} at '{path}'.",
                exception));
        }
    }
}
