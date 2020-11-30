using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.TraceListener;

namespace VstsSyncMigrator.Engine
{
    public static class Telemetry
    {
        #region - Private Members

        private const string applicationInsightsKey = "bb3d2679-7fee-4d92-9014-3269fb34801b";
        private static TelemetryClient _telemetryClient;
        
        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Telemetry"));

        #endregion

        #region - Public Members

        public static void AddModules()
        {
            var perfCollectorModule = new PerformanceCollectorModule();
            perfCollectorModule.Counters.Add(new PerformanceCounterCollectionRequest(
              string.Format(@"\.NET CLR Memory({0})\# GC Handles", System.AppDomain.CurrentDomain.FriendlyName), "GC Handles"));
            perfCollectorModule.Initialize(TelemetryConfiguration.Active);
        }

        public static bool EnableTrace { get; set; } = false;

        public static void InitializeTelemetry()
        {
            // Add Application Insight to trace listeners.
            if (EnableTrace) { Trace.Listeners.Add(new ApplicationInsightsTraceListener(applicationInsightsKey)); }

            // Set key.
            TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsKey;

            // Create the telemetry client and assign key.
            _telemetryClient = new TelemetryClient
            {
                InstrumentationKey = applicationInsightsKey
            };

            // Set the telemetry context.
            _telemetryClient.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            _telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            _telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            _telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Send some traces.
            _mySource.Value.TraceInformation("SessionID: {0}", _telemetryClient.Context.Session.Id);
            _mySource.Value.Flush();

            // Add performance counters.
            AddModules();

            // Send telemetry data.
            _telemetryClient.TrackEvent("ApplicationStarted");
        }

        public static TelemetryClient Current { get {
                if (_telemetryClient == null)
                    InitializeTelemetry();

                return _telemetryClient;
                // No change
            } }
        
        #endregion
    }
}
