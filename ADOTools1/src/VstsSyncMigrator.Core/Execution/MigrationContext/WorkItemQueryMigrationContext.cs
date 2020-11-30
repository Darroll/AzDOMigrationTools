using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemQueryMigrationContext : MigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemQueryMigrationContext"));

        #endregion

        #region - Private Members

        /// <summary>
        /// Counter for folders processed
        /// </summary>
        private int _totalFoldersAttempted = 0;

        /// <summary>
        /// Counter for queries skipped
        /// </summary>
        private int _totalQueriesSkipped = 0;

        /// <summary>
        /// Counter for queries attempted
        /// </summary>
        private int _totalQueriesAttempted = 0;

        /// <summary>
        /// Counter for queries successfully migrated
        /// </summary>
        private int _totalQueriesMigrated = 0;

        /// <summary>
        /// Counter for the queries that failed migration
        /// </summary>
        private int _totalQueryFailed = 0;

        /// <summary>
        /// The processor configuration
        /// </summary>
        private readonly WorkItemQueryMigrationConfig _config;

        /// <summary>
        /// Define Query Folders under the current parent
        /// </summary>
        /// <param name="targetQH">The object that represents the whole of the target query tree</param>
        /// <param name="sourceQF">The source folder in tree on source instance</param>
        /// <param name="parentQF">The target folder in tree on target instance</param>
        private void MigrateFolder(QueryHierarchy targetQH, QueryFolder sourceQF, QueryFolder parentQF)
        {
            try
            {
                // We only migrate non-private folders and their contents
                if (sourceQF.IsPersonal)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Found a personal folder {sourceQF.Name}. Migration only available for shared Team Query folders");
                    _mySource.Value.Flush();
                }
                else
                {                    
                    // Increment the number of total folders attempted.
                    _totalFoldersAttempted++;

                    // We need to replace the team project name in folder names as it included in query paths.
                    string requiredPath = sourceQF.Path.Replace($"{Engine.Source.Name}/", $"{Engine.Target.Name}/");

                    // Is the project name to be used in the migration as an extra folder level?
                    if (
                        _config.PrefixProjectToNodes
                        || (!_config.PrefixProjectToNodes && (!string.IsNullOrEmpty(_config.PrefixPath)))// != null))
                        )
                    {
                        // we need to inject the team name as a folder in the structure
                        if (!string.IsNullOrEmpty(_config.PrefixPath))// != null)
                            requiredPath = requiredPath.Replace(_config.SharedFolderName, $"{_config.SharedFolderName}/{_config.PrefixPath.Replace('\\', '/')}/{Engine.Source.Name}");
                        else
                            requiredPath = requiredPath.Replace(_config.SharedFolderName, $"{_config.SharedFolderName}/{Engine.Source.Name}");

                        if (_config.IsClone)
                        {
                            var mayNeed = "to do something";
                        }

                        // If on the root level we need to check that the extra folder has already been added
                        if (sourceQF.Path.Count(x => x == '/') == 1)
                        {
                            QueryFolder targetSharedFolderRoot = (QueryFolder)parentQF[_config.SharedFolderName];
                            QueryFolder extraFolder = (QueryFolder)targetSharedFolderRoot.FirstOrDefault(x => x.Path == requiredPath);

                            if (extraFolder == null)
                            {
                                // Initialize.
                                string path;

                                // we are at the root level on the first pass and need to create the extra folder for the team name

                                // Generate the new path for query.
                                if (_config.PrefixPath != null)
                                    path = $"{_config.PrefixPath.Replace('\\', '/')}/{Engine.Source.Name}";
                                else
                                    path = Engine.Source.Name;

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Adding a folder '{path}'");
                                _mySource.Value.Flush();

                                // Save changes.
                                targetSharedFolderRoot = AddFolders(targetSharedFolderRoot, path);
                                targetQH.Save(); // moved the save here a more immediate and relavent error message
                            }

                            // Adjust the working folder to the newly added one.
                            parentQF = targetSharedFolderRoot;
                        }
                    }

                    // Check if there is a folder of the required name, using the path to make sure it is unique.
                    QueryFolder targetFolder = (QueryFolder)parentQF.FirstOrDefault(q => q.Path == requiredPath);
                    if (targetFolder != null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Skipping folder '{sourceQF.Name}' as it already exists");
                        _mySource.Value.Flush();
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Migrating a folder '{sourceQF.Name}'");
                        _mySource.Value.Flush();

                        // Create the query folder.
                        targetFolder = new QueryFolder(sourceQF.Name);

                        // Add the folder under the parent folder.
                        parentQF.Add(targetFolder);
                        targetQH.Save(); // moved the save here a more immediate and relavent error message
                    }

                    // Process child items
                    foreach (QueryItem subquery in sourceQF)
                    {
                        if (subquery.GetType() == typeof(QueryFolder))
                            MigrateFolder(targetQH, (QueryFolder)subquery, (QueryFolder)targetFolder);
                        else
                            MigrateQuery(targetQH, (QueryDefinition)subquery, (QueryFolder)targetFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
            }
        }

        private QueryFolder AddFolders(QueryFolder targetSharedQFRoot, string path)
        {
            var pathPieces = path.Split(new char[] { '/' });

            QueryFolder extraFolder = null;
            QueryFolder currentRoot = targetSharedQFRoot;
            QueryFolder previousRoot = targetSharedQFRoot;
            string pathSoFar = targetSharedQFRoot.Path;
            foreach (var piece in pathPieces)
            {
                previousRoot = currentRoot;
                pathSoFar += $"/{piece}";
                //check if folder exists
                extraFolder = (QueryFolder)currentRoot.FirstOrDefault(x => x.Path == pathSoFar);
                if (extraFolder == null)
                {
                    extraFolder = new QueryFolder(piece);
                    currentRoot.Add(extraFolder);
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Skipping folder '{extraFolder.Path}' as already exists");
                    _mySource.Value.Flush();
                }
                currentRoot = extraFolder;
            }
            return previousRoot;
        }

        /// <summary>
        /// Add Query Definition under a specific Query Folder.
        /// </summary>
        /// <param name="targetQH">The object that represents the whole of the target query tree</param>
        /// <param name="query">Query Definition - Contains the Query Details</param>
        /// <param name="QueryFolder">Parent Folder</param>
        private void MigrateQuery(QueryHierarchy targetQH, QueryDefinition query, QueryFolder parentQF)
        {
            if (parentQF.FirstOrDefault(q => q.Name == query.Name) != null)
            {
                _totalQueriesSkipped++;

                // Send some traces.
                _mySource.Value.TraceInformation($"Skipping query '{query.Name}' as already exists");
                _mySource.Value.Flush();
            }
            else
            {
                // Sort out any path issues in the queryText
                string fixedQueryText = query.QueryText.Replace($"'{Engine.Source.Name}", $"'{Engine.Target.Name}");  // the ' should only items at the start of areapath etc.

                if (_config.PrefixProjectToNodes)
                {
                    // we need to inject the team name as a folder in the structure too
                    if (_config.PrefixPath != null)
                        fixedQueryText = fixedQueryText.Replace($"{Engine.Target.Name}\\", $"{Engine.Target.Name}\\{_config.PrefixPath}\\{Engine.Source.Name}\\");
                    else
                        fixedQueryText = fixedQueryText.Replace($"{Engine.Target.Name}\\", $"{Engine.Target.Name}\\{Engine.Source.Name}\\");
                }

                // you cannot just add an item from one store to another, we need to create a new object
                QueryDefinition queryCopy;
                _totalQueriesAttempted++;

                // Send some traces.
                _mySource.Value.TraceInformation($"Migrating query '{query.Name}'");
                _mySource.Value.Flush();

                try
                {
                    queryCopy = new QueryDefinition(query.Name, fixedQueryText);
                    parentQF.Add(queryCopy);

                    try
                    {
                        targetQH.Save(); // moved the save here for better error message
                        _totalQueriesMigrated++;
                    }
                    catch (Exception ex)
                    {
                        _totalQueryFailed++;

                        // Send some traces.
                        _mySource.Value.TraceInformation($"Error saving query '{query.Name}', probably due to invalid area or iteration paths");
                        _mySource.Value.TraceInformation(ex.Message);
                        _mySource.Value.Flush();

                        targetQH.Refresh(); // get the tree without the last edit
                    }
                }
                catch (Exception ex)
                {
                    _totalQueryFailed++;

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Error copying query '{query.Name}', probably due to inability to parse the query text or fix it");
                    _mySource.Value.TraceInformation(ex.Message);
                    _mySource.Value.Flush();

                    targetQH.Refresh(); // get the tree without the last edit
                }

            }
        }

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Initialize.
            Stopwatch migrationOpTimer = new Stopwatch();

            // Start timer. 
            migrationOpTimer.Start();

            // Generate the source and target context.
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(Engine.Source, WorkItemStoreFlags.None);
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.None);

            // Define the Query Hiercharchy client for both source and target.
            QueryHierarchy sourceQueryHierarchy = sourceStore.Store.Projects[Engine.Source.Name].QueryHierarchy;
            QueryHierarchy targetQueryHierarchy = targetStore.Store.Projects[Engine.Target.Name].QueryHierarchy;

            // Send some traces.
            _mySource.Value.TraceInformation("Found {0} root level child WIQ folders", sourceQueryHierarchy.Count);
            _mySource.Value.Flush();

            foreach (QueryFolder query in sourceQueryHierarchy)
                MigrateFolder(targetQueryHierarchy, query, targetQueryHierarchy);

            // Stop timer.
            migrationOpTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation($"Folders scanned {_totalFoldersAttempted}");
            _mySource.Value.TraceInformation($"Queries Found:{_totalQueriesAttempted}  Skipped:{_totalQueriesSkipped}  Migrated:{_totalQueriesMigrated}   Failed:{_totalQueryFailed}");
            _mySource.Value.TraceInformation(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", migrationOpTimer.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "WorkItemQueryMigrationProcessorContext"; }
        }

        public WorkItemQueryMigrationContext(MigrationEngine me, WorkItemQueryMigrationConfig config) : base(me)
        {
            _config = config;
        }

        #endregion
    }
}