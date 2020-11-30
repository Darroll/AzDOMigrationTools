using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VstsSyncMigrator.Engine
{
    public abstract class ProcessingContextBase : ITfsProcessingContext
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.ProcessingContextBase"));

        private readonly MigrationEngine _me;
        private ProcessingStatus _status;

        #endregion

        #region - Internal Members

        internal abstract void InternalExecute();

        #endregion

        #region - Public Members

        public MigrationEngine Engine
        {
            get { return _me; }
        }

        public abstract string Name { get; }

        public ProcessingStatus Status { get => _status; private set => _status = value; }

        public ProcessingContextBase(MigrationEngine engine)
        {
            // Store migration engine.
            _me = engine;

            // Set default status.
            _status = ProcessingStatus.None;
        }

        public void Execute()
        {
            // Send telemetry data.
            Telemetry.Current.TrackPageView(Name);

            // Send some traces.
            _mySource.Value.TraceInformation($"{Name} Start");
            _mySource.Value.Flush();

            // Create a stop watch to measure the execution time.
            Stopwatch executeTimer = new Stopwatch();

            // Keep connection attempt time.
            DateTime executeStart = DateTime.Now;

            // Start timer.
            executeTimer.Start();

            try
            {
                // Change status to running.
                Status = ProcessingStatus.Running;

                // Execute.
                InternalExecute();

                // Change status to complete.
                Status = ProcessingStatus.Complete;

                // Stop timer.
                executeTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackEvent(
                        "ProcessingContextComplete",
                        new Dictionary<string, string> {
                            { "Name", Name},
                            { "Target Project", Engine.Target.Name},
                            { "Target Collection", Engine.Target.Collection.Name },
                            { "Status", Status.ToString() }
                        },
                        new Dictionary<string, double> {
                            { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                        }
                    );

                // Send some traces.
                _mySource.Value.TraceInformation($"{Name} Complete {0} ");
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Change status to failed.
                Status = ProcessingStatus.Failed;

                // Stop timer.
                executeTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackException(
                        ex,
                        new Dictionary<string, string> {
                            { "Name", Name},
                            { "Target Project", Engine.Target.Name},
                            { "Target Collection", Engine.Target.Collection.Name },
                            { "Status", Status.ToString() }
                        },
                        new Dictionary<string, double> {
                            { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                        }
                      );

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"[EXCEPTION] {ex}");
                _mySource.Value.Flush();
            }
            finally
            {
                // Send telemetry data.
                Telemetry.Current.TrackRequest(Name, executeStart, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }
        }

        #endregion
    }
}