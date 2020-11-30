using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Core;
using VstsSyncMigrator.Engine.Configuration.Processing;
using Microsoft.Azure.ActiveDirectory.GraphClient;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemRevisionReplayMigrationContext : WorkItemMigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemRevisionReplayMigrationContext"));

        #endregion

        #region - Private Members

        private readonly WorkItemRevisionReplayMigrationConfig _config;

        private void PopulateWorkItemHistory(Revision sourceWIRevision, WorkItem targetWI)
        {
            // Set the 'ChangedBy' and 'History' fields with the values from the work item at the source.
            // The key value is the index of the revision in the array of all revisions for work item at the source.
            targetWI.Fields["System.ChangedBy"].Value = sourceWIRevision.Fields["System.ChangedBy"].Value;
            targetWI.Fields["System.History"].Value = sourceWIRevision.Fields["System.History"].Value;
        }

        private void PopulateWorkItemHistory(WorkItem targetWI, bool reprocessing)
        {
            // Add an entry in the history to explain this work item has been migrated.
            targetWI.Fields["System.ChangedBy"].Value = AuthorizedIdentityName;
            targetWI.Fields["System.History"].Value = Utility.GenerateMigrationGreeting(reprocessing);
        }

        private void ReplayRevisions(WorkItem sourceWI, WorkItem targetWI, Project targetProject, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore, bool reprocessing = false, int waitTime = 0)
        {
            // Initialize.
            bool isDirty = false;
            bool workItemTypeHasChanged;
            int discardRevisions;
            int totalRevisions = 0;
            int totalRevisionsToReplay = 0;
            double averageOpSpeed = 0;
            double waitTimeInSec = waitTime / 1000;
            string prefixTraceMessage;
            string sourceWIType;
            string targetWIType;
            string verb;
            WorkItem firstRevWI = null;
            WorkItem lastRevWI = null;
            WorkItem currentWI;
            WorkItem rwi;
            List<int> revisionDictKeys;
            Dictionary<int, WorkItem> wiRevisions = new Dictionary<int, WorkItem>();
            ArrayList failFields;
            Stopwatch replayWIRevisionsTimer = new Stopwatch();

            // Set prefix for trace message.
            prefixTraceMessage = $"Work Item {sourceWI.Id}";

            try
            {
                // Start timer.
                replayWIRevisionsTimer.Start();

                // Set the right verb for traces.
                if (reprocessing)
                    verb = "Migrate/Reprocess";
                else
                    verb = "Migrate";

                #region - Extract and sort work item revisions.

                // Just to make sure, we replay the events in the same order as they appeared
                // maybe, the Revisions collection is not sorted according to the actual Revision number
                var sortedRevisions = sourceWI.Revisions.Cast<Revision>().Select(x => new { x.Index, Number = Convert.ToInt32(x.Fields["System.Rev"].Value) })
                    .OrderBy(x => x.Number)
                    .ToList();

                // Get the number of revisions to replay.
                totalRevisions = sortedRevisions.Count;

                if (totalRevisions == 1)
                {
                    workItemTypeHasChanged = false;

                    // Get the source work item type at last revision.
                    sourceWIType = sourceWI.Type.Name;
                }
                else
                {
                    // Get first and last revision of the work item.
                    firstRevWI = sourceStore.GetRevision(sourceWI, sortedRevisions[0].Number);
                    lastRevWI = sourceStore.GetRevision(sourceWI, sortedRevisions[sortedRevisions.Count - 1].Number);

                    // Verify if the work item type has changed between first and last revisions.
                    workItemTypeHasChanged = (firstRevWI.Type.Name != lastRevWI.Type.Name);

                    // Get the source work item type at last revision.
                    sourceWIType = lastRevWI.Type.Name;
                }

                #endregion

                // Validate if the source work item type at last revision is part of the supported work item type definition.
                if (Engine.TestIfSupportedWorkItemTypeDefinition(sourceWIType))
                {
                    #region - Identify work item revisions to keep.

                    // If it has changed grab only the revision with this type.
                    if (workItemTypeHasChanged)
                    {
                        // Copy the list and reverse sorting to get the most recent revision first.
                        var backwardSortedRevisions = sortedRevisions.ToList();
                        backwardSortedRevisions.Reverse();

                        // Get all revisions.
                        foreach (var revision in backwardSortedRevisions)
                        {
                            // Get work item revision.
                            rwi = sourceStore.GetRevision(sourceWI, revision.Number);

                            if (Engine.TestIfSupportedWorkItemTypeDefinition(rwi.Type.Name))
                            {
                                // Care only if the work item revision type is the same as the
                                // most recent work item type.
                                if (rwi.Type.Name == lastRevWI.Type.Name)
                                {
                                    // Add to revisions list to replay.
                                    wiRevisions.Add(revision.Index, rwi);

                                    // Increment number of revision to replay.
                                    totalRevisionsToReplay++;
                                }
                            }
                        }
                    }
                    // Grab all revisions except the ones with unsupported types.
                    else
                    {
                        // Get all revisions.
                        foreach (var revision in sortedRevisions)
                        {
                            // Get work item revision.
                            rwi = sourceStore.GetRevision(sourceWI, revision.Number);

                            if (Engine.TestIfSupportedWorkItemTypeDefinition(rwi.Type.Name))
                            {
                                // Add to revisions list to replay.
                                wiRevisions.Add(revision.Index, rwi);

                                // Increment number of revision to replay.
                                totalRevisionsToReplay++;
                            }
                        }
                    }

                    // Send some traces.
                    discardRevisions = totalRevisions - totalRevisionsToReplay;
                    _mySource.Value.TraceInformation($"{prefixTraceMessage} - {verb} {totalRevisionsToReplay} of the {totalRevisions} revisions found?");
                    _mySource.Value.TraceInformation($"{prefixTraceMessage} - {discardRevisions} will be discarded because of the type change or an unsupported type");

                    // Sort the list of keys to read the revision from less recent to most recent ones.
                    revisionDictKeys = wiRevisions.Keys.ToList();
                    revisionDictKeys.Sort();

                    #endregion

                    #region - Replay each revision.

                    // Browse each revision.
                    foreach (int key in revisionDictKeys)
                    {
                        // Initialize.
                        currentWI = wiRevisions[key];      // Get current revision.

                        // Get the work item type from the source work item at last revision.
                        sourceWIType = currentWI.Type.Name;

                        // Get the work item type from that revision.
                        targetWIType = Engine.WorkItemTypeDefinitions[sourceWIType].DiscreteMapValue;

                        // Modify an existing work item revision...
                        if (reprocessing || targetWI != null)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Found work item: {targetWI.Id} from target project");
                            _mySource.Value.TraceInformation($"Field: System.WorkItemType: {targetWI.Fields["System.WorkItemType"].Value.ToString()}");
                            _mySource.Value.TraceInformation($"Field: System.Title: {targetWI.Fields["System.Title"].Value.ToString()}");
                            _mySource.Value.TraceInformation($"Field: System.State: {targetWI.Fields["System.State"].Value.ToString()}");
                            _mySource.Value.TraceInformation($"Field: System.CreatedBy: {targetWI.Fields["System.CreatedBy"].Value.ToString()}");
                            _mySource.Value.TraceInformation($"Field: System.CreatedDate: {targetWI.Fields["System.CreatedDate"].Value.ToString()}");
                            _mySource.Value.TraceInformation($"Field: {Engine.ReflectedWorkItemIdFieldName}: {targetWI.Fields[Engine.ReflectedWorkItemIdFieldName].Value.ToString()}");
                            _mySource.Value.TraceInformation("Open this work item");

                            // Open work item.
                            targetWI.Open();
                        }
                        // Create a new work item...
                        else if (targetWI == null)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Create a work item ({targetWIType}) into target project");

                            // Create a new work item.
                            targetWI = targetProject.WorkItemTypes[targetWIType].NewWorkItem();

                            // Set the 'CreatedBy' and 'CreatedDate' fields with the values from the work item at the source at revision 1.
                            targetWI.Fields["System.CreatedBy"].Value = currentWI.Revisions[0].Fields["System.CreatedBy"].Value;
                            targetWI.Fields["System.CreatedDate"].Value = currentWI.Revisions[0].Fields["System.CreatedDate"].Value;
                        }

                        // Populate a revision of work item for the first time or perform reprocessing.
                        PopulateWorkItem(currentWI, targetWI, reprocessing);

                        // Apply field mappings for that revision of work item.
                        Engine.ApplyFieldMappings(currentWI, targetWI);

                        // Populate history.
                        PopulateWorkItemHistory(currentWI.Revisions[key], targetWI);

                        // Validate the target work item to see if any problems arise.
                        failFields = targetWI.Validate();

                        // Send some traces.
                        if (failFields.Count > 0)
                            foreach (Field f in failFields)
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Invalid: {targetWI.Id}-{targetWIType}-{targetWI.Title} Status: {f.Status} Field: {f.ReferenceName} Value: {f.Value}");

                        // Save work item revision.
                        targetWI.Save();
                        isDirty = true;
                    }

                    #endregion

                    #region - Set last revision.

                    // Finalize operation only if the work item has been modified.
                    if (isDirty)
                    {
                        // If the target work item contains already the reflected work item id field,
                        // modify it with the new value and set the 'ChangedBy' field with the authorized identity name for target project.
                        if (targetWI.Fields.Contains(Engine.ReflectedWorkItemIdFieldName))
                            targetWI.Fields[Engine.ReflectedWorkItemIdFieldName].Value = sourceStore.GenerateReflectedWorkItemId(sourceWI);

                        // Populate history.
                        PopulateWorkItemHistory(targetWI, reprocessing);

                        // Save work item new revision which will contain the reflected work item id and the migration greeting in the history.
                        targetWI.Save();

                        // Close target work item.
                        targetWI.Close();

                        // Send some traces.
                        _mySource.Value.TraceInformation($"{prefixTraceMessage} - Work item saved: {targetWI.Id}");

                        // Update the source reflected work item id if it has been defined to update it.
                        // Is the field defined on work item at the source.
                        if (_config.UpdateSourceReflectedId && sourceWI.Fields.Contains(Engine.SourceReflectedWorkItemIdFieldName))
                        {
                            // Write the value to source reflected work item id field.
                            sourceWI.Fields[Engine.SourceReflectedWorkItemIdFieldName].Value = targetStore.GenerateReflectedWorkItemId(targetWI);

                            // Save source work item.
                            sourceWI.Save();
                        }
                    }

                    #endregion

                    #region - Self-Throttling.

                    // Slow the process if needed.
                    if (waitTime > 0)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"{prefixTraceMessage} - To prevent execution throttling, add {waitTime} milliseconds to wait");

                        // Impose a wait time to process.
                        Thread.Sleep(waitTime);
                    }

                    #endregion
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"{prefixTraceMessage} - WITD: {sourceWIType} is not in the list provided in the configuration.json under WorkItemTypeDefinitions. Add it to the list to enable migration of this work item type.");
                }
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"{prefixTraceMessage} - Failed to save - {Name}: {ex.Message}");
            }

            // Stop timer.
            replayWIRevisionsTimer.Stop();

            // Calculate average operation speed without the artificially imposed delay operation.
            averageOpSpeed = (replayWIRevisionsTimer.Elapsed.TotalSeconds - waitTimeInSec) / totalRevisionsToReplay;

            // Send some traces.
            _mySource.Value.TraceInformation($"{prefixTraceMessage} - {totalRevisionsToReplay} revisions replayed in " + @"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds. Artificially imposed delay was {1:f2} seconds. Average speed is {2:f2} second/revision replay", replayWIRevisionsTimer.Elapsed, waitTimeInSec, averageOpSpeed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Protected Members

        protected override void PopulateWorkItem(WorkItem sourceWI, WorkItem targetWI, bool reprocessing = false)
        {
            // Initialize.
            bool isTestCaseWI;
            List<string> ignoreFields;
            Stopwatch populateOneWITimer = new Stopwatch();

            // Evaluate if it is a test case work item.
            isTestCaseWI = (targetWI.Type.Name == "Test Case");

            // Send some traces.
            if (targetWI.Id == 0)
                _mySource.Value.TraceInformation($"Populating work item ({targetWI.Type.Name}) - ???");
            else
                _mySource.Value.TraceInformation($"Populating work item ({targetWI.Type.Name}) - {targetWI.Id}");

            // Start timer.
            populateOneWITimer.Start();

            // Get the fields to ignore.
            if (reprocessing)
                ignoreFields = IgnoreFieldsWhenReprocessing;
            else
                ignoreFields = IgnoreFields;

            // Start timer.
            populateOneWITimer.Start();

            // Set all fields that exist from the source work item to target work item.
            // Some exceptions are handled outside this method such as the history.
            foreach (Field f in sourceWI.Fields)
            {
                if (targetWI.Fields.Contains(f.ReferenceName))
                {
                    // Send some traces.
                    if (ignoreFields.Contains(f.ReferenceName))
                        _mySource.Value.TraceInformation($"Field: {f.ReferenceName} will be ignored");
                    else
                    {
                        if (targetWI.Fields[f.ReferenceName].IsEditable)
                        {
                            // Rebuild the area path for this target work item.
                            if (f.ReferenceName == "System.AreaPath")
                            {
                                string nodePath = SetTargetNodePath(_config.AreaPath, _config.IterationPath, _config.PrefixProjectToNodes, _config.PrefixPath, sourceWI.AreaPath, sourceWI.Project.Name, targetWI.Project.Name, targetWI.Store, "Area",
                                    _config.IsClone);
                                targetWI.Fields[f.ReferenceName].Value = nodePath;

                                if (_config.UseAreaPathMap)
                                {
                                    string mappedAreaPath = null;
                                    try
                                    {
                                        mappedAreaPath = _config.FullAreaPathMap.GetMappedPath(sourceWI.AreaPath, true, Core.BusinessEntities.Constants.AreaStructureType);
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        string errorMessage = $"Unable to map Area Path {sourceWI.AreaPath} for work item {sourceWI.Id} of type {sourceWI.Type.Name}: {ex.Message}";
                                        // Send some traces.
                                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMessage);
                                        _mySource.Value.Flush();
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        string errorMessage = $"Unable to map Area Path {sourceWI.AreaPath} for work item {sourceWI.Id} of type {sourceWI.Type.Name}: {ex.Message}";
                                        // Send some traces.
                                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMessage);
                                        _mySource.Value.Flush();
                                        throw;
                                    }
                                    if (mappedAreaPath != nodePath)
                                    {
                                        //need to ensure that remapping works i.e. Area path exists before proceeding
                                        throw new NotImplementedException();
                                    }
                                }

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Field: {f.ReferenceName} will be set with: {nodePath}");
                            }
                            // Rebuild the iteration path for this target work item.
                            else if (f.ReferenceName == "System.IterationPath")
                            {
                                string nodePath = SetTargetNodePath(_config.AreaPath, _config.IterationPath, _config.PrefixProjectToNodes, _config.PrefixPath, sourceWI.IterationPath, sourceWI.Project.Name, targetWI.Project.Name, targetWI.Store, "Iteration",
                                    _config.IsClone);
                                targetWI.Fields[f.ReferenceName].Value = nodePath;

                                if (_config.UseIterationPathMap)
                                {
                                    string mappedIterationPath = null;
                                    try
                                    {
                                        mappedIterationPath = _config.FullIterationPathMap.GetMappedPath(sourceWI.IterationPath, true, Core.BusinessEntities.Constants.IterationStructureType);
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        string errorMessage = $"Unable to map Iteration Path {sourceWI.IterationPath} for work item {sourceWI.Id} of type {sourceWI.Type.Name}: {ex.Message}";
                                        // Send some traces.
                                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMessage);
                                        _mySource.Value.Flush();
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        string errorMessage = $"Unable to map Iteration Path {sourceWI.IterationPath} for work item {sourceWI.Id} of type {sourceWI.Type.Name}: {ex.Message}";
                                        // Send some traces.
                                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, errorMessage);
                                        _mySource.Value.Flush();
                                        throw;
                                    }
                                    if (mappedIterationPath != nodePath)
                                    {
                                        //need to ensure that remapping works i.e. Iteration path exists before proceeding
                                        throw new NotImplementedException();
                                    }
                                }

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Field: {f.ReferenceName} will be set with: {nodePath}");
                            }
                            else
                            {
                                targetWI.Fields[f.ReferenceName].Value = sourceWI.Fields[f.ReferenceName].Value;

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Field: {f.ReferenceName} will be set with: {sourceWI.Fields[f.ReferenceName].Value}");
                            }
                        }
                        // Send some traces.
                        else
                            _mySource.Value.TraceInformation($"Field: {f.ReferenceName} is not editable");
                    }
                }
                // Send some traces.
                else
                    _mySource.Value.TraceInformation($"Field: {f.ReferenceName} does not exist");
            }

            // If the work item type is 'Test Case', some extra fields must be added/set.
            if (isTestCaseWI)
            {
                targetWI.Fields["Microsoft.VSTS.TCM.Steps"].Value = sourceWI.Fields["Microsoft.VSTS.TCM.Steps"].Value;
                targetWI.Fields["Microsoft.VSTS.Common.Priority"].Value = sourceWI.Fields["Microsoft.VSTS.Common.Priority"].Value;
            }

            // Set the backlog priority for this target work item.
            if (targetWI.Fields.Contains("Microsoft.VSTS.Common.BacklogPriority")
                && targetWI.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value != null
                && !Utility.ValidateIfNumeric(targetWI.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value.ToString(), NumberStyles.Any))
                targetWI.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value = 10;

            // Set the description field for this target work item.
            targetWI.Description = sourceWI.Description;

            // Stop timer.
            populateOneWITimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation("Populated in {0} seconds", populateOneWITimer.Elapsed.TotalSeconds.ToString());
        }

        #endregion

        #region - Internal Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0039:Use local function", Justification = "Prefer to use an action instead of local function")]
        internal override void InternalExecute()
        {
            // Initialize.
            bool generateSlowingFactor;
            int revisionCount = 0;
            int totalRevisions = 0;
            UInt32 slowingFactor = 0;
            UInt32 minSlowingFactor = 0;
            UInt32 maxSlowingFactor = 0;
            List<object[]> listOfActionDelegateParameters = new List<object[]>();

            // Start internal execution timer.
            InternalExecutionTimer.Start();

            // Start query timer.
            QueryTimer.Start();

            // Get source work item store.
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(Engine.Source, WorkItemStoreFlags.BypassRules);

            // Create a new query context.
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Source.Name);

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id]", _config.QueryBit);

            // Send some traces.
            _mySource.Value.TraceInformation("Query database");
            _mySource.Value.Flush();

            // Execute.
            WorkItemCollection sourceWIs = tfsqc.Execute();

            // Stop timer.
            QueryTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation("Time to query database: {0} seconds", QueryTimer.Elapsed.TotalSeconds.ToString());
            _mySource.Value.Flush();

            // Count the total number of revisions.
            foreach (WorkItem sourceWI in sourceWIs.Cast<WorkItem>())
            {
                // Reset flag.
                generateSlowingFactor = false;

                // Count the number of revisions in each work item.
                // Use the revision number instead of the .Count property which will trigger a call
                // behind the scene.
                revisionCount = sourceWI.Rev;

                // Sum all revisions to get the full total.
                totalRevisions += revisionCount;

                // Adjust slowing factors based on number of revisions to replay.
                if (revisionCount < 10)
                    slowingFactor = 0;
                else if (revisionCount >= 10 && revisionCount < 25)
                {
                    generateSlowingFactor = true;
                    minSlowingFactor = 3000;
                    maxSlowingFactor = 5000;
                }
                else if (revisionCount >= 25 && revisionCount < 50)
                {
                    generateSlowingFactor = true;
                    minSlowingFactor = 5000;
                    maxSlowingFactor = 7500;
                }
                else if (revisionCount >= 75 && revisionCount < 100)
                {
                    generateSlowingFactor = true;
                    minSlowingFactor = 7500;
                    maxSlowingFactor = 12000;
                }
                else
                {
                    generateSlowingFactor = true;
                    minSlowingFactor = 12000;
                    maxSlowingFactor = 60000;
                }

                // Generate a slowing factor - from x ms to y ms.
                if (generateSlowingFactor)
                    slowingFactor = Utility.GenerateTrueRandomInteger(minSlowingFactor, maxSlowingFactor);

                // Generate the list of all parameters to pass inside the action delegate.
                object[] actionDelegateParam = new object[] { sourceWI, slowingFactor };

                // Add to list of parameters.
                listOfActionDelegateParameters.Add(actionDelegateParam);
            }

            // Send some traces.
            _mySource.Value.TraceInformation($"Replay {totalRevisions} revisions found within {sourceWIs.Count} work items?");

            // Generate a context for work items at target.
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);

            // Access to authorized identity to target collection/project.
            TeamFoundationIdentity tfi = ((TeamProjectContext)Engine.Target).AuthorizedIdentity;
            if (string.IsNullOrEmpty(tfi.DisplayName))
                AuthorizedIdentityName = tfi.UniqueName;
            else
                AuthorizedIdentityName = tfi.DisplayName;

            // Find target project.
            Project targetProject = targetStore.GetProject();

            // Send some traces.
            _mySource.Value.TraceInformation($"Found target project as {targetProject.Name}");
            _mySource.Value.Flush();

            // Set a thread-safe collection of objects to retain all failed work items.
            ConcurrentBag<WorkItem> failedWIs = new ConcurrentBag<WorkItem>();

            // Create the action delegate to be called in the foreach in parallel.
            Action<object[]> replayRevisionForWI = (actionDelegateParameters) =>
            {
                // Initialize.
                bool reprocessing;
                WorkItem sourceWI = (WorkItem)actionDelegateParameters[0];
                int sf = Convert.ToInt32(actionDelegateParameters[1]);      // Convert UInt32 to int32.
                string prefixTraceMessage = $"Work Item {sourceWI.Id}";

                try
                {
                    // Initialize.
                    WorkItem targetWI = null;

                    // Try to find if the work item at target contains already the reflected work item id.
                    targetWI = targetStore.FindReflectedWorkItemByReflectedWorkItemId(sourceWI, Engine.ReflectedWorkItemIdFieldName);

                    // Proceed only if the work item does not exist at target or the force option is enabled.
                    if (targetWI == null || (targetWI != null && _config.Force))
                    {
                        // Evaluate if it is reprocessing of an existing work item.
                        reprocessing = (targetWI != null && _config.Force);

                        // Replay all revisions of a work item.
                        ReplayRevisions(sourceWI, targetWI, targetProject, sourceStore, targetStore, reprocessing, sf);

                        // Send some traces.
                        _mySource.Value.TraceInformation($"{prefixTraceMessage} - {sourceWI.Type.Name}: Completed");
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"{prefixTraceMessage} - {sourceWI.Type.Name}: Exists");
                    }

                    // Close source work item.
                    sourceWI.Close();
                }
                catch (Exception ex)
                {
                    failedWIs.Add(sourceWI);

                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"{prefixTraceMessage} - Revision Replay: Failed with error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                }
                finally
                {
                    // Flush traces.
                    _mySource.Value.Flush();
                }
            };

            // Replay revisions in parallel.
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = _config.DegreeOfParallelism };
            Parallel.ForEach<object[]>(listOfActionDelegateParameters.ToList(), options, replayRevisionForWI);

            // Send some traces.
            _mySource.Value.TraceInformation("Errors in work items:");
            foreach (WorkItem failed in failedWIs)
                _mySource.Value.TraceInformation($"{failed.Id}");

            // Stop timer.
            InternalExecutionTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"All work item revision replays done in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", InternalExecutionTimer.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "WorkItemRevisionReplayMigrationContext"; }
        }

        public WorkItemRevisionReplayMigrationContext(MigrationEngine me, WorkItemRevisionReplayMigrationConfig config) : base(me)
        {
            // Initialize.
            List<string> l = new List<string>();

            // Save the configuration.
            _config = config;

            // Add fields to ignore when reprocessing.
            if (_config.Force)
            {
                if (_config.KeepState)
                    l.Add("System.State");

                if (_config.KeepAreaPath)
                    l.Add("System.AreaPath");

                if (_config.KeepIterationPath)
                    l.Add("System.IterationPath");

                // Add ignore fields when reprocessing.
                if (l.Count > 0)
                    AddIgnoreFieldWhenReprocessing(l);
            }
        }

        #endregion
    }
}
