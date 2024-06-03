using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
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

        var cfg = new Config
        {
            FilePath = args[0],
            ChunkSize = 1024 * 1024 * 8,
            MaxConcurrency = maxConcurrency
        };

        var stopwatch = Stopwatch.StartNew();
        var totalLines = DoCount(cfg);
        Console.WriteLine($"  Total lines: {totalLines}");
        Console.WriteLine($"Total threads: {maxConcurrency}");
        Console.WriteLine($" Time elapsed: {stopwatch.Elapsed}");
    }

    private struct Config
    {
        public string FilePath { get; init; }
        public long ChunkSize { get; init; }
        public int MaxConcurrency { get; init; }
    }

    private static long DoCount(Config cfg)
    {
        var fileSize = new FileInfo(cfg.FilePath).Length;
        var partitionSize = (fileSize + cfg.MaxConcurrency - 1) / cfg.MaxConcurrency;

        var tasks = new List<Task<long>>();
        for (var i = 0; i < cfg.MaxConcurrency; i++)
        {
            var start = i * partitionSize;
            tasks.Add(
            Task.Factory.StartNew(
                () => CountLines(cfg, start, Math.Min(start + partitionSize, fileSize)),
                TaskCreationOptions.LongRunning
            ));
        }

        Task.WhenAll(tasks.ToArray() as Task[]);

        return tasks.Sum(t => t.Result);
    }

    // since the goal is to count as fast as possible, will use blocking API's for
    // reads versus deferring to the underlying task thread pool via async/await
    private static long CountLines(Config cfg, long start, long end)
    {
        using var fs = new FileStream(cfg.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        fs.Seek(start, SeekOrigin.Begin);

        var bufSize = cfg.ChunkSize;
        if (end - start < bufSize)
        {
            bufSize = end - start;
        }

        var buf = new byte[bufSize];
        long lineCount = 0;

        while (start < end)
        {
            var bytesRead = fs.Read(buf, 0, buf.Length);
            if (bytesRead == 0)
            {
                break;
            }

            lineCount += buf.Take(bytesRead).Count(b => b == '\n');
            start += bytesRead;
        }

        return lineCount;
    }
}