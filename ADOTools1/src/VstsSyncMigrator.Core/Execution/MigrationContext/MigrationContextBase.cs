using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VstsSyncMigrator.Engine
{
    public abstract class MigrationContextBase : ITfsProcessingContext
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.MigrationContextBase"));

        #endregion

        #region - Private Members

        private readonly MigrationEngine _me;

        #endregion

        #region - Internal Members

        internal abstract void InternalExecute();

        #endregion

        #region - Protected Members

        protected MigrationContextBase(MigrationEngine me)
        {
            _me = me;
        }

        #endregion

        #region - Public Members

        public MigrationEngine Engine
        {
            get { return _me; }
        }

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;

        public void Execute()
        {
            // Send telemetry data.
            Telemetry.Current.TrackPageView(this.Name);

            // Send some traces.
            _mySource.Value.TraceInformation($"{Name} Start {0}", Name);
            _mySource.Value.Flush();

            // Create a stop watch to measure the execution time.
            Stopwatch executionTimer = new Stopwatch();

            // Keep execution attempt time.
            DateTime start = DateTime.Now;

            // Start timer.
            executionTimer.Start();

            try
            {
                // Change status to running.
                this.Status = ProcessingStatus.Running;

                // Execute processor.
                this.InternalExecute();

                // Change status to complete.
                this.Status = ProcessingStatus.Complete;

                // Stop timer.
                executionTimer.Stop();

                // Send some traces.
                _mySource.Value.TraceInformation($"{Name} Complete");
                _mySource.Value.Flush();
            }
            catch (Exception ex)
            {
                // Change status to failed.
                this.Status = ProcessingStatus.Failed;

                // Stop timer.
                executionTimer.Stop();
                
                // Send telemetry data.
                Telemetry.Current.TrackException(
                        ex,
                        new Dictionary<string, string>
                        {
                            {"Name", Name},
                            {"Target Project", _me.Target.Name},
                            {"Target Collection", _me.Target.Collection.Name},
                            {"Source Project", _me.Source.Name},
                            {"Source Collection", _me.Source.Collection.Name},
                            {"Status", Status.ToString()}
                        },
                        new Dictionary<string, double>
                        {
                            {"MigrationContextTime", executionTimer.ElapsedMilliseconds}
                        }
                    );

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"[EXCEPTION] {ex.Message}");
                _mySource.Value.Flush();
            }
            finally
            {
                // Send telemetry data.
                Telemetry.Current.TrackRequest(this.Name, start, executionTimer.Elapsed, this.Status.ToString(), (this.Status == ProcessingStatus.Complete));
            }
        }

        #endregion
    }
}