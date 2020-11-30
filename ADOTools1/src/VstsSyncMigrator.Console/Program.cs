using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.ApplicationInsights.DataContracts;
using CommandLine;
using Newtonsoft.Json;
using VstsSyncMigrator.Engine;
using VstsSyncMigrator.Engine.Configuration;
using VstsSyncMigrator.Engine.Configuration.FieldMap;
using VstsSyncMigrator.Core;
using VstsSyncMigrator.Core.Configuration;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        #region - Static Declarations

        // Constants.
        private const string _defaultLogFilename = "BulkImportProject.log";
        private const string _engineName = "bulk import engine";

        // Get the start time of this program to execute.
        static readonly DateTime _programExecutionStartTime = DateTime.Now;
        // Create a timer to capture the execution time.
        static readonly Stopwatch _programExecutionTimer = new Stopwatch();
        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("VstsSyncMigrator.Program"));
        // Define log path + file.
        private static string _logPath = String.Empty;
        private static string _logFQFN = String.Empty;

        #region - Private Members

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Send telemetry data.
            ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.ExceptionObject)
            {
                SeverityLevel = SeverityLevel.Critical,
                HandledAt = ExceptionHandledAt.Unhandled
            };
            Telemetry.Current.TrackException(excTelemetry);
            Telemetry.Current.Flush();
            System.Threading.Thread.Sleep(1000);
        }

        private static object DoNothingWithInitialization()
        {
            // Return exit code 0.
            return 0;
        }

        private static object EnableConsoleOutput(ExecuteOptions opts)
        {
            // Enable console output.
            if (opts.ConsoleOutput)
            {
                // Add the console to trace listeners.
                Trace.Listeners.Add(new ColorTextWriterTraceListener(Console.Out));
            }

            // Return exit code 0.
            return 0;
        }

        private static object DefineLog(ExecuteOptions opts)
        {
            // Define the path where to store logs.
            string validLogPath;

            // Create the log path where to send messages.
            if (opts.LogPath != null)
            {
                // Is it a valid root path?
                try
                {
                    validLogPath = Path.GetFullPath(opts.LogPath);
                }
                catch (Exception)
                {
                    return 1;
                }

                // Create the log path.
                _logPath = Tracing.CreateLogPath(opts.LogPath);
            }
            else
                _logPath = Tracing.CreateDefaultLogPath();

            // Define the fully qualified filename to store logs.
            if (opts.LogFile == null)
                _logFQFN = Path.Combine(_logPath, _defaultLogFilename);
            else
                _logFQFN = Path.Combine(_logPath, opts.LogFile);

            // Return exit code 0.
            return 0;
        }

        private static object PauseBeforeStart(ExecuteOptions opts)
        {
            // Pause before start.
            if (opts.PauseBeforeStart)
            {
                // Pause until a key is pressed.
                Console.WriteLine("Press any key to continue (Attach to process if you want to debug)");
                Console.ReadKey();
            }

            // Return exit code 0.
            return 0;
        }

        private static object RunInitializationAndReturnExitCode()
        {
            // Initialize.
            string file = "configuration.json";

            // Send telemetry data.
            Telemetry.Current.TrackEvent("InitializeCommand");

            if (!File.Exists(file))
            {
                string json = JsonConvert.SerializeObject(EngineConfiguration.GetDefault(),
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter(),
                    new EngineConfigurationJsonConverter());
                StreamWriter sw = new StreamWriter(file);
                sw.WriteLine(json);
                sw.Close();

                // Send some traces.
                _mySource.Value.TraceInformation("New empty configuration.json file has been created");
                _mySource.Value.Flush();
            }

            // Return exit code 0.
            return 0;
        }

        private static object RunExecuteAndReturnExitCode(ExecuteOptions opts)
        {
            // Send telemetry data.
            Telemetry.Current.TrackEvent("ExecuteCommand");

            // Define an engine configuration object.
            EngineConfiguration engineConfig;

            // Use a default JSON configuration filename if not defined.
            if (opts.ConfigFile == string.Empty)
            {
                opts.ConfigFile = "configuration.json";
            }

            if (!File.Exists(opts.ConfigFile))
            {
                var configFileFullPath = new FileInfo(opts.ConfigFile);
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 1, "The configuration file {0} does not exist, nor the default configuration.json. Use 'init' to create a configuration file first", configFileFullPath.FullName);
                _mySource.Value.Flush();

                // Return an exit code of 1.
                return 1;
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Loading configuration file");
                _mySource.Value.Flush();

                // Read the entire file.
                StreamReader sr = new StreamReader(opts.ConfigFile);
                string configurationjson = sr.ReadToEnd();
                sr.Close();

                // Deserialize json object.
                engineConfig = JsonConvert.DeserializeObject<EngineConfiguration>(configurationjson,
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter(),
                    new EngineConfigurationJsonConverter());

                // Send some traces.
                _mySource.Value.TraceInformation("Configuration file loaded");
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation(@"Creating {0}", _engineName);
            _mySource.Value.Flush();

            // Create a migration engine processor.
            MigrationEngine engineProcessor = new MigrationEngine(engineConfig);

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0} created", _engineName);
            _mySource.Value.Flush();

            // Send some traces.
            _mySource.Value.TraceInformation(@"Start {0}", _engineName);
            _mySource.Value.Flush();

            // Execute engine.
            engineProcessor.Run();

            // Send some traces.
            _mySource.Value.TraceInformation(@"{0} has stopped", _engineName);
            _mySource.Value.Flush();

            // Return exit code 0.
            return 0;
        }

        #endregion

        #region - Public Members

        public static int Main(string[] args)
        {
            // Start the timer.
            _programExecutionTimer.Start();

            // Send telemetry data.
            Telemetry.Current.TrackEvent("ApplicationStart");

            // Add an handler for unhandled exception.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Initialize
            int result = 0;
            string assemblyFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Define log path.
            result = (int)Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args).MapResult(
                (InitOptions opts) => DoNothingWithInitialization(),
                (ExecuteOptions opts) => DefineLog(opts),
                errs => 1);

            // If enable console output has been passed, add the console to trace listeners.
            if (result == 0)
            {
                result = (int)Parser.Default.ParseArguments<ExecuteOptions>(args).MapResult(
                (ExecuteOptions opts) => EnableConsoleOutput(opts),
                errs => 2);
            }

            if (result == 0)
            {
                // Add a log file to trace listeners.
                Trace.Listeners.Add( new TextWriterTraceListener(_logFQFN) {TraceOutputOptions = TraceOptions.DateTime} );
                // Remove the autoflush behavior.
                Trace.AutoFlush = false;

                // Send some traces.
                _mySource.Value.TraceInformation("-------------------------------START------------------------------");
                _mySource.Value.TraceInformation("Running version detected as {0}", assemblyVersion);
                _mySource.Value.TraceInformation("Telemetry Enabled: {0}", Telemetry.Current.IsEnabled().ToString());
                _mySource.Value.TraceInformation("SessionID: {0}", Telemetry.Current.Context.Session.Id);
                _mySource.Value.TraceInformation("User: {0}", Telemetry.Current.Context.User.Id);
                _mySource.Value.TraceInformation("Start Time: {0}", _programExecutionStartTime.ToUniversalTime().ToLocalTime());
                _mySource.Value.Flush();
            }

            if (result == 0)
            {
                // Pause before starting the processor.
                result = (int)Parser.Default.ParseArguments<ExecuteOptions>(args).MapResult(
                (ExecuteOptions opts) => PauseBeforeStart(opts),
                errs => 3);
            }

            if (result == 0)
            {
                // Parse the command verbs and options.
                result = (int)Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args).MapResult(
                (InitOptions opts) => RunInitializationAndReturnExitCode(),
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts),
                errs => 10);

                // Send some traces.
                _mySource.Value.TraceInformation("-------------------------------END------------------------------");
                _mySource.Value.Flush();

                // Stop the timer.
                _programExecutionTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackEvent("ApplicationEnd", null, new Dictionary<string, double> { { "ApplicationDuration", _programExecutionTimer.ElapsedMilliseconds } });
                // If telemetry is still active...
                if (Telemetry.Current != null)
                {
                    Telemetry.Current.Flush();
                    // Allow time for flushing.
                    System.Threading.Thread.Sleep(1000);
                }

                // Send some traces.
                _mySource.Value.TraceInformation("Duration: {0}", _programExecutionTimer.Elapsed.ToString("c"));
                _mySource.Value.TraceInformation("End Time: {0}", DateTime.Now.ToUniversalTime().ToLocalTime());
                _mySource.Value.Flush();
            }

            // Return Exit code.
            return result;
        }

        #endregion

        #endregion

        #region - Private Members.

        #region - Classes used for command line verbs and options.

        [Verb("init", HelpText = "Creates initial config file")]
        private class InitOptions
        {
            // There are no options with this verb.
        }

        [Verb("execute", HelpText = "Record changes to the repository.")]
        private class ExecuteOptions
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }

            [Option('o', "consoleoutput", Required = false, HelpText = "Will output processor messages at the console.")]
            public bool ConsoleOutput { get; set; }

            [Option('o', "logpath", Required = false, HelpText = "Will create the log file under this path.")]
            public string LogPath { get; set; }

            [Option('o', "logfile", Required = false, HelpText = "Will create the log file with this filename.")]
            public string LogFile { get; set; }

            [Option('p', "pausebeforestart", Required = false, HelpText = "Will pause the processor before it starts.")]
            public bool PauseBeforeStart { get; set; }
        }

        #endregion

        #endregion
    }
}
