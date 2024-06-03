using System.Diagnostics;
using System.Net;

class Program
{
    const byte rune = (byte)'\n';

    // since the goal is to count as fast as possible, will use blocking API's for
    // reads versus using async IO methods (though there may be soem overallping 
    // IO caching bits that I could be missing out on in windows by doing this)
    private static void CountLines(Config cfg, long start, long end, Action starting, Action<long> done)
    {
        var lineCount = 0L;
        try
        {
            starting();

            using var fs = new FileStream(cfg.FilePath, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: cfg.ChunkSize, FileOptions.SequentialScan
            );

            fs.Seek(start, SeekOrigin.Begin);

            var currentPos = start;
            var bufSize = end - start < cfg.ChunkSize ? (end - start) : cfg.ChunkSize;
            var buf = new Span<byte>(new byte[bufSize]);
            do
            {
                var bytesRead = fs.Read(buf);
                if (bytesRead == 0)
                {
                    break;
                }

                lineCount += buf.Count(rune);

                currentPos += bytesRead;
            } while (currentPos < end);
        }
        catch (Exception e)
        {
            // just report since we're in a thread pool thread
            Console.WriteLine($"Error: {e.Message}");
        }

        done(lineCount);
    }

    private static (long totalLines, TimeSpan elapsedTime) DoCount(Config cfg)
    {
        // I am being a bit pedantic about the start/stop due to some issues 
        // locally when starting up the runtime (e.g. I need to ensure things 
        // are primed before starting the stopwatch)
        using var counter = new CountdownCounter(cfg.MaxConcurrency);
        var stopwatch = new BarrierStopwatch(cfg.MaxConcurrency);

        var starting = stopwatch.SignalStartAndWait;
        var done = new Action<long>(count =>
        {
            counter.IncrementAndSignal(count);
            stopwatch.SignalStop();
        });

        var fileSize = new FileInfo(cfg.FilePath).Length;
        var partitionSize = (fileSize + cfg.MaxConcurrency - 1) / cfg.MaxConcurrency;
        for (var i = 0; i < cfg.MaxConcurrency; i++)
        {
            var start = i * partitionSize;
            var end = Math.Min(start + partitionSize, fileSize);
            ThreadPool.QueueUserWorkItem(_ => CountLines(cfg, start, end, starting, done));
        }

        counter.Wait();

        return (counter.Count, stopwatch.Elapsed);
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            var appName = Environment.GetCommandLineArgs()[0];
            Console.WriteLine($"Usage: ${appName} <filename> <max_concurrency_optional>");
            Environment.Exit(1);
        }

        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"File ${args[0]} not found");
            Environment.Exit(1);
        }

        var maxConcurrency = Environment.ProcessorCount;
        if (args.Length > 1)
        {
            maxConcurrency = int.Parse(args[1]);
            if (maxConcurrency <= 0)
            {
                Console.WriteLine("Concurrency must be greater than 0");
                Environment.Exit(1);
            }
        }

        var chunkSize = 1024 * 1024 * 4;
        if (args.Length > 2)
        {
            chunkSize = int.Parse(args[2]);
            if (chunkSize <= 0)
            {
                Console.WriteLine("Chunksize must be greater than 0");
                Environment.Exit(1);
            }
        }

        var cfg = new Config
        {
            FilePath = args[0],
            ChunkSize = chunkSize,
            MaxConcurrency = maxConcurrency
        };

        var (totalLines, elapsedTime) = DoCount(cfg);

        Console.WriteLine($"  Total lines: {totalLines}");
        Console.WriteLine($"Total threads: {maxConcurrency}");
        Console.WriteLine($"    File size: {GetFileSize(cfg.FilePath)}");
        Console.WriteLine($" Time elapsed: {elapsedTime.Milliseconds} millis");
    }

    private static string GetFileSize(string filePath)
    {
        var fileSize = new FileInfo(filePath).Length;
        if (fileSize < 1024)
        {
            return $"{fileSize} bytes";
        }

        if (fileSize < 1024 * 1024)
        {
            return $"{fileSize / 1024} KB";
        }

        if (fileSize < 1024 * 1024 * 1024)
        {
            return $"{fileSize / 1024 / 1024} MB";
        }

        return $"{fileSize / 1024 / 1024 / 1024} GB";
    }

    private readonly struct Config
    {
        public string FilePath { get; init; }
        public int ChunkSize { get; init; }
        public int MaxConcurrency { get; init; }
    }

    private sealed class BarrierStopwatch(int count)
    {
        private readonly Barrier _barrier = new(count);
        private readonly Stopwatch _stopwatch = new();
        private int running;

        public void SignalStartAndWait()
        {
            if (Interlocked.Increment(ref running) == count)
            {
                _stopwatch.Start();
            }
            _barrier.SignalAndWait();
        }

        public void SignalStop()
        {
            if (Interlocked.Decrement(ref running) == 0)
            {
                _stopwatch.Stop();
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                lock (_stopwatch)
                {
                    return _stopwatch.Elapsed;
                }
            }
        }
    }

    private sealed class CountdownCounter(int initialCount) : IDisposable
    {
        private long _counter;
        private readonly CountdownEvent _countdownEvent = new(initialCount);

        public void IncrementAndSignal(long count)
        {
            Interlocked.Add(ref _counter, count);
            _countdownEvent.Signal();
        }

        public void Wait() => _countdownEvent.Wait();
        public long Count => _counter;
        public void Dispose() => _countdownEvent.Dispose();
    }
}