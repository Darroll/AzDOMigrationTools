using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace VstsSyncMigrator.Engine
{
    public class TfsQueryContext
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.TfsQueryContext"));

        #endregion

        #region - Private Members

        private readonly WorkItemStoreContext _storeContext;
        private readonly Dictionary<string, string> _parameters;

        // Fix for Query SOAP error when passing parameters
        [Obsolete("Temporary work around for SOAP issue https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066")]
        private string WorkAroundForSOAPError(string query, IDictionary<string, string> parameters)
        {
            foreach (string key in parameters.Keys)
            {
                string pattern = "'{0}'";

                if(int.TryParse(parameters[key], out _))
                    pattern = "{0}";
                query = query.Replace(string.Format("@{0}", key), string.Format(pattern, parameters[key]));
            }
            return query;
        }

        #endregion

        #region - Public Members

        public TfsQueryContext(WorkItemStoreContext storeContext)
        {
            // Store context.
            _storeContext = storeContext;

            // Initialize the parameters.
            _parameters = new Dictionary<string, string>();
        }

        public string Query { get; set; }

        public void AddParameter(string name, string value)
        {
            _parameters.Add(name, value);
        }
        
        public WorkItemCollection Execute()
        {
            // Send telemetry data.
            Telemetry.Current.TrackEvent("TfsQueryContext.Execute", _parameters);

            // Create a stop watch to measure the query execution time.
            Stopwatch queryTimer = new Stopwatch();

            // Keep connection attempt time.
            DateTime startTime = DateTime.Now;

            // Start timer.
            queryTimer.Start();

            // Initialize.
            WorkItemCollection wc;

            try
            {
                // TODO: Remove this once bug fixed... https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066 
                Query = WorkAroundForSOAPError(Query, _parameters);

                // Get the work items.
                wc = _storeContext.Store.Query(Query); //, parameters);
                
                // Stop timer.
                queryTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackDependency("Azure DevOps", "TeamService", "Query", startTime, queryTimer.Elapsed, true);

                // Add additional bits to reuse the parameter dictionary for telemetry.
                _parameters.Add("CollectionUrl", _storeContext.Store.TeamProjectCollection.Uri.ToString());
                _parameters.Add("Query", Query);

                // Send telemetry data.
                Telemetry.Current.TrackEvent(
                                                "QueryComplete",
                                                  _parameters,
                                                  new Dictionary<string, double> {
                                                        { "QueryTime", queryTimer.ElapsedMilliseconds },
                                                      { "QueryCount", wc.Count }
                                                  }
                                              );
            }
            catch (Exception ex)
            {
                // Stop timer.
                queryTimer.Stop();

                // Send telemetry data.
                Telemetry.Current.TrackDependency("Azure DevOps", "TeamService", "Query", startTime, queryTimer.Elapsed, false);
                Telemetry.Current.TrackException(
                        ex,
                        new Dictionary<string, string> {
                            { "CollectionUrl", _storeContext.Store.TeamProjectCollection.Uri.ToString() }
                        },
                        new Dictionary<string, double> {
                            { "QueryTime",queryTimer.ElapsedMilliseconds }
                        }
                    );

                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"[EXCEPTION] {ex}");
                _mySource.Value.Flush();

                throw;
            }

            // Return work items.
            return wc;
        }

        #endregion
    }
}