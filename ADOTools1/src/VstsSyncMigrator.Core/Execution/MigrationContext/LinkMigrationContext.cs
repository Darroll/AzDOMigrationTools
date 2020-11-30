using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.Processing;
using VstsSyncMigrator.Engine.Execution.Exceptions;

namespace VstsSyncMigrator.Engine
{
    public class LinkMigrationContext : MigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.LinkMigrationContext"));

        // Create locking object.
        private static readonly object _lock = new object();

        #endregion

        #region - Private Members

        private readonly LinkMigrationConfig _config;

        private void MigrateSharedSteps(WorkItem sourceWI, WorkItem targetWI, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore)
        {
            // Initialize.
            const string microsoftVstsTcmSteps = "Microsoft.VSTS.TCM.Steps";

            // Get old steps.
            string oldSteps = targetWI.Fields[microsoftVstsTcmSteps].Value.ToString();
            // Copy them.
            string newSteps = oldSteps;

            // Query shared step links.
            // todo: verify if a lock would be needed here.
            var sourceSharedStepLinks = sourceWI.Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.Name == "Shared Steps").ToList();

            // Query all shared steps.
            var sourceSharedSteps =
                sourceSharedStepLinks.Select(x => sourceStore.Store.GetWorkItem(x.RelatedWorkItemId));

            // Browse shared steps.
            foreach (WorkItem sourceSharedStep in sourceSharedSteps)
            {
                // Search for reflected work item.
                WorkItem matchingTargetSharedStep = targetStore.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedStep, Engine.ReflectedWorkItemIdFieldName);

                // If it exists...
                if (matchingTargetSharedStep != null)
                {
                    // Create new steps with new references.
                    newSteps = newSteps.Replace($"ref=\"{sourceSharedStep.Id}\"", $"ref=\"{matchingTargetSharedStep.Id}\"");

                    // Save these steps on work item at the target.
                    targetWI.Fields[microsoftVstsTcmSteps].Value = newSteps;
                }
            }

            // Save if needed.
            // Operation against the store will call internal methods
            // that are not thread-safe.
            // Synchronize the save operation to store.
            if (targetWI.IsDirty)
            {
                lock (_lock)
                {
                    targetWI.Save();
                }
            }
        }

        private void CreateExternalLink(ExternalLink link, WorkItem targetWI)
        {
            ExternalLink vel = (from Link l in targetWI.Links
                         where l is ExternalLink && ((ExternalLink)l).LinkedArtifactUri == ((ExternalLink)link).LinkedArtifactUri
                         select (ExternalLink)l).SingleOrDefault();
            
            if (vel == null)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Creating new {0} on {1}", link.GetType().Name, targetWI.Id);
                _mySource.Value.Flush();

                ExternalLink nel = new ExternalLink(link.ArtifactLinkType, link.LinkedArtifactUri) { Comment = link.Comment };

                // Operation against the store will call internal methods
                // that are not thread-safe.
                // Synchronize the save operation to store.
                lock (_lock)
                {
                    // Add link.
                    targetWI.Links.Add(nel);

                    // Save target work item.
                    targetWI.Save();
                }
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Link {0} on {1} already exists", link.GetType().Name, targetWI.Id);
                _mySource.Value.Flush();
            }
        }

        private bool IsExternalLink(Link l)
        {
            return l is ExternalLink;
        }

        private void CreateRelatedLink(WorkItem sourceWI, RelatedLink link, WorkItem targetWI, WorkItemStoreContext sourceWIStore, WorkItemStoreContext targetWIStore)
        {
            // Initialize.
            bool linkAlreadyExists;
            // sourceWI is the source work item from source context.
            // targetWI is the target work item from target context.
            WorkItem sourceLWI = null;      // This is the linked source work item from source context.
            WorkItem targetLWI = null;      // This is the linked target work item from target context.
            
            try
            {
                #region - Retrieve linked work item from source and target contexts.

                // Try to get the linked work item from the source context.
                try
                {
                    sourceLWI = sourceWIStore.Store.GetWorkItem(link.RelatedWorkItemId);
                }
                catch (Exception ex)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"[FIND-FAIL] Adding Link of type {link.LinkTypeEnd.ImmutableName} where sourceWI={sourceWI.Id}, targetWI={targetWI.Id}");
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);

                    // Leave.
                    throw;
                }

                // Try to get the linked work item from the target context.
                try
                {
                    // Get the work item from the target context.
                    targetLWI = GetWorkItemFromContext(sourceLWI, targetWI, targetWIStore);
                }
                catch (Exception ex)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("[FIND-FAIL] Adding Link of type {0} where sourceWIL={1}, targetWIL={2} ", link.LinkTypeEnd.ImmutableName, sourceWI.Id, targetWI.Id);
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);

                    // Leave.
                    throw;
                }

                #endregion

                #region - Process linked worked item from target context.

                if (targetLWI != null)
                {
                    // Verify if work item at the target has this link already.
                    try
                    {
                        RelatedLink vrl = (
                                            from Link l in targetWI.Links
                                            where l is RelatedLink
                                                && ((RelatedLink)l).RelatedWorkItemId == targetLWI.Id
                                                && ((RelatedLink)l).LinkTypeEnd.ImmutableName == link.LinkTypeEnd.ImmutableName
                                            select (RelatedLink)l).SingleOrDefault();

                        linkAlreadyExists = (vrl != null);
                    }
                    catch (Exception ex)
                    {
                        // Send some traces.
                        string sourceWIForDisplay = (sourceWI != null) ? sourceWI.Id.ToString() : "NotFound";
                        string SourceLWIForDisplay = (sourceLWI != null) ? sourceLWI.Id.ToString() : "NotFound";
                        string targetWIForDisplay = (targetWI != null) ? targetWI.Id.ToString() : "NotFound";
                        _mySource.Value.TraceInformation($"[SKIP] Unable to migrate links where sourceWI={sourceWIForDisplay}, sourceLWI={SourceLWIForDisplay}, targetWI={targetWIForDisplay}");
                        _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);

                        // Leave.
                        throw;
                    }

                    // If the link does not exist and can be accessed...
                    if (!linkAlreadyExists && !targetLWI.IsAccessDenied)
                    {
                        if (sourceLWI.Id != targetLWI.Id)
                        {
                            // Initialize.
                            WorkItemLinkTypeEnd linkTypeEnd = null;

                            // Send some traces.                        
                            _mySource.Value.TraceInformation($"[CREATE-START] Adding link of type {link.LinkTypeEnd.ImmutableName} where sourceWI = {sourceWI.Id}, sourceLWI = {sourceLWI.Id}, targetWI = {targetWI.Id}, targetLWI = {targetLWI.Id}");

                            // Operation against the store will call internal methods
                            // that are not thread-safe.
                            // Synchronize the reading to store.
                            lock(_lock)
                            {
                                linkTypeEnd = targetWIStore.Store.WorkItemLinkTypes.LinkTypeEnds[link.LinkTypeEnd.ImmutableName];
                            }

                            RelatedLink nrl = new RelatedLink(linkTypeEnd, targetLWI.Id);

                            // Operation against the store will call internal methods
                            // that are not thread-safe.
                            // Synchronize the save operation to store.
                            lock (_lock)
                            {
                                // Add the link.
                                targetWI.Links.Add(nrl);

                                // Save.
                                targetWI.Save();
                            }

                            // Send some traces.
                            _mySource.Value.TraceInformation($"[CREATE-SUCCESS] Adding link of type {link.LinkTypeEnd.ImmutableName} where sourceWI = {sourceWI.Id}, sourceLWI = {sourceLWI.Id}, targetWI = {targetWI.Id}, targetLWI = {targetLWI.Id}");
                        }
                        else
                        {
                            // This situation could occur when a linked work item cannot be found at the target. Links cannot
                            // create on unexistent work items.

                            // Send some traces.                        
                            _mySource.Value.TraceInformation($"[SKIP] Unable to migrate link where link of type {link.LinkTypeEnd.ImmutableName} where sourceWI = {sourceWI.Id}, sourceLWI = {sourceLWI.Id}, targetWI = {targetWI.Id}, targetLWI = {targetLWI.Id}");
                        }
                    }
                    else
                    {
                        // Send some traces.
                        if (linkAlreadyExists)
                            _mySource.Value.TraceInformation($"[SKIP] Already exists a link of type {link.LinkTypeEnd.ImmutableName} where sourceWI={sourceWI.Id}, sourceLWI={sourceLWI.Id}, targetWI={targetWI.Id}, targetLWI={targetLWI.Id}");
                        if (targetLWI.IsAccessDenied)
                            _mySource.Value.TraceInformation($"[AccessDenied] The Target  work item is inaccessible to create a link of type {link.LinkTypeEnd.ImmutableName} where sourceWI={sourceWI.Id}, sourceLWI={sourceLWI.Id}, targetWI={targetWI.Id}, targetLWI={targetLWI.Id}");
                    }
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"[SKIP] Cant find targetLWI where sourceWI={sourceWI.Id}, sourceLWI={sourceLWI.Id}, targetWI={targetWI.Id}");
                }

                #endregion
            }
            catch (Exception ex)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, ex.Message);
            }
            finally
            {
                // Flush traces.
                _mySource.Value.Flush();
            }
        }

        private WorkItem GetWorkItemFromContext(WorkItem sourceWI, WorkItem targetWI, WorkItemStoreContext wiStore)
        {
            // This method is to get the work item object but from the given context.
            
            // Initialize.
            WorkItem contextLWI;            // Linked work item from given store context.

            // If source and given contexts are the same, return the work item at the source.
            if (!(targetWI == null)
                && sourceWI.Project.Name == targetWI.Project.Name
                && sourceWI.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", null) == targetWI.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", null))
                contextLWI = sourceWI;
            // If source and given contexts are NOT the same...
            else
            {
                // Try to find the reflected work item from the given store context.
                contextLWI = wiStore.FindReflectedWorkItem(sourceWI, Engine.ReflectedWorkItemIdFieldName, true);
                if (contextLWI == null) // Assume source only (other team project)
                {
                    contextLWI = sourceWI;
                    if (contextLWI.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", null) != sourceWI.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", null))
                        contextLWI = null; // Totally bogus break! as not same team collection
                }
            }

            // Return work item from the given store context.
            return contextLWI;
        }

        private bool IsRelatedLink(Link l)
        {
            return l is RelatedLink;
        }

        private void CreateHyperlink(Hyperlink link, WorkItem w)
        {
            // Verify if work item at the target has this link already.
            Hyperlink vhl = (from Link l in w.Links where l is Hyperlink && ((Hyperlink)l).Location == ((Hyperlink)link).Location select (Hyperlink)l).SingleOrDefault();
            
            if (vhl == null)
            {
                Hyperlink nhl = new Hyperlink(link.Location) { Comment = link.Comment };

                // Operation against the store will call internal methods
                // that are not thread-safe.
                // Synchronize the save operation to store.
                lock (_lock)
                {
                    // Add link.
                    w.Links.Add(nhl);

                    // Save target work item.
                    w.Save();
                }
            }
        }

        private bool IsHyperlink(Link l)
        {
            return l is Hyperlink;
        }

        #endregion

        #region - Internal Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0039:Use local function", Justification = "Prefer to use an action instead of local function")]
        internal override void InternalExecute()
        {
            // Create a stop watch to measure the execution time.
            Stopwatch migrationOpTimer = new Stopwatch();

            // Start timer.
            migrationOpTimer.Start();

            // Initialize.
            TfsQueryContext tfsqc;
            WorkItemCollection statWIs;
            WorkItemCollection sourceWIs;

            // Get source work item store.
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(Engine.Source, WorkItemStoreFlags.BypassRules);

            // Create a query context for statistics.
            tfsqc = new TfsQueryContext(sourceStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Source.Name);

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id], [System.ExternalLinkCount], [System.HyperLinkCount], [System.RelatedLinkCount] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0}", _config.QueryBit);

            // Execute.
            statWIs = tfsqc.Execute();

            // Count the total number of links.
            int totalLinks = 0;
            foreach (WorkItem statWI in statWIs.Cast<WorkItem>())
                totalLinks += (statWI.HyperLinkCount + statWI.ExternalLinkCount + statWI.RelatedLinkCount);

            // Send some traces.
            _mySource.Value.TraceInformation($"Migrate {totalLinks} links found within {statWIs.Count} work items?");
            _mySource.Value.Flush();

            // Create a new query context.
            tfsqc = new TfsQueryContext(sourceStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Source.Name);

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id]", _config.QueryBit); // AND  [Microsoft.VSTS.Common.ClosedDate] = ''
            //tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", config.QueryBit); // AND  [Microsoft.VSTS.Common.ClosedDate] = ''

            // Execute.
            sourceWIs = tfsqc.Execute();

            // Define target work item store context.
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);
            string destProjectName = targetStore.GetProject().Name;

            // Send some traces.
            _mySource.Value.TraceInformation($"Found target project as {destProjectName}");
            _mySource.Value.Flush();

            // Create the action delegate to be called in the foreach in parallel.
            Action<WorkItem> migrateLinksForWI = (sourceWI) =>
            {
                // Initialize.
                int totalOpLinks = 0;
                int totalOpLinksToCreate = 0;
                double averageOpSpeed = 0;
                string prefixTraceMessage = $"Work Item {sourceWI.Id}";

                // Create a stop watch to measure the execution time.
                Stopwatch migrateLinkOpTimer = new Stopwatch();

                // Start timer.
                migrateLinkOpTimer.Start();

                try
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"{prefixTraceMessage} Link Creation: Started");

                    // Initialize.
                    bool canMigrate;
                    WorkItem targetWI = null;

                    try
                    {
                        // Try to find if the work item at the target contains already the reflected work item id.
                        targetWI = targetStore.FindReflectedWorkItem(sourceWI, Engine.ReflectedWorkItemIdFieldName, true);
                    }
                    catch (Exception ex)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"{prefixTraceMessage} - Cannot find targetWI matching sourceWI={sourceWI.Id} probably due to missing ReflectedWorkItemID. Error: {ex.Message} StackTrace: {ex.StackTrace}");
                    }

                    if (targetWI != null)
                        canMigrate = true;
                    // sourceWI was not migrated, or the migrated work item has been deleted. 
                    else
                    {
                        canMigrate = false;

                        // Send some traces.
                        _mySource.Value.TraceInformation($"{prefixTraceMessage} - [SKIP] Unable to migrate links where sourceWI={sourceWI.Id}, targetWI=NotFound");
                    }

                    #region - Migrate the links...

                    if (canMigrate)
                    {
                        // Check if the number of links between source and target to see if any processing is required.
                        bool linkCountMatch = false;
                        try
                        {
                            // Get the number of links to create.
                            totalOpLinks = sourceWI.Links.Count;

                            // Calculate how many links are required to create.
                            totalOpLinksToCreate = totalOpLinks - targetWI.Links.Count;

                            // Compare the numbers of links both source and target work items have.
                            linkCountMatch = (targetWI.Links.Count == sourceWI.Links.Count);
                        }
                        catch
                        {
                            _mySource.Value.TraceInformation($"{prefixTraceMessage} - Unable to check link count, continue the process");
                        }

                        // If the number of links match, skip.
                        if (linkCountMatch)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"{prefixTraceMessage} - [SKIP] Source and target have same number of links ({totalOpLinks})");
                        }
                        else
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"{prefixTraceMessage} - Create {totalOpLinksToCreate} of the {totalOpLinks} links found?");

                            // Migrate the links.
                            try
                            {
                                // Indent trace.
                                Trace.Indent();

                                // Process each link.
                                foreach (Link item in sourceWI.Links)
                                {
                                    // Send some traces.
                                    _mySource.Value.TraceInformation($"{prefixTraceMessage} - Migrating links of type {item.GetType().Name}");

                                    // Create hyperlink if needed.
                                    if (IsHyperlink(item))
                                        this.CreateHyperlink((Hyperlink)item, targetWI);
                                    // Create releated link if needed.
                                    else if (IsRelatedLink(item))
                                    {
                                        // Cast link into the proper link type.
                                        RelatedLink rl = (RelatedLink)item;
                                        this.CreateRelatedLink(sourceWI, rl, targetWI, sourceStore, targetStore);
                                    }
                                    // Create external link if needed.
                                    else if (IsExternalLink(item))
                                    {
                                        // Cast link into the proper link type.
                                        ExternalLink rl = (ExternalLink)item;
                                        this.CreateExternalLink(rl, targetWI);
                                    }
                                    // Unsupported or unknown link type.
                                    else
                                    {
                                        // Generate exception.
                                        UnknownLinkTypeException ex = new UnknownLinkTypeException($"{prefixTraceMessage} - [UnknownLinkType] Unable to {item.GetType().Name}");

                                        // Send telemetry data.
                                        Telemetry.Current.TrackException(ex);

                                        // Send some traces.
                                        _mySource.Value.TraceInformation(ex.Message);

                                        throw ex;
                                    }
                                }
                            }
                            catch (WorkItemLinkValidationException ex)
                            {
                                // Reset any changes from source and target work items.
                                sourceWI.Reset();
                                targetWI.Reset();

                                // Send telemetry data.
                                Telemetry.Current.TrackException(ex);

                                // Send some traces.
                                _mySource.Value.TraceInformation($"{prefixTraceMessage} - [WorkItemLinkValidationException] Adding link for sourceWI={sourceWI.Id}");
                                _mySource.Value.TraceInformation(ex.Message);
                            }
                            catch (Exception ex)
                            {
                                // Send telemetry data.
                                Telemetry.Current.TrackException(ex);

                                // Send some traces.
                                _mySource.Value.TraceInformation($"{prefixTraceMessage} - [CREATE-FAIL] Adding Link for sourceWI={sourceWI.Id}");
                                _mySource.Value.TraceInformation(ex.Message);
                            }
                            finally
                            {
                                // flush traces.
                                _mySource.Value.Flush();

                                // Unindent trace.
                                Trace.Unindent();
                            }
                        }

                        // If the work item is of Test Case type, migrate the shared steps.
                        if (sourceWI.Type.Name == "Test Case")
                            this.MigrateSharedSteps(sourceWI, targetWI, sourceStore, targetStore);
                    }

                    // Send some traces.
                    _mySource.Value.TraceInformation($"{prefixTraceMessage} Link Creation: Completed");

                    #endregion
                }
                catch (Exception ex)
                {
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"{prefixTraceMessage} Link Creation: Failed with error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                }
                finally
                {
                    // Flush traces.
                    _mySource.Value.Flush();
                }

                // Stop timer.
                migrateLinkOpTimer.Stop();

                // Calculate average operation speed.
                averageOpSpeed = migrateLinkOpTimer.Elapsed.TotalSeconds / totalOpLinksToCreate;

                // Send some traces.
                _mySource.Value.TraceInformation($"{prefixTraceMessage} - {totalOpLinksToCreate} links created in " + @"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds. Average speed is {1:f2} second/link", migrateLinkOpTimer.Elapsed, averageOpSpeed);
                _mySource.Value.Flush();
            };

            // Process links in parallel.
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = _config.DegreeOfParallelism };
            Parallel.ForEach<WorkItem>(sourceWIs.Cast<WorkItem>(), options, migrateLinksForWI);

            // Stop timer.
            migrationOpTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"All work item links were created in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", migrationOpTimer.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "LinkMigrationContext"; }
        }

        public LinkMigrationContext(MigrationEngine me, LinkMigrationConfig config) : base(me)
        {
            _config = config;
        }

        public void AddTagToWorkItem(WorkItem w, string tag)
        {
            List<string> listOfTags = w.Tags.Split(char.Parse(@";")).ToList();
            List<string> listOfNewTags = listOfTags.Union(new List<string>() { tag }).ToList();
            w.Tags = string.Join(";", listOfNewTags.ToArray());
            w.Save();
        }

        #endregion
        
    }
}