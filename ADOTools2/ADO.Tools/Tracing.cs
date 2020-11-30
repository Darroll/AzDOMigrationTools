using System;
using System.IO;
using System.Diagnostics;

namespace ADO.Tools
{
    public static class Tracing
    {
        public static void SendStartLineTrace(TraceSource source)
        {
            source.TraceInformation("-------------------------------START------------------------------");
        }

        public static void SendEndLineTrace(TraceSource source)
        {
            source.TraceInformation("--------------------------------END-------------------------------");
        }

        public static void SendDurationTrace(TraceSource source, Stopwatch watch)
        {
            source.TraceInformation("Duration: {0}", watch.Elapsed.ToString("c"));
        }

        public static void SendStartTimeTrace(TraceSource source, DateTime startTime)
        {
            source.TraceInformation("Start Time: {0}", startTime.ToUniversalTime().ToLocalTime());
        }

        public static void SendEndTimeTrace(TraceSource source)
        {
            source.TraceInformation("End Time: {0}", DateTime.Now.ToUniversalTime().ToLocalTime());
        }

        public static TraceSource Create(string sourceName)
        {
            // Create new source.
            TraceSource source = new TraceSource(sourceName);

            // Add all existing listeners to this source.
            source.Listeners.AddRange(Trace.Listeners);

            // Set the source to how all messages.
            source.Switch.Level = SourceLevels.All;

            // Return the trace source newly created.
            return source;
        }

        public static string CreateDefaultLogPath()
        {
            // Initialize.
            string logPath;

            // Get assembly path.
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string p = Path.GetDirectoryName(assemblyPath);

            // Define date/time folder.
            string dateTimeFormat = "yyyyMMddHHmmss";
            string dateTimeFolderName = DateTime.Now.ToString(dateTimeFormat);

            // Define the full path to log folder where to create log files.
            logPath = Path.Combine(p, "logs", dateTimeFolderName);

            // Create it if needed.
            logPath = CreateLogPath(logPath, true);

            // Return the log path.
            return logPath;
        }

        public static string CreateLogPath(string path, bool cleanup = false)
        {
            // Create the path if it does not exist.
            if (Directory.Exists(path))
            {
                if (cleanup)
                {
                    DirectoryInfo di = new DirectoryInfo(path);

                    // Delete just log files.
                    foreach (FileInfo file in di.GetFiles())
                    {
                        if (Path.GetExtension(file.Name) == ".log")
                            file.Delete();
                    }
                }
            }
            else
                Directory.CreateDirectory(path);

            // Return the log path.
            return path;
        }
    }
}