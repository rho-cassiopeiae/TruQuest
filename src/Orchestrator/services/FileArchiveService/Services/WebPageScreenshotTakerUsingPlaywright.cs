using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

using Microsoft.Playwright;

namespace Services;

internal class WebPageScreenshotTakerUsingPlaywright : IWebPageScreenshotTaker
{
    private class WorkerAssignment
    {
        public BlockingCollection<(IEnumerable<string> Urls, TaskCompletionSource<List<string>> Tcs)> Worker { get; init; }
        public int CurrentSize { get; init; }
        public int TargetSize { get; set; }
    }

    private readonly ILogger<WebPageScreenshotTakerUsingPlaywright> _logger;

    private readonly ChannelWriter<(IEnumerable<string> Urls, TaskCompletionSource<List<string>?> Tcs)> _sink;
    private readonly ChannelReader<(IEnumerable<string> Urls, TaskCompletionSource<List<string>?> Tcs)> _stream;

    private readonly BlockingCollection<(IEnumerable<string> Urls, TaskCompletionSource<List<string>> Tcs)>[] _workerQueues;

    private readonly int _targetAvgLoad;

    public WebPageScreenshotTakerUsingPlaywright(
        IConfiguration configuration,
        ILogger<WebPageScreenshotTakerUsingPlaywright> logger,
        IHostApplicationLifetime appLifetime
    )
    {
        _logger = logger;

        var workerCount = configuration.GetValue<int>("Playwright:WorkerCount");
        _targetAvgLoad = configuration.GetValue<int>("Playwright:TargetAvgLoadPerWorker");

        var channel = Channel.CreateUnbounded<(IEnumerable<string> Urls, TaskCompletionSource<List<string>?> Tcs)>(
            new() { SingleReader = true }
        );
        _sink = channel.Writer;
        _stream = channel.Reader;

        _workerQueues = new BlockingCollection<(IEnumerable<string> Urls, TaskCompletionSource<List<string>> Tcs)>[workerCount];
        for (int i = 0; i < _workerQueues.Length; ++i)
        {
            _workerQueues[i] = new();
        }
        for (int i = 0; i < _workerQueues.Length; ++i)
        {
            // @@NOTE: We do this in a separate loop to guarantee that _workerQueue array is fully set
            // by the time the first worker starts.
            int index = i;
            new Thread(() => _processRequests(index)) { IsBackground = true }.Start();
        }

        var cts = CancellationTokenSource.CreateLinkedTokenSource(appLifetime.ApplicationStopping);
        _monitorRequests(cts.Token);
    }

    private async void _monitorRequests(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Starting monitoring requests...");

            await foreach (var request in _stream.ReadAllAsync(ct))
            {
                if (_loadBalanceRequests(request.Urls.Count(), out List<WorkerAssignment>? workerAssignments))
                {
                    int urlsTakenCount = 0;
                    var tasks = new List<Task<List<string>>>(workerAssignments!.Count(a => a.TargetSize > a.CurrentSize));
                    foreach (var workerAssignment in workerAssignments!.Where(a => a.TargetSize > a.CurrentSize))
                    {
                        int urlsToTake = workerAssignment.TargetSize - workerAssignment.CurrentSize;
                        var urls = request.Urls.Skip(urlsTakenCount).Take(urlsToTake);
                        var tcs = new TaskCompletionSource<List<string>>();
                        tasks.Add(tcs.Task);

                        workerAssignment.Worker.Add((Urls: urls, Tcs: tcs));
                        urlsTakenCount += urlsToTake;
                    }

                    Debug.Assert(request.Urls.Count() == urlsTakenCount);

                    var awaiter = Task.WhenAll(tasks).GetAwaiter();
                    awaiter.OnCompleted(() =>
                    {
                        try
                        {
                            List<string>[] result = awaiter.GetResult();
                            request.Tcs.SetResult(result.SelectMany(r => r).ToList());
                        }
                        catch (Exception)
                        {
                            foreach (var task in tasks) // in case some tasks succeeded
                            {
                                if (!task.IsFaulted)
                                {
                                    var filePaths = task.Result;
                                    foreach (var filePath in filePaths)
                                    {
                                        File.Delete(filePath);
                                    }
                                }
                            }

                            request.Tcs.SetResult(null);
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Couldn't assign work due to high average load");
                    request.Tcs.SetResult(null);
                }

                _reportLoad();
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    private void _reportLoad()
    {
        for (int i = 0; i < _workerQueues.Length; ++i)
        {
            _logger.LogInformation("Worker {WorkerIndex} load: {RequestCount}", i, _workerQueues[i].Count);
        }
    }

    private bool _loadBalanceRequests(int requestCount, out List<WorkerAssignment>? workerAssignments)
    {
        workerAssignments = null;
        var avgLoad = (int)(_workerQueues.Average(q => q.Count) + 0.5);
        if (avgLoad >= _targetAvgLoad) return false;

        workerAssignments = _workerQueues
            .Select((q) => new WorkerAssignment
            {
                Worker = q,
                CurrentSize = q.Count,
                TargetSize = q.Count
            })
            .OrderBy(a => a.CurrentSize)
            .ToList();

        while (requestCount > 0)
        {
            int smallestSize = workerAssignments.First().TargetSize;
            int nextSmallestSizeIndex = workerAssignments.Count;
            int nextSmallestSize = int.MaxValue;
            for (int i = 1; i < workerAssignments.Count; ++i)
            {
                if (workerAssignments[i].TargetSize > smallestSize)
                {
                    nextSmallestSizeIndex = i;
                    nextSmallestSize = workerAssignments[i].TargetSize;
                    break;
                }
            }

            int workersToUpCount = nextSmallestSizeIndex;
            int uppedWorkerCount = 0;
            while (requestCount > 0 && workerAssignments[nextSmallestSizeIndex - 1].TargetSize < nextSmallestSize)
            {
                workerAssignments[uppedWorkerCount % workersToUpCount].TargetSize++;
                ++uppedWorkerCount;
                --requestCount;
            }
        }

        return true;
    }

    private async void _processRequests(int workerIndex)
    {
        _logger.LogInformation("Worker {WorkerIndex}: awaiting requests...", workerIndex);

        var queue = _workerQueues[workerIndex];
        while (true)
        {
            var request = queue.Take();
            var filePaths = new List<string>(request.Urls.Count());

            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(
                    userDataDir: string.Empty,
                    new()
                    {
                        ViewportSize = new()
                        {
                            Width = 1920,
                            Height = 1080
                        },
                        Headless = false,
                        Args = new[]
                        {
                            "--headless=new",
                            "--disable-extensions-except=/ublock,/isdcac",
                            "--load-extension=/ublock,/isdcac"
                        }
                    }
                );

                var page = await browser.NewPageAsync();
                foreach (var url in request.Urls)
                {
                    await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.Load }); // 'NetworkIdle' can cause problems on some websites.
                    await page.EvaluateAsync<bool>(
                        @"
                            async (args) => {
                                const {speed, timeoutMs} = args;
                                let timedOut = false;
                                setTimeout(() => timedOut = true, timeoutMs);
                                
                                const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
                                const scrollHeight = () => document.body.scrollHeight;
                                const start = 0;
                                const shouldStop = (position) => position > scrollHeight();
                                const increment = 100;
                                const delayTime = speed === ""slow"" ? 50 : 10;

                                for (let i = start; !timedOut && !shouldStop(i); i += increment)
                                {
                                    window.scrollTo(0, i);
                                    await delay(delayTime);
                                }

                                window.scrollTo(0, 0);

                                return true;
                            }
                        ",
                        new
                        {
                            speed = "slow",
                            timeoutMs = 15000 // @@TODO: Config.
                        }
                    );

                    var filePath = $"/screenshots/{Guid.NewGuid()}.png";
                    filePaths.Add(filePath);

                    await page.ScreenshotAsync(new()
                    {
                        Path = filePath,
                        FullPage = true,
                        Animations = ScreenshotAnimations.Disabled,
                        Timeout = 15000 // @@TODO: Config.
                    });
                }

                await browser.CloseAsync();

                request.Tcs.SetResult(filePaths);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to take a webpage screenshot");

                foreach (var filePath in filePaths)
                {
                    File.Delete(filePath);
                }
                request.Tcs.SetException(ex);
            }
        }
    }

    public async Task<List<string>?> Take(IEnumerable<string> urls)
    {
        var tcs = new TaskCompletionSource<List<string>?>();
        await _sink.WriteAsync((Urls: urls, Tcs: tcs));
        return await tcs.Task;
    }
}