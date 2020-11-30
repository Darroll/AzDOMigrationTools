using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Core;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemMigrationContext : WorkItemMigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.WorkItemMigrationContext"));

        #endregion

        #region - Private Members

        private readonly WorkItemMigrationConfig _config;

        private void PopulateWorkItemHistory(WorkItem sourceWI, WorkItem targetWI, bool reprocessing = false)
        {
            // Initialize.
            bool hasHistory;
            string paragraphTemplate = "<p>{0}</p>";
            string commentsFromPreviousRevisionsTags = string.Format(paragraphTemplate, "Changes from previous revisions:");
            string fieldValuesFromPreviousRevisionsTags = string.Format(paragraphTemplate, "Field values from previous revisions:");
            string tableSTag = "<table border='1' style='width:100%;border-color:#C0C0C0;'>";
            string tableETag = "</table>";
            string emptyParagraphTags = string.Format(paragraphTemplate, "&nbsp;");
            string tableRowTags = "<tr><td style='align:right;width:100%'><p><b>{0} on {1}:</b></p><p>{2}</p></td></tr>";
            string changedDateAsString;
            string greetingTags = string.Format(paragraphTemplate, Utility.GenerateMigrationGreeting(reprocessing));
            StringBuilder historySB = new StringBuilder();

            // Generate the comment table.
            if (sourceWI.Revisions != null && sourceWI.Revisions.Count > 0)
            {
                // Create a html table to show all changes.
                historySB.Append(commentsFromPreviousRevisionsTags);
                historySB.Append(tableSTag);

                foreach (Revision r in sourceWI.Revisions)
                {
                    // Evaluate if it has history.
                    hasHistory = !string.IsNullOrEmpty((string)r.Fields["System.History"].Value);

                    if (hasHistory)
                    {
                        // Open work item.
                        r.WorkItem.Open();

                        // Get changed date.
                        changedDateAsString = DateTime.Parse(r.Fields["System.ChangedDate"].Value.ToString()).ToLongDateString();

                        // Append change done to a field.
                        historySB.AppendFormat(tableRowTags, r.Fields["System.ChangedBy"].Value.ToString(), changedDateAsString, r.Fields["System.History"].Value.ToString());
                    }
                }

                // Finalize table and add an empty paragraph.
                historySB.Append(tableETag);
                historySB.Append(emptyParagraphTags);
            }

            // Add a new html paragraph.
            historySB.Append(fieldValuesFromPreviousRevisionsTags);

            // Browse each fields.
            foreach (Field f in sourceWI.Fields)
            {
                if (f.Value == null)
                    historySB.AppendLine($"{f.Name}: null<br/>");
                else
                    historySB.AppendLine($"{f.Name}: {f.Value.ToString()}<br/>");
            }

            // Add an empty paragraph.
            historySB.Append(emptyParagraphTags);

            // Append the signature footer.
            historySB.Append(greetingTags);

            // Set the new history.
            targetWI.History = historySB.ToString();
        }

        #endregion

        #region - Protected Members

        protected override void PopulateWorkItem(WorkItem sourceWI, WorkItem targetWI, bool reprocessing = false)
        {
            // Initialize.
            bool isTestCaseWI;
            List<string> ignoreFields;

            // Evaluate if it is a test case work item.
            isTestCaseWI = (targetWI.Type.Name == "Test Case");

            // Send some traces.
            if (targetWI.Id == 0)
                _mySource.Value.TraceInformation($"Populating work item ({targetWI.Type.Name}) - ???");
            else
                _mySource.Value.TraceInformation($"Populating work item ({targetWI.Type.Name}) - {targetWI.Id}");

            // Get the fields to ignore.
            if (reprocessing)
                ignoreFields = IgnoreFieldsWhenReprocessing;
            else
                ignoreFields = IgnoreFields;

            // Start timer.
            PopulateOperationTimer.Start();

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
                        if(targetWI.Fields[f.ReferenceName].IsEditable)
                        {
                            // Rebuild the area path for this target work item.
                            if (f.ReferenceName == "System.AreaPath")
                            {
                                string nodePath = SetTargetNodePath(_config.AreaPath, _config.IterationPath, _config.PrefixProjectToNodes, _config.PrefixPath, sourceWI.AreaPath, sourceWI.Project.Name, targetWI.Project.Name, targetWI.Store, "Area",
                                    _config.IsClone);
                                targetWI.Fields[f.ReferenceName].Value = nodePath;

                                if (_config.UseAreaPathMap)
                                {
                                    throw new NotImplementedException();
                                }

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Field: {f.ReferenceName} will be set with: {nodePath}");
                            }
                            // Rebuild the iteration path for this target work item.
                            else if (f.ReferenceName == "System.IterationPath")
                            {
                                string nodePath = SetTargetNodePath(_config.AreaPath, _config.IterationPath, _config.PrefixProjectToNodes, _config.PrefixPath, sourceWI.AreaPath, sourceWI.Project.Name, targetWI.Project.Name, targetWI.Store, "Iteration",
                                    _config.IsClone);
                                targetWI.Fields[f.ReferenceName].Value = nodePath;

                                if (_config.UseIterationPathMap)
                                {
                                    throw new NotImplementedException();
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
            PopulateOperationTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation("Populated in {0} seconds", PopulateOperationTimer.Elapsed.TotalSeconds.ToString());
            _mySource.Value.Flush();
        }        

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Initialize.
            bool reprocessing;
            string averageTimeAsString;
            string timeToCompletion;
            string sourceWIType;
            string targetWIType;
            string verb;
            ArrayList failFields;
            TimeSpan averageTimespan;
            TimeSpan remainingTimespan;
            WorkItem targetWI;

            // Set counters.
            int totalWIs;
            int remainingWIs;
            int processedWIs = 0;
            int failureWIs = 0;
            int importedWIs = 0;
            int skippedWIs = 0;

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

            // Set the query to get all work item identifiers for a given project.
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id]", _config.QueryBit);

            // Send some traces.
            _mySource.Value.TraceInformation("Query database");
            _mySource.Value.Flush();

            // Execute.
            WorkItemCollection sourceWIs = tfsqc.Execute();

            // Stop timer.
            QueryTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation("Time to query database: {0} seconds", QueryTimer.Elapsed.TotalSeconds.ToString());

            // Count the number of WIs found.
            totalWIs = sourceWIs.Count;
            
            // Set the remaining counter.
            remainingWIs = totalWIs;

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
            _mySource.Value.TraceInformation($"Target project: {targetProject.Name}");
            _mySource.Value.TraceInformation($"Migrate/Replay last revision found with {totalWIs} work items");
            _mySource.Value.Flush();

            // Browse each work item.
            foreach (WorkItem sourceWI in sourceWIs)
            {
                // Reset and start.
                ReplayWITimer.Reset();
                ReplayWITimer.Start();

                // Get the work item type from the source work item at last revision.
                sourceWIType = sourceWI.Type.Name;

                // Increment counter.
                processedWIs++;

                // Try to find if the work item at target contains already the reflected work item id.
                targetWI = targetStore.FindReflectedWorkItem(sourceWI, Engine.ReflectedWorkItemIdFieldName, false);

                // Is it for reprocessing?
                reprocessing = (targetWI != null && _config.Force);

                // Proceed only if the work item does not exist at target or the force option is enabled.
                if (targetWI == null || reprocessing)
                {
                    // Send some traces.
                    if (reprocessing)
                        verb = "Migrating/Reprocessing";
                    else
                        verb = "Migrating";
                    _mySource.Value.TraceInformation($"{verb} work item ({sourceWIType}): {sourceWI.Id} from source project - {processedWIs}/{totalWIs}");
                    _mySource.Value.Flush();

                    // Is the work item type is among the ones we care to handle?
                    if (Engine.TestIfSupportedWorkItemTypeDefinition(sourceWIType))
                    {
                        // Get the corresponding type at target.
                        targetWIType = Engine.WorkItemTypeDefinitions[sourceWIType].DiscreteMapValue;

                        // Modify an existing work item revision.
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
                            _mySource.Value.Flush();

                            // Open work item.
                            targetWI.Open();
                        }
                        // Create a new work item.
                        else
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Create a work item ({targetWIType}) into target project");
                            _mySource.Value.Flush();

                            // Create a new work item.
                            targetWI = targetProject.WorkItemTypes[targetWIType].NewWorkItem();

                            // Set the 'CreatedBy' and 'CreatedDate' fields with the values from the work item at the source from the last revision.
                            targetWI.Fields["System.CreatedDate"].Value = sourceWI.Fields["System.CreatedDate"].Value;
                            targetWI.Fields["System.CreatedBy"].Value = sourceWI.Fields["System.CreatedBy"].Value;
                        }

                        // Populate a revision of work item for the first time or perform reprocessing.
                        PopulateWorkItem(sourceWI, targetWI, reprocessing);

                        // Apply or re-apply field mappings.
                        Engine.ApplyFieldMappings(sourceWI, targetWI);

                        // Populate history.
                        PopulateWorkItemHistory(sourceWI, targetWI, reprocessing);

                        // Validate the target work item to see if any problems arise.
                        failFields = targetWI.Validate();

                        // Send some traces.
                        if (failFields.Count > 0)
                            foreach (Field f in failFields)
                                _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Invalid: {targetWI.Id}-{targetWIType}-{targetWI.Title} Status: {f.Status} Field: {f.ReferenceName} Value: {f.Value}");
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Work item definition name {sourceWIType} is not part of the list provided in the configuration.json under WorkItemTypeDefinitions. Add it to the list to enable migration of this work item type.");

                        // Increment the skip counter.
                        skippedWIs++;
                    }

                    // Flush traces.
                    _mySource.Value.Flush();

                    // Save changes if needed.
                    if (targetWI != null)
                    {
                        try
                        {
                            // If the target work item contains already the reflected work item id field,
                            // modify it with the new value and set the 'ChangedBy' field with the authorized identity name for target project.
                            if (targetWI.Fields.Contains(Engine.ReflectedWorkItemIdFieldName))
                            {
                                targetWI.Fields[Engine.ReflectedWorkItemIdFieldName].Value = sourceStore.GenerateReflectedWorkItemId(sourceWI);
                                targetWI.Fields["System.ChangedBy"].Value = AuthorizedIdentityName;
                            }

                            // Save work item.
                            targetWI.Save();

                            // Close target work item.
                            targetWI.Close();

                            // Send some traces.
                            _mySource.Value.TraceInformation($"Work item saved: {targetWI.Id}");

                            // Update the source reflected work item id if it has been defined to update it.
                            // Is the field defined on work item at the source.
                            if (_config.UpdateSoureReflectedId && sourceWI.Fields.Contains(Engine.SourceReflectedWorkItemIdFieldName))
                            {
                                // Write the value to source reflected work item id field.
                                sourceWI.Fields[Engine.SourceReflectedWorkItemIdFieldName].Value = targetStore.GenerateReflectedWorkItemId(targetWI);

                                // Save source work item.
                                sourceWI.Save();

                                // Send some traces.
                                _mySource.Value.TraceInformation($"Source work item updated {sourceWI.Id}");
                            }

                            // Increment the imported counter.
                            importedWIs++;

                            // Flush traces.
                            _mySource.Value.Flush();
                        }
                        catch (Exception ex)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Failed to save {Name}: {ex.Message}");

                            // Increment the failure counter.
                            failureWIs++;

                            // Send some traces.
                            _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
                            _mySource.Value.Flush();
                        }
                    }
                }
                else
                {
                    // Send some traces.
                    if (reprocessing)
                        verb = "migrate/reprocess";
                    else
                        verb = "migrate";
                    _mySource.Value.TraceInformation($"Cannot {verb} work item ({sourceWIType}): {sourceWI.Id} from source project because it already exists at target");
                    _mySource.Value.Flush();

                    // Increment the skip counter.
                    skippedWIs++;
                }

                // Close source work item.
                sourceWI.Close();

                // Stop timer.
                ReplayWITimer.Stop();

                // Decrement the number of remaining work items to process.
                remainingWIs--;

                // Calculate stats.
                averageTimespan = new TimeSpan(0, 0, 0, 0, (int)(InternalExecutionTimer.ElapsedMilliseconds / processedWIs));
                averageTimeAsString = string.Format(@"{0:s\:fff} seconds", averageTimespan);
                remainingTimespan = new TimeSpan(0, 0, 0, 0, (int)(averageTimespan.TotalMilliseconds * remainingWIs));

                // Calculate the time to completion.
                timeToCompletion = string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remainingTimespan);

                // Send some traces.
                _mySource.Value.TraceInformation($"Average time of {averageTimeAsString} per work item and {timeToCompletion} estimated to completion");
                _mySource.Value.Flush();
            }

            // Stop timer.
            InternalExecutionTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"Done in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds - {1} WIs, {2} Imported WIs, {3} Skipped WIs, {4} Failures with WIs", InternalExecutionTimer.Elapsed, sourceWIs.Count, importedWIs, skippedWIs, failureWIs);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "WorkItemMigrationContext"; }
        }

        public WorkItemMigrationContext(MigrationEngine me, WorkItemMigrationConfig config) : base(me)
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