using System.Diagnostics;

class Program
{
    const byte rune = (byte)'\n';

    // since the goal is to count as fast as possible, will use blocking API's for
    // reads versus using async IO methods (though there may be soem overallping 
    // IO caching bits that I could be missing out on in windows by doing this)
    private static long CountLines(
        string filePath, int chunkSize, long start = 0, long end = long.MaxValue
    )
    {
        var lineCount = 0L;
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: chunkSize, FileOptions.SequentialScan
            );

            fs.Seek(start, SeekOrigin.Begin);

            var currentPos = start;
            var bufSize = end - start < chunkSize ? (end - start) : chunkSize;
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

        return lineCount;
    }

    private static (long totalLines, TimeSpan elapsedTime) DoCount(Config cfg)
    {
        // I am being a bit pedantic about the start/stop due to some issues 
        // locally when starting up the runtime (e.g. I need to ensure things 
        // are primed before starting the stopwatch)
        using var executor = new ExecutionTimer(cfg.MaxConcurrency);
        var fileSize = new FileInfo(cfg.FilePath).Length;
        var totalLines = 0L;
        var partitionSize = (fileSize + cfg.MaxConcurrency - 1) / cfg.MaxConcurrency;
        for (var i = 0; i < cfg.MaxConcurrency; i++)
        {
            var start = i * partitionSize;
            var end = Math.Min(start + partitionSize, fileSize);
            executor.EnqueueAction(() =>
            {
                Interlocked.Add(
                    ref totalLines,
                    CountLines(cfg.FilePath, cfg.ChunkSize, start, end)
                );
            });
        }

        executor.Wait();

        return (totalLines, executor.Elapsed);
    }

    static void Main(string[] args)
    {
        var cfg = ParseArgs(args);
        var (totalLines, elapsedTime) = DoCount(cfg);

        Console.WriteLine($"  Total lines: {totalLines}");
        Console.WriteLine($"Total threads: {cfg.MaxConcurrency}");
        Console.WriteLine($"    File size: {GetFileSize(cfg.FilePath)}");
        Console.WriteLine($" Time elapsed: {elapsedTime.Milliseconds} millis");
    }

    private static Config ParseArgs(string[] args)
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

        return new Config
        {
            FilePath = args[0],
            ChunkSize = chunkSize,
            MaxConcurrency = maxConcurrency
        };
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

    private sealed class ExecutionTimer : IDisposable
    {
        private readonly Stopwatch stopwatch = new();
        private readonly CountdownEvent countdown;
        private readonly Barrier barrier;
        private int enqueued = 0;

        public ExecutionTimer(int executionCount)
        {
            this.countdown = new CountdownEvent(executionCount);
            this.barrier = new Barrier(executionCount, _ => stopwatch.Start());
        }

        public void EnqueueAction(Action action)
        {
            if (Interlocked.Increment(ref enqueued) > countdown.InitialCount)
            {
                throw new InvalidOperationException("Cannot enqueue more than the initial count");
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    barrier.SignalAndWait();
                    action();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
                finally
                {
                    if (countdown.Signal()) stopwatch.Stop();
                }
            });
        }

        public void Wait() => countdown.Wait();

        public TimeSpan Elapsed => stopwatch.Elapsed;

        public void Dispose()
        {
            countdown.Dispose();
            barrier.Dispose();
        }
    }
}