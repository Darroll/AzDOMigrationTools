using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;

namespace VstsSyncMigrator.Engine
{
    public class TeamProjectContext : ITeamProjectContext
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.TeamProjectContext"));

        #endregion

        #region - Private Members

        private readonly Uri _collectionUrl;
        private readonly string _teamProjectName;
        private readonly string _token;
        private TfsTeamProjectCollection _tpc;

        #endregion

        #region - Public Members

        public TeamFoundationIdentity AuthorizedIdentity
        {
            get
            {
                // Initialize.
                TeamFoundationIdentity identity = null;

                if (_tpc != null)
                    identity = _tpc.AuthorizedIdentity;

                // Return identity.
                return identity;
            }
        }

        public TfsTeamProjectCollection Collection
        {
            get
            {
                // Connect and return team project collection.
                this.Connect();

                return _tpc;
            }
        }

        public string Name
        {
            get { return _teamProjectName; }
        }

        public TeamProjectContext(Uri collectionUrl, string teamProjectName, string token)
        {
            _collectionUrl = collectionUrl;
            _teamProjectName = teamProjectName;
            _token = token;
        }

        public void Connect()
        {
            if (_tpc == null)
            {
                // Send telemetry data.
                Telemetry.Current.TrackEvent(
                        "TeamProjectContext.Connect",
                        new Dictionary<string, string> {
                              { "Name", this.Name},
                              { "Target Project", _teamProjectName},
                              { "Target Collection", _collectionUrl.ToString() }
                        }
                    );

                // Create a stop watch to measure the connection time.
                Stopwatch connectionTimer = new Stopwatch();
                
                // Keep connection attempt time.
                DateTime start = DateTime.Now;

                // Start timer.
                connectionTimer.Start();

                // Send some traces.
                _mySource.Value.TraceInformation("Creating TfsTeamProjectCollection Object");
                _mySource.Value.Flush();

                // Prepare connection to Azure DevOps server.
                // string projname = "project";
                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(string.Empty, _token);
                Microsoft.VisualStudio.Services.Common.VssBasicCredential bac = new Microsoft.VisualStudio.Services.Common.VssBasicCredential(nc);
                Microsoft.VisualStudio.Services.Common.VssCredentials tfcc = new Microsoft.VisualStudio.Services.Common.VssCredentials(bac)
                    {
                        PromptType = Microsoft.VisualStudio.Services.Common.CredentialPromptType.DoNotPrompt
                    };
                
                _tpc = new TfsTeamProjectCollection(_collectionUrl, tfcc);

                try
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Connected to {0}", _tpc.Uri.ToString());
                    _mySource.Value.TraceInformation("Validating security for {0}", _tpc.AuthorizedIdentity.ToString());                    
                    _mySource.Value.Flush();

                    // Attemp a connection.
                    _tpc.EnsureAuthenticated();

                    // Stop timer.
                    connectionTimer.Stop();

                    // Send telemetry data.
                    Telemetry.Current.TrackDependency("Azure DevOps", "TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, true);

                    // Send some traces.
                    _mySource.Value.TraceInformation("Access granted");
                    _mySource.Value.Flush();
                }
                catch (TeamFoundationServiceUnavailableException ex)
                {
                    // Send telemetry data.
                    Telemetry.Current.TrackDependency("Azure DevOps", "TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, false);
                    Telemetry.Current.TrackException(
                            ex,
                            new Dictionary<string, string> {
                                { "CollectionUrl", _collectionUrl.ToString() },
                                { "TeamProjectName",  _teamProjectName}
                            },
                            new Dictionary<string, double> {
                                { "ConnectionTimer", connectionTimer.ElapsedMilliseconds }
                            }
                       );

                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Warning, ex.ErrorCode, $"[EXCEPTION] {ex}");
                    _mySource.Value.Flush();

                    throw;
                }
            }            
        }

        #endregion
    }
}