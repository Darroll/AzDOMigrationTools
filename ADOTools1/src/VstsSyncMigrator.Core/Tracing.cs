using System;
using System.IO;
using System.Diagnostics;

namespace VstsSyncMigrator
{
    public static class Tracing
    {
        public static TraceSource Create(string sourceName)
        {
            /*  Useful documentation.
                
                https://stackoverflow.com/questions/10581448/add-remove-tracelistener-to-all-tracesources
                https://docs.microsoft.com/en-us/dotnet/api/system.lazy-1?redirectedfrom=MSDN&view=netframework-4.8
            */

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
                        if (Path.GetExtension(file.Name) == ".log")
                            file.Delete();
                }
            }
            else
                Directory.CreateDirectory(path);

            // Return the log path.
            return path;
        }
    }
}
