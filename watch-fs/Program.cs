using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace watch_fs
{
    static class Program
    {
        private static FileSystemWatcher[] _watchers;
        private static readonly Barrier ExitBarrier = new Barrier(2);
        private static readonly object Lock = new object();
        
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return Fail("Please specify at least one folder to watch");
            }

            var existing = args.Where(Directory.Exists).ToArray();
            var missing = args.Except(existing).ToArray();
            if (missing.Any())
            {
                Console.WriteLine($"WARNING: not watching non-existent folders:\n- {string.Join("\n- ", missing)}");
            }

            if (!existing.Any())
            {
                return 2;
            }

            _watchers = existing.Select(
                d =>
                {
                    var watcher = new FileSystemWatcher(d)
                    {
                        Filter = "*.*",
                        IncludeSubdirectories = true,
                    };
                    watcher.Changed += (o, e) => OnEvent("changed", o, e);
                    watcher.Deleted += (o, e) => OnEvent("deleted", o, e);
                    watcher.Created += (o, e) => OnEvent("created", o, e);
                    watcher.EnableRaisingEvents = true;
                    return watcher;
                }).ToArray();
            Console.WriteLine("--- waiting for filesystem changes ---");
            Console.CancelKeyPress += OnCancelled;
            ExitBarrier.SignalAndWait();
            return 0;
        }

        private static void OnCancelled(object sender, ConsoleCancelEventArgs e)
        {
            lock (Lock)
            {
                foreach (var watcher in _watchers)
                {
                    try
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.Dispose();
                    }
                    catch (Exception)
                    {
                        /* ignore */
                    }
                }
                _watchers = new FileSystemWatcher[0];
            }
            ExitBarrier.SignalAndWait();
        }


        private static void OnEvent(string ev, object sender, FileSystemEventArgs args)
        {
            Console.WriteLine($"{ev}: {args.FullPath}");
        }

        private static int Fail(string message)
        {
            Console.WriteLine(message);
            return 1;
        }
    }
}