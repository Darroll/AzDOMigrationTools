using System;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class AttachementExportMigrationContext : AttachementMigrationContextBase
    {
        #region - Static Declarations
        // Create a trace source.

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.AttachementExportMigrationContext"));

        #endregion

        #region - Private Members

        private readonly AttachementExportMigrationConfig _config;

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Create a stop watch to measure the query execution time.
            Stopwatch queryTimer = new Stopwatch();

            // Start timer.
            queryTimer.Start();

            // Get source work item store.
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(Engine.Source, WorkItemStoreFlags.None);

            // Create a new query context.
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);

            // Add parameters to context.            
            tfsqc.AddParameter("TeamProject", Engine.Source.Name);

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id]", _config.QueryBit);
            //tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", _config.QueryBit);

            // Execute.
            WorkItemCollection sourceWIS = tfsqc.Execute();

            // How many work items to process.
            int currentWI = sourceWIS.Count;

            // Get work item service.
            WorkItemServer workItemServer = Engine.Source.Collection.GetService<WorkItemServer>();

            foreach (WorkItem wi in sourceWIS)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Attachement Export: {0} of {1} - {2}", currentWI, sourceWIS.Count, wi.Id);
                _mySource.Value.Flush();

                foreach (Attachment wia in wi.Attachments)
                {
                    string fname = string.Format("{0}#{1}", wi.Id, wia.Name);

                    // Send some traces.
                    _mySource.Value.TraceInformation("-");
                    _mySource.Value.TraceInformation(fname);
                    _mySource.Value.Flush();

                    // Define file path.
                    string fpath = Path.Combine(ExportPath, fname);
                    if (!File.Exists(fpath))
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("...downloading");
                        _mySource.Value.Flush();

                        try
                        {
                            var fileLocation = workItemServer.DownloadFile(wia.Id);
                            File.Copy(fileLocation, fpath, true);

                            // Send some traces.
                            _mySource.Value.TraceInformation("...done");
                            _mySource.Value.Flush();
                        }
                        catch (Exception ex)
                        {
                            // Send telemetry data.
                            Telemetry.Current.TrackException(ex);

                            // Send some traces.
                            _mySource.Value.TraceInformation($"\r\nException downloading attachements {ex.Message}");
                            _mySource.Value.Flush();
                        }

                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("...skipping");
                        _mySource.Value.Flush();
                    }

                    // Send some traces.
                    _mySource.Value.TraceInformation("...done");
                    _mySource.Value.Flush();
                }

                // Decrement counter.
                currentWI--;
            }

            // Stop timer.
            queryTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"EXPORT DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", queryTimer.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "AttachementExportMigrationContext"; }
        }
        public AttachementExportMigrationContext(MigrationEngine me, AttachementExportMigrationConfig config) : base(me)
        {
            _config = config;
        }

        #endregion
    }
}