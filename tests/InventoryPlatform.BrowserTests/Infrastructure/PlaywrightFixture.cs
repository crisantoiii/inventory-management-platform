using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace InventoryPlatform.BrowserTests.Infrastructure;

public sealed class PlaywrightFixture : IAsyncDisposable
{
    private readonly ConcurrentDictionary<IBrowserContext, byte> _contexts = new();
    private readonly ConcurrentDictionary<IBrowserContext, byte> _tracedContexts = new();
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private int _disposed;

    private PlaywrightFixture(IPlaywright playwright, IBrowser browser)
    {
        _playwright = playwright;
        _browser = browser;
    }

    public static async Task<PlaywrightFixture> CreateAsync()
    {
        var playwright = await Playwright.CreateAsync();

        try
        {
            var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

            return new PlaywrightFixture(playwright, browser);
        }
        catch (PlaywrightException exception)
        {
            playwright.Dispose();

            throw new InvalidOperationException(
                "Playwright could not launch Chromium. Install the browser " +
                "prerequisite once from the repository root with " +
                "`powershell.exe -NoProfile -ExecutionPolicy Bypass -File " +
                ".\\tests\\InventoryPlatform.BrowserTests\\bin\\Release\\" +
                "net10.0\\playwright.ps1 install`.",
                exception);
        }
    }

    public async Task<IBrowserContext> CreateBrowserContextAsync()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        var context = await _browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                Locale = "en-US",
                TimezoneId = "UTC"
            });

        try
        {
            await context.Tracing.StartAsync(
                new TracingStartOptions
                {
                    Screenshots = true,
                    Snapshots = true,
                    Sources = true
                });

            if (!_contexts.TryAdd(context, 0) ||
                !_tracedContexts.TryAdd(context, 0))
            {
                throw new InvalidOperationException(
                    "The Playwright BrowserContext was already registered.");
            }
        }
        catch
        {
            await context.CloseAsync();
            throw;
        }

        return context;
    }

    public async Task SaveTraceAsync(
        IBrowserContext context,
        string? path = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!_tracedContexts.TryRemove(context, out _))
        {
            throw new InvalidOperationException(
                "The requested BrowserContext has no active trace owned by this fixture.");
        }

        await context.Tracing.StopAsync(
            new TracingStopOptions
            {
                Path = path
            });
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var failures = new List<Exception>();

        foreach (var context in _contexts.Keys)
        {
            if (_tracedContexts.ContainsKey(context))
            {
                await TryStopTraceAsync(context, failures);
            }

            try
            {
                await context.CloseAsync();
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
            finally
            {
                _contexts.TryRemove(context, out _);
                _tracedContexts.TryRemove(context, out _);
            }
        }

        try
        {
            await _browser.CloseAsync();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
        finally
        {
            _playwright.Dispose();
        }

        if (failures.Count > 0)
        {
            throw new AggregateException(
                "One or more Playwright resources failed to close.",
                failures);
        }
    }

    private async Task TryStopTraceAsync(
        IBrowserContext context,
        ICollection<Exception> failures)
    {
        try
        {
            await SaveTraceAsync(context);
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
    }
}
