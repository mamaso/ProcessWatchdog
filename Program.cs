using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ProcessWatchdog
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int parentId = Convert.ToInt32(args[0]);
                string childProcessFilePath = Path.GetFullPath(args[1]);

                WatchProcess(parentId, childProcessFilePath);
            }
            catch
            {
                Console.WriteLine("Expected two arguments: <PID> <ChildFilePath>");
            }
        }

        static void WatchProcess(int parentId, string childProcessFilePath)
        {
            WaitForExit(parentId);

            var processes = GetChildProcesses(childProcessFilePath);

            foreach (var child in processes)
            {
                try
                {
                    Console.WriteLine($"Shutting down child process {child.Id}");
                    child.Kill();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error {e.Message}");
                }
            }
        }

        static void WaitForExit(int processId)
        {
            while (Process.GetProcesses().Where(process => process.Id == processId).Count() == 1)
            {
                Console.WriteLine($"Parent process {processId} alive.");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            Console.WriteLine($"Parent process {processId} exited.");
        }

        static IEnumerable<Process> GetChildProcesses(string path)
        {
            if (File.Exists(path))
            {
                var ids =  File.ReadAllLines(path)
                    .Select(line => Convert.ToInt32(line))
                    .ToHashSet();

                var processes = Process.GetProcesses().Where(process => ids.Contains(process.Id));
                return processes;
            }
            else
            {
                return Enumerable.Empty<Process>();
            }
        }
    }
}
