using System;
using System.IO;
using System.Diagnostics;
using CommandLine;
using Newtonsoft.Json;
using ADO.Engine.Configuration;
using ADO.Tools;

namespace ADO.Engine.Console
{
    class Program
    {
        #region - Static Declarations

        #region - Private Members

        // Constants.
        private const string _defaultLogFilename = "ADOTools2.log";
        // Create a trace source for the program launcher itself.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("ADO.Engine.Console"));
        // Get the start time of this program to execute.
        private static readonly DateTime _programExecutionStartTime = DateTime.Now;
        // Create a timer to capture the execution time.
        private static readonly Stopwatch _programExecutionTimer = new Stopwatch();
        // Define log path + file.
        private static string _logPath = string.Empty;
        private static string _logFQFN = string.Empty;
        // Engine name.
        private static string _engineName = string.Empty;

        private static object EnableConsoleOutput(object genericOptions)
        {
            // Initialize.
            bool value = false;
            Type optionType = genericOptions.GetType();

            if (typeof(InitExportOptions).Equals(optionType))
            {
                InitExportOptions options = (InitExportOptions)genericOptions;
                value = options.ConsoleOutput;
            }
            else if (typeof(InitImportOptions).Equals(optionType))
            {
                InitImportOptions options = (InitImportOptions)genericOptions;
                value = options.ConsoleOutput;
            }
            else if (typeof(ExportOptions).Equals(optionType))
            {
                ExportOptions options = (ExportOptions)genericOptions;
                value = options.ConsoleOutput;
            }
            else if (typeof(ImportOptions).Equals(optionType))
            {
                ImportOptions options = (ImportOptions)genericOptions;
                value = options.ConsoleOutput;
            }

            // Enable console output.
            if (value)
            {
                // Add the console to trace listeners.
                Trace.Listeners.Add(new ColorTextWriterTraceListener(System.Console.Out));
            }

            // Return exit code 0.
            return 0;
        }

        private static object DefineLog(object genericOptions)
        {
            // Initialize.
            string logPath = null;
            string logFile = null;
            Type optionType = genericOptions.GetType();

            // Define the path where to store logs.
            string validLogPath;

            if (typeof(InitExportOptions).Equals(optionType))
            {
                InitExportOptions options = (InitExportOptions)genericOptions;
                logPath = options.LogPath;
                logFile = options.LogFile;
            }
            else if (typeof(InitImportOptions).Equals(optionType))
            {
                InitImportOptions options = (InitImportOptions)genericOptions;
                logPath = options.LogPath;
                logFile = options.LogFile;
            }
            else if (typeof(ExportOptions).Equals(optionType))
            {
                ExportOptions options = (ExportOptions)genericOptions;
                logPath = options.LogPath;
                logFile = options.LogFile;
            }
            else if (typeof(ImportOptions).Equals(optionType))
            {
                ImportOptions options = (ImportOptions)genericOptions;
                logPath = options.LogPath;
                logFile = options.LogFile;
            }

            // Create the log path where to send messages.
            if (logPath != null)
            {
                // Is it a valid root path?
                try
                {
                    validLogPath = Path.GetFullPath(logPath);
                }
                catch (Exception)
                {
                    return 1;
                }

                // Create the log path.
                _logPath = Tracing.CreateLogPath(logPath);
            }
            else
                _logPath = Tracing.CreateDefaultLogPath();

            // Define the fully qualified filename to store logs.
            if (logFile == null)
                _logFQFN = Path.Combine(_logPath, _defaultLogFilename);
            else
                _logFQFN = Path.Combine(_logPath, logFile);

            // Return exit code 0.
            return 0;
        }

        private static object PauseBeforeStart(object genericOptions)
        {
            // Initialize.
            bool value = false;
            Type optionType = genericOptions.GetType();

            if (typeof(InitExportOptions).Equals(optionType))
            {
                InitExportOptions options = (InitExportOptions)genericOptions;
                value = options.PauseBeforeStart;
            }
            else if (typeof(InitImportOptions).Equals(optionType))
            {
                InitImportOptions options = (InitImportOptions)genericOptions;
                value = options.PauseBeforeStart;
            }
            else if (typeof(ExportOptions).Equals(optionType))
            {
                ExportOptions options = (ExportOptions)genericOptions;
                value = options.PauseBeforeStart;
            }
            else if (typeof(ImportOptions).Equals(optionType))
            {
                ImportOptions options = (ImportOptions)genericOptions;
                value = options.PauseBeforeStart;
            }

            // Pause before start.
            if (value)
            {
                // Pause until a key is pressed.
                System.Console.WriteLine("Press any key to continue (Attach to process if you want to debug)");
                System.Console.ReadKey();
            }

            // Return exit code 0.
            return 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It will use it but no time has been invested yet on this feature.")]
        private static object RunInitExportAndReturnExitCode(InitExportOptions opts)
        {
            // Default JSON configuration file to create.
            string file = "configuration.json";

            if (!File.Exists(file))
            {
                // Initialize a stream writer and a JSON serializer.
                StreamWriter sw = new StreamWriter(file);
                JsonSerializer serializer = new JsonSerializer();

                // Specify standard indented formatting for json.
                JsonTextWriter jw = new JsonTextWriter(sw)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = ' ',
                    Indentation = 4
                };

                // Serialize and write to file.
                serializer.Serialize(jw, Configuration.ProjectExport.EngineConfiguration.GetDefault());

                // Close streams.
                jw.Close();
                sw.Close();

                // Send some traces.
                _mySource.Value.TraceInformation("New default configuration.json file has been created");
                _mySource.Value.Flush();
            }

            // Return exit code 0.
            return 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It will use it but no time has been invested yet on this feature.")]
        private static object RunInitImportAndReturnExitCode(InitImportOptions opts)
        {
            // Default JSON configuration file to create.
            string file = "configuration.json";

            if (!File.Exists(file))
            {
                // Initialize a stream writer and a JSON serializer.
                StreamWriter sw = new StreamWriter(file);
                JsonSerializer serializer = new JsonSerializer();

                // Specify standard indented formatting for json.
                JsonTextWriter jw = new JsonTextWriter(sw)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = ' ',
                    Indentation = 4
                };

                // Serialize and write to file.
                serializer.Serialize(jw, Configuration.ProjectImport.EngineConfiguration.GetDefault());

                // Close streams.
                jw.Close();
                sw.Close();

                // Send some traces.
                _mySource.Value.TraceInformation("New default configuration.json file has been created");
                _mySource.Value.Flush();
            }

            // Return exit code 0.
            return 0;
        }

        private static object RunExportAndReturnExitCode(ExportOptions opts)
        {
            // Set engine name.
            _engineName = "Project Export";

            // Define an empty engine configuration object.
            Configuration.ProjectExport.EngineConfiguration engineConfig;

            // Use a default JSON configuration filename if not defined.
            if (opts.ConfigFile == string.Empty)
                opts.ConfigFile = "configuration.json";

            if (!File.Exists(opts.ConfigFile))
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, "The configuration file {0} does not exist, nor the default configuration.json. Use 'init' verb to create a configuration file first", opts.ConfigFile);
                _mySource.Value.Flush();

                // Return an exit code of 1.
                return 1;
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Load configuration file");
                _mySource.Value.Flush();

                // Create a stream reader to read JSON configuration file.
                StreamReader sr = new StreamReader(opts.ConfigFile);
                string configJsonContent = sr.ReadToEnd();

                // Close and dispose of stream reader.
                sr.Close();
                sr.Dispose();

                // Deserialize json content to revitalize object.
                engineConfig = JsonConvert.DeserializeObject<Configuration.ProjectExport.EngineConfiguration>(configJsonContent, new EngineConfigurationJsonConverter());

                // Send some traces.
                _mySource.Value.TraceInformation("Configuration file loaded");
                _mySource.Value.Flush();

                // Show configuration file at the console only if the console output option is defined.
                if (opts.ConsoleOutput)
                    ADO.Tools.Utility.OutputAllText(opts.ConfigFile);
            }

            // Send some traces.
            _mySource.Value.TraceInformation("Create {0}", _engineName);
            _mySource.Value.Flush();

            // Create engine and pass configuration.
            ProjectExportEngine engine = new ProjectExportEngine(engineConfig);

            // Send some traces.
            _mySource.Value.TraceInformation("{0} created", _engineName);
            _mySource.Value.Flush();

            // Send some traces.
            _mySource.Value.TraceInformation("Start {0}", _engineName);
            _mySource.Value.Flush();

            // Execute engine.
            engine.Run();

            // Send some traces.
            _mySource.Value.TraceInformation("{0} has stopped", _engineName);
            _mySource.Value.Flush();

            // Return exit code 0.
            return 0;
        }

        private static object RunImportAndReturnExitCode(ImportOptions opts)
        {
            // Set engine name.
            _engineName = "Project Import";

            // Define an empty engine configuration object.
            Configuration.ProjectImport.EngineConfiguration engineConfig;

            // Use a default JSON configuration filename if not defined.
            if (opts.ConfigFile == string.Empty)
                opts.ConfigFile = "configuration.json";

            if (!File.Exists(opts.ConfigFile))
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, "The configuration file {0} does not exist, nor the default configuration.json. Use 'init' verb to create a configuration file first", opts.ConfigFile);
                _mySource.Value.Flush();

                // Return an exit code of 1.
                return 1;
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Load configuration file");
                _mySource.Value.Flush();

                // Create a stream reader to read JSON configuration file.
                StreamReader sr = new StreamReader(opts.ConfigFile);
                string configJsonContent = sr.ReadToEnd();

                // Close and dispose of stream reader.
                sr.Close();
                sr.Dispose();

                // Deserialize json content to revitalize object.
                engineConfig = JsonConvert.DeserializeObject<Configuration.ProjectImport.EngineConfiguration>(configJsonContent, new EngineConfigurationJsonConverter());

                // Send some traces.
                _mySource.Value.TraceInformation("Configuration file loaded");
                _mySource.Value.Flush();

                // Show configuration file at the console only if the console output option is defined.
                if (opts.ConsoleOutput)
                    ADO.Tools.Utility.OutputAllText(opts.ConfigFile);
            }

            // Send some traces.
            _mySource.Value.TraceInformation("Create {0}", _engineName);
            _mySource.Value.Flush();

            // Create engine and pass configuration.
            ProjectImportEngine engine = new ProjectImportEngine(engineConfig);

            // Send some traces.
            _mySource.Value.TraceInformation("{0} created", _engineName);
            _mySource.Value.Flush();

            // Send some traces.
            _mySource.Value.TraceInformation("Start {0}", _engineName);
            _mySource.Value.Flush();

            // Execute engine.
            engine.Run();

            // Send some traces.
            _mySource.Value.TraceInformation("{0} has stopped", _engineName);
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

            // Initialize
            int result = 0;
            string assemblyFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Define log path + file.
            result = (int)Parser.Default.ParseArguments<InitImportOptions, InitExportOptions, ExportOptions, ImportOptions>(args).MapResult(
                (InitImportOptions options) => DefineLog(options),
                (InitExportOptions options) => DefineLog(options),
                (ExportOptions options) => DefineLog(options),
                (ImportOptions options) => DefineLog(options),
                errs => 1);

            // If enable console output has been passed, add the console to trace listeners.
            if (result == 0)
            {
                result = (int)Parser.Default.ParseArguments<InitImportOptions, InitExportOptions, ExportOptions, ImportOptions>(args).MapResult(
                    (InitImportOptions options) => EnableConsoleOutput(options),
                    (InitExportOptions options) => EnableConsoleOutput(options),
                    (ExportOptions options) => EnableConsoleOutput(options),
                    (ImportOptions options) => EnableConsoleOutput(options),
                    errs => 2);
            }

            if (result == 0)
            {
                // Add a log file to trace listeners.
                // Add date + time when logging a trace.
                Trace.Listeners.Add(new TextWriterTraceListener(_logFQFN) { TraceOutputOptions = TraceOptions.DateTime });
                // Remove the autoflush behavior.
                Trace.AutoFlush = false;

                // Send some traces.
                Tracing.SendStartLineTrace(_mySource.Value);
                _mySource.Value.TraceInformation("Running version detected as {0}", assemblyVersion);
                Tracing.SendStartTimeTrace(_mySource.Value, _programExecutionStartTime);
                _mySource.Value.Flush();
            }

            if (result == 0)
            {
                // Pause before starting the processor.
                result = (int)Parser.Default.ParseArguments<InitImportOptions, InitExportOptions, ExportOptions, ImportOptions>(args).MapResult(
                    (InitImportOptions options) => PauseBeforeStart(options),
                    (InitExportOptions options) => PauseBeforeStart(options),
                    (ExportOptions options) => PauseBeforeStart(options),
                    (ImportOptions options) => PauseBeforeStart(options),
                    errs => 3);
            }

            if (result == 0)
            {
                // Parse the command verbs and options.
                result = (int)Parser.Default.ParseArguments<InitImportOptions, InitExportOptions, ExportOptions, ImportOptions>(args).MapResult(
                (InitImportOptions options) => RunInitImportAndReturnExitCode(options),
                (InitExportOptions options) => RunInitExportAndReturnExitCode(options),
                (ExportOptions options) => RunExportAndReturnExitCode(options),
                (ImportOptions options) => RunImportAndReturnExitCode(options),
                errs => 10);

                // Send some traces.
                Tracing.SendEndLineTrace(_mySource.Value);
                _mySource.Value.Flush();

                // Stop the timer.
                _programExecutionTimer.Stop();

                // Send some traces.
                Tracing.SendDurationTrace(_mySource.Value, _programExecutionTimer);
                Tracing.SendEndTimeTrace(_mySource.Value);
                _mySource.Value.Flush();
            }


            System.Console.WriteLine("\r\n\r\nApplication Run Complete\r\nPress any key to continue.\r\n");
            System.Console.ReadKey();

            // Return Exit code.
            return result;
        }

        #endregion

        #endregion

        #region - Private Members.

        #region - Classes used for command line verbs and options.

        [Verb("initimport", HelpText = "Creates initial configuration file for import engine")]
        private class InitImportOptions
        {
            #region - General options that apply to all verbs.

            [Option('o', "consoleoutput", Required = false, HelpText = "Will output engine and processor messages at the console.")]
            public bool ConsoleOutput { get; set; }

            [Option('o', "logpath", Required = false, HelpText = "Will create the log file under this path.")]
            public string LogPath { get; set; }

            [Option('o', "logfile", Required = false, HelpText = "Will create the log file with this filename.")]
            public string LogFile { get; set; }

            [Option('p', "pausebeforestart", Required = false, HelpText = "Will pause the engine before it starts.")]
            public bool PauseBeforeStart { get; set; }

            #endregion
        }

        [Verb("initexport", HelpText = "Creates initial configuration file for export engine")]
        private class InitExportOptions
        {
            #region - General options that apply to all verbs.

            [Option('o', "consoleoutput", Required = false, HelpText = "Will output engine and processor messages at the console.")]
            public bool ConsoleOutput { get; set; }

            [Option('o', "logpath", Required = false, HelpText = "Will create the log file under this path.")]
            public string LogPath { get; set; }

            [Option('o', "logfile", Required = false, HelpText = "Will create the log file with this filename.")]
            public string LogFile { get; set; }

            [Option('p', "pausebeforestart", Required = false, HelpText = "Will pause the engine before it starts.")]
            public bool PauseBeforeStart { get; set; }

            #endregion
        }

        [Verb("export", HelpText = "Export ADO objects from source project.")]
        private class ExportOptions
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }

            #region - General options that apply to all verbs.

            [Option('o', "consoleoutput", Required = false, HelpText = "Will output engine and processor messages at the console.")]
            public bool ConsoleOutput { get; set; }

            [Option('o', "logpath", Required = false, HelpText = "Will create the log file under this path.")]
            public string LogPath { get; set; }

            [Option('o', "logfile", Required = false, HelpText = "Will create the log file with this filename.")]
            public string LogFile { get; set; }

            [Option('p', "pausebeforestart", Required = false, HelpText = "Will pause the engine before it starts.")]
            public bool PauseBeforeStart { get; set; }

            #endregion
        }

        [Verb("import", HelpText = "Import ADO objects to destination project.")]
        private class ImportOptions
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }

            #region - General options that apply to all verbs.

            [Option('o', "consoleoutput", Required = false, HelpText = "Will output engine and processor messages at the console.")]
            public bool ConsoleOutput { get; set; }

            [Option('o', "logpath", Required = false, HelpText = "Will create the log file under this path.")]
            public string LogPath { get; set; }

            [Option('o', "logfile", Required = false, HelpText = "Will create the log file with this filename.")]
            public string LogFile { get; set; }

            [Option('p', "pausebeforestart", Required = false, HelpText = "Will pause the engine before it starts.")]
            public bool PauseBeforeStart { get; set; }

            #endregion
        }

        #endregion

        #endregion
    }
}