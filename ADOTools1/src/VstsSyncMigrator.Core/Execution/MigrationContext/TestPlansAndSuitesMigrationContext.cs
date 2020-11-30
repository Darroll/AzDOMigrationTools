using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class TestPlandsAndSuitesMigrationContext : MigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.TestPlandsAndSuitesMigrationContext"));

        #endregion

        #region - Private Members

        private readonly WorkItemStoreContext _sourceWitStore;
        private readonly TestManagementContext _sourceTestStore;
        private readonly ITestConfigurationCollection _sourceTestConfigs;
        private readonly WorkItemStoreContext _targetWitStore;
        private readonly TestManagementContext _targetTestStore;
        private readonly ITestConfigurationCollection _targetTestConfigs;
        private readonly TestPlansAndSuitesMigrationConfig _config;
        private readonly IIdentityManagementService _sourceIdentityManagementService;
        private readonly IIdentityManagementService _targetIdentityManagementService;

        private ClassificationNodeDetection _nodeOMatic;

        /// <summary>
        /// Remove invalid links
        /// </summary>
        /// <remarks>
        /// VSTS cannot store some links which have an invalid URI Scheme. You will get errors like "The URL specified has a potentially unsafe URL protocol"
        /// For myself, the issue were urls that pointed to TFVC:    "vstfs:///VersionControl/Changeset/19415"
        /// Unfortunately the API does not seem to allow access to the "raw" data, so there's nowhere to retrieve this as far as I can find.
        /// Should take care of https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </remarks>
        /// <param name="targetTP">The plan to remove invalid links drom</param>
        private void RemoveInvalidLinks(ITestPlan targetTP)
        {
            var linksToRemove = new List<ITestExternalLink>();
            foreach (var link in targetTP.Links)
            {
                try
                { link.Uri.ToString(); }
                catch (UriFormatException)
                { linksToRemove.Add(link); }
            }

            if (linksToRemove.Any())
            {
                if (!_config.RemoveInvalidTestSuiteLinks)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("We have detected test suite links that probably can't be migrated. You might receive an error 'The URL specified has a potentially unsafe URL protocol' when migrating to VSTS.");
                    _mySource.Value.TraceInformation("Please see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178 for more details.");
                    _mySource.Value.Flush();
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Link count before removal of invalid: [{targetTP.Links.Count}]");
                    _mySource.Value.Flush();

                    foreach (var link in linksToRemove)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Link with Description [{link.Description}] could not be migrated, as the URI is invalid. (We can't display the URI because of limitations in the TFS/VSTS/DevOps API.) Removing link.");
                        _mySource.Value.Flush();

                        targetTP.Links.Remove(link);
                    }

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Link count after removal of invalid: [{targetTP.Links.Count}]");
                    _mySource.Value.Flush();
                }
            }
        }

        private void AssignReflectedWorkItemId(int sourceWIId, int targetWIId)
        {
            WorkItem sourceWI = _sourceWitStore.Store.GetWorkItem(sourceWIId);
            WorkItem targetWI = _targetWitStore.Store.GetWorkItem(targetWIId);
            targetWI.Fields[Engine.ReflectedWorkItemIdFieldName].Value = _sourceWitStore.GenerateReflectedWorkItemId(sourceWI);
            targetWI.Save();
        }

        private void ApplyFieldMappings(int sourceWIId, int targetWIId)
        {
            var sourceWI = _sourceWitStore.Store.GetWorkItem(sourceWIId);
            var targetWI = _targetWitStore.Store.GetWorkItem(targetWIId);
            
            if (_config.PrefixProjectToNodes)
            {
                targetWI.AreaPath = string.Format(@"{0}\{1}", Engine.Target.Name, sourceWI.AreaPath);
                targetWI.IterationPath = string.Format(@"{0}\{1}", Engine.Target.Name, sourceWI.IterationPath);
            }
            else
            {
                var regex = new Regex(Regex.Escape(Engine.Source.Name));
                targetWI.AreaPath = regex.Replace(sourceWI.AreaPath, Engine.Target.Name, 1);
                targetWI.IterationPath = regex.Replace(sourceWI.IterationPath, Engine.Target.Name, 1);
            }

            // Set area/iteration path from config if specified
            if (_config.AreaPath != null) { targetWI.AreaPath = $@"{Engine.Target.Name}\Area\{_config.AreaPath}".TrimEnd('\\'); }
            if (_config.IterationPath != null) { targetWI.IterationPath = $@"{Engine.Target.Name}\Iteration\{_config.IterationPath}".TrimEnd('\\'); }
         
            if (_nodeOMatic == null)
                _nodeOMatic = new ClassificationNodeDetection(_targetWitStore.Store);
         
            // Validate the node exists
            if (!_nodeOMatic.NodeExists(targetWI.AreaPath))
            {
                // Send some traces.
                _mySource.Value.TraceInformation("The Node '{0}' does not exist, leaving as '{1}'. This may be because it has been renamed or moved and no longer exists, or that you have not migrated the Node Structure yet.", targetWI.AreaPath, Engine.Target.Name);
                _mySource.Value.Flush();

                targetWI.AreaPath = Engine.Target.Name;
            }
         
            // Validate the node exists
            if (!_nodeOMatic.NodeExists(targetWI.IterationPath))
            {
                // Send some traces.
                _mySource.Value.TraceInformation("The Node '{0}' does not exist, leaving as '{1}'. This may be because it has been renamed or moved and no longer exists, or that you have not migrated the Node Structure yet.", targetWI.IterationPath, Engine.Target.Name);
                _mySource.Value.Flush();

                targetWI.IterationPath = Engine.Target.Name;
            }
            // Replace extra Node path from area/iteration
            targetWI.AreaPath = targetWI.AreaPath.Replace("Area\\","").TrimEnd('\\');
            targetWI.AreaPath = targetWI.AreaPath.Replace("Area", "").TrimEnd('\\');
            targetWI.IterationPath = targetWI.IterationPath.Replace("Iteration\\","").TrimEnd('\\');
            targetWI.IterationPath = targetWI.IterationPath.Replace("Iteration", "").TrimEnd('\\');
            Engine.ApplyFieldMappings(sourceWI, targetWI);
            targetWI.Save();
        }

        private bool CanSkipElementBecauseOfTags(int wiId)
        {
            if (_config.OnlyElementsWithTag == null)
                return false;

            var sourcePlanWorkItem = _sourceWitStore.Store.GetWorkItem(wiId);
            var tagWhichMustBePresent = _config.OnlyElementsWithTag;
            return !sourcePlanWorkItem.Tags.Contains(tagWhichMustBePresent);
        }

        private void ProcessTestSuite(ITestSuiteBase sourceTS, ITestSuiteBase targetTSParent, ITestPlan targetTP)
        {
            if (CanSkipElementBecauseOfTags(sourceTS.Id))
                return;

            // Send some traces.
            _mySource.Value.TraceInformation($"Processing {sourceTS.TestSuiteType} : {sourceTS.Id} - {sourceTS.Title} ", Name);
            _mySource.Value.Flush();

            var targetSuiteChild = FindSuiteEntry((IStaticTestSuite)targetTSParent, sourceTS.Title);

            // Target suite is not found in target system. We should create it.
            if (targetSuiteChild == null)
            {
                switch (sourceTS.TestSuiteType)
                {
                    case TestSuiteType.None:
                        throw new NotImplementedException($"todo implement test suite type {sourceTS.TestSuiteType.ToString()}");
                    case TestSuiteType.DynamicTestSuite:
                        targetSuiteChild = CreateNewDynamicTestSuite(sourceTS);
                        break;
                    case TestSuiteType.StaticTestSuite:
                        targetSuiteChild = CreateNewStaticTestSuite(sourceTS);
                        break;
                    case TestSuiteType.RequirementTestSuite:
                        int sourceRid = ((IRequirementTestSuite)sourceTS).RequirementId;
                        WorkItem sourceReq;
                        WorkItem targetReq;
                        try
                        {
                            sourceReq = _sourceWitStore.Store.GetWorkItem(sourceRid);
                            if (sourceReq == null)
                            {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Source work item not found", Name);
                            _mySource.Value.Flush();

                            break;
                            }
                        }
                        catch (Exception)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Source work item cannot be loaded", Name);
                            _mySource.Value.Flush();

                            break;
                        }
                        try
                        {
                            targetReq = _targetWitStore.FindReflectedWorkItemByReflectedWorkItemId(sourceReq,
                                Engine.ReflectedWorkItemIdFieldName);

                            if (targetReq == null)
                            {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Target work item not found", Name);
                            _mySource.Value.Flush();

                            break;
                            }
                        }
                        catch (Exception)
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Source work item not migrated to target, cannot be found", Name);
                            _mySource.Value.Flush();

                            break;
                        }
                        targetSuiteChild = CreateNewRequirementTestSuite(sourceTS, targetReq);
                        break;

                    default:
                        throw new NotImplementedException($"todo implement test suite type {sourceTS.TestSuiteType.ToString()}");
                        //break;
                }

                if (targetSuiteChild == null) { return; }
            
                // Apply default configurations, Add to target and Save
                ApplyDefaultConfigurations(sourceTS, targetSuiteChild);
                if (targetSuiteChild.Plan == null)
                    SaveNewTestSuiteToPlan(targetTP, (IStaticTestSuite)targetTSParent, targetSuiteChild);

                ApplyFieldMappings(sourceTS.Id, targetSuiteChild.Id);
                AssignReflectedWorkItemId(sourceTS.Id, targetSuiteChild.Id);
                FixAssignedToValue(sourceTS.Id, targetSuiteChild.Id);
            }
            else
            {
                // The target suite already exists, take it from here

                // Send some traces.
                _mySource.Value.TraceInformation("Suite Exists", Name);
                _mySource.Value.Flush();

                ApplyDefaultConfigurations(sourceTS, targetSuiteChild);
                if (targetSuiteChild.IsDirty)
                    targetTP.Save();
            }

            // Recurse if Static Suite
            if (sourceTS.TestSuiteType == TestSuiteType.StaticTestSuite)
            {
                // Add Test Cases 
                AddChildTestCases(sourceTS, targetSuiteChild, targetTP);

                if (HasChildSuites(sourceTS))
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Suite has {((IStaticTestSuite)sourceTS).Entries.Count} children", Name);
                    _mySource.Value.Flush();

                    foreach (var sourceSuitChild in ((IStaticTestSuite)sourceTS).SubSuites)
                        ProcessTestSuite(sourceSuitChild, targetSuiteChild, targetTP);
                }
            }
        }

        /// <summary>
        /// Fix work item ID's in query based suites
        /// </summary>
        private void FixWorkItemIdInQuery(ITestSuiteBase targetTSChild)
        {
            var targetPlan = targetTSChild.Plan;
            if (targetTSChild.TestSuiteType == TestSuiteType.DynamicTestSuite)
            {
            var dynamic = (IDynamicTestSuite)targetTSChild;

            if (
                CultureInfo.InvariantCulture.CompareInfo.IndexOf(dynamic.Query.QueryText, "[System.Id]",
                    CompareOptions.IgnoreCase) >= 0)
            {
                string regExSearchForSystemId = @"(\[System.Id\]\s*=\s*[\d]*)";
                // string regExSearchForSystemId2 = @"(\[System.Id\]\s*IN\s*)";

                MatchCollection matches = Regex.Matches(dynamic.Query.QueryText, regExSearchForSystemId, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    var qid = match.Value.Split('=')[1].Trim();
                    var targetWi = _targetWitStore.FindReflectedWorkItemByReflectedWorkItemId(qid,
                        Engine.ReflectedWorkItemIdFieldName);

                    if (targetWi == null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Target WI does not exist. We are skipping this item. Please fix it manually.");
                        _mySource.Value.Flush();
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Fixing [System.Id] in query in test suite '" + dynamic.Title + "' from " + qid + " to " + targetWi.Id, Name);
                        _mySource.Value.Flush();

                        dynamic.Refresh();
                        dynamic.Repopulate();
                        dynamic.Query = _targetTestStore.Project.CreateTestQuery(dynamic.Query.QueryText.Replace(match.Value, string.Format("[System.Id] = {0}", targetWi.Id)));
                        targetPlan.Save();
                    }
                }
            }
            }
        }

        private void FixAssignedToValue(int sourceWIId, int targetWIId)
        {
            var sourceWI = _sourceWitStore.Store.GetWorkItem(sourceWIId);
            var targetWI = _targetWitStore.Store.GetWorkItem(targetWIId);
            targetWI.Fields["System.AssignedTo"].Value = sourceWI.Fields["System.AssignedTo"].Value;
            targetWI.Save();
        }

        private void AddChildTestCases(ITestSuiteBase sourceTS, ITestSuiteBase targetTS, ITestPlan targetTP)
        {
            targetTS.Refresh();
            targetTP.Refresh();
            targetTP.RefreshRootSuite();

            if (CanSkipElementBecauseOfTags(sourceTS.Id))
            return;

            // Send some traces.
            _mySource.Value.TraceInformation("Suite has {0} test cases", sourceTS.TestCases.Count);
            _mySource.Value.Flush();

            List<ITestCase> tcs = new List<ITestCase>();
            foreach (ITestSuiteEntry sourceTestCaseEntry in sourceTS.TestCases)
            {
            // Send some traces.
            _mySource.Value.TraceInformation($"Work item: {sourceTestCaseEntry.Id}");
            _mySource.Value.Flush();

            if (CanSkipElementBecauseOfTags(sourceTestCaseEntry.Id))
                return;

            // Send some traces.
            _mySource.Value.TraceInformation("Processing {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title);
            _mySource.Value.Flush();

            WorkItem wi = _targetWitStore.FindReflectedWorkItem(sourceTestCaseEntry.TestCase.WorkItem, Engine.ReflectedWorkItemIdFieldName, false);
            if (wi == null)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Can't find work item for Test Case. Has it been migrated? {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title);
                _mySource.Value.Flush();

                break;
            }
            var exists = (from tc in targetTS.TestCases
                            where tc.TestCase.WorkItem.Id == wi.Id
                            select tc).SingleOrDefault();

            if (exists != null)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Test case already in suite {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title);
                _mySource.Value.Flush();
            }
            else
            {
                ITestCase targetTestCase = _targetTestStore.Project.TestCases.Find(wi.Id);
                if (targetTestCase == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("ERROR: Test case not found {0} : {1} - {2} ", sourceTestCaseEntry.EntryType, sourceTestCaseEntry.Id, sourceTestCaseEntry.Title);
                    _mySource.Value.Flush();
                }
                else
                {
                    tcs.Add(targetTestCase);

                    // Send some traces.
                    _mySource.Value.TraceInformation("Adding {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title);
                    _mySource.Value.Flush();
                }
            }
            }

            try
            {
            // This call can fail on an internal SetPointAssignments call with an "Object reference not set to an instance of an object."
            // Ignoring that for now so the test cases can be properly linked at least.
            targetTS.TestCases.AddCases(tcs);
            }
            catch (Exception ex)
            {
            _mySource.Value.TraceEvent(TraceEventType.Warning, 0, $"Error adding test cases. Should be safe to ignore? {ex.Message}");
            _mySource.Value.Flush();
            }
         

            targetTP.Save();

            // Send some traces.
            _mySource.Value.TraceInformation("SAVED {0} : {1} - {2} ", targetTS.TestSuiteType.ToString(), targetTS.Id, targetTS.Title);
            _mySource.Value.Flush();
        }

        /// <summary>
        /// Sets default configurations on migrated test suites.
        /// </summary>
        /// <param name="sourceTS">The test suite to take as a source.</param>
        /// <param name="targetTS">The test suite to apply the default configurations to.</param>
        private void ApplyDefaultConfigurations(ITestSuiteBase sourceTS, ITestSuiteBase targetTS)
        {
            if (sourceTS.DefaultConfigurations != null)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"Setting default configurations for suite {targetTS.Title}");
                _mySource.Value.Flush();

                IList<IdAndName> targetConfigs = new List<IdAndName>();
                foreach (var config in sourceTS.DefaultConfigurations)
                {
                    var targetFound = (from tc in _targetTestConfigs
                                        where tc.Name == config.Name
                                        select tc).SingleOrDefault();
                    if (targetFound != null)
                        targetConfigs.Add(new IdAndName(targetFound.Id, targetFound.Name));
                }
                try
                { targetTS.SetDefaultConfigurations(targetConfigs); }
                catch (Exception)
                {
                    // Sometimes this will error out for no reason.
                }
            }
            else
                targetTS.ClearDefaultConfigurations();
        }

        private void ApplyConfigurationsAndAssignTesters(ITestSuiteBase sourceTS, ITestSuiteBase targetTS)
        {
            // Send some traces.
            _mySource.Value.TraceInformation($"Applying configurations for test cases in source test suite {sourceTS.Title}");
            _mySource.Value.Flush();

            foreach (ITestSuiteEntry sourceTce in sourceTS.TestCases)
            {
                WorkItem wi = _targetWitStore.FindReflectedWorkItem(sourceTce.TestCase.WorkItem, Engine.ReflectedWorkItemIdFieldName, false);
                ITestSuiteEntry targetTce;
                if (wi != null)
                {
                    targetTce = (from tc in targetTS.TestCases
                                where tc.TestCase.WorkItem.Id == wi.Id
                                select tc).SingleOrDefault();
                    if (targetTce != null)
                        ApplyConfigurations(sourceTce, targetTce);
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Test Case ${sourceTce.Title} from source is not included in target. Cannot apply configuration for it.");
                        _mySource.Value.Flush();
                    }
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Work Item for Test Case {sourceTce.Title} cannot be found in target. Has it been migrated?");
                    _mySource.Value.Flush();
                }
            }

            AssignTesters(sourceTS, targetTS);

            //Loop over child suites and set configurations for test case entries there
            if (HasChildSuites(sourceTS))
            {
                foreach (ITestSuiteEntry sourceSuiteChild in ((IStaticTestSuite)sourceTS).Entries.Where(
                            e => e.EntryType == TestSuiteEntryType.DynamicTestSuite
                            || e.EntryType == TestSuiteEntryType.RequirementTestSuite
                            || e.EntryType == TestSuiteEntryType.StaticTestSuite))
                {
                    //Find migrated suite in target
                    WorkItem sourceSuiteWi = _sourceWitStore.Store.GetWorkItem(sourceSuiteChild.Id);
                    WorkItem targetSuiteWi = _targetWitStore.FindReflectedWorkItem(sourceSuiteWi, Engine.ReflectedWorkItemIdFieldName, false);
                    if (targetSuiteWi != null)
                    {
                        ITestSuiteEntry targetSuiteChild = (from tc in ((IStaticTestSuite)targetTS).Entries
                                                            where tc.Id == targetSuiteWi.Id
                                                            select tc).FirstOrDefault();
                        if (targetSuiteChild != null)
                            ApplyConfigurationsAndAssignTesters(sourceSuiteChild.TestSuite, targetSuiteChild.TestSuite);
                        else
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"Test Suite {sourceSuiteChild.Title} from source cannot be found in target. Has it been migrated?");
                            _mySource.Value.Flush();
                        }
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Test Suite {sourceSuiteChild.Title} from source cannot be found in target. Has it been migrated?");
                        _mySource.Value.Flush();
                    }
                }
            }
        }

        private void AssignTesters(ITestSuiteBase sourceTS, ITestSuiteBase targetTS)
        {
            if (targetTS == null)
            {
                // Send some traces.
                _mySource.Value.TraceEvent(TraceEventType.Error, 0, "Target Suite is NULL");
                _mySource.Value.Flush();
            }

            List<ITestPointAssignment> assignmentsToAdd = new List<ITestPointAssignment>();

            //loop over all source test case entries
            foreach (ITestSuiteEntry sourceTce in sourceTS.TestCases)
            {
                // find target testcase id for this source tce
                WorkItem targetTc = _targetWitStore.FindReflectedWorkItem(sourceTce.TestCase.WorkItem, Engine.ReflectedWorkItemIdFieldName, false);

                if (targetTc == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceEvent(TraceEventType.Error, 0, $"Target Reflected Work Item Not found for source WorkItem ID: {sourceTce.TestCase.WorkItem.Id}");
                    _mySource.Value.Flush();
                }

            //figure out test point assignments for each source tce
                foreach (ITestPointAssignment tpa in sourceTce.PointAssignments)
                {
                    int sourceConfigurationId = tpa.ConfigurationId;

                    TeamFoundationIdentity targetIdentity = null;

                    if (tpa.AssignedTo != null)
                    {
                        targetIdentity = GetTargetIdentity(tpa.AssignedTo.Descriptor);
                        if (targetIdentity == null)
                            _sourceIdentityManagementService.RefreshIdentity(tpa.AssignedTo.Descriptor);
                        targetIdentity = GetTargetIdentity(tpa.AssignedTo.Descriptor);
                    }

                    // translate source configuration id to target configuration id and name
                    // Get source configuration name
                    string sourceConfigName = (from tc in _sourceTestConfigs
                                                where tc.Id == sourceConfigurationId
                                                select tc.Name).FirstOrDefault();

                    // Find source configuration name in target and get the id for it
                    int targetConfigId = (from tc in _targetTestConfigs
                                            where tc.Name == sourceConfigName
                                            select tc.Id).FirstOrDefault();

                    if (targetConfigId != 0)
                    {
                        IdAndName targetConfiguration = new IdAndName(targetConfigId, sourceConfigName);

                        Guid targetUserId = Guid.Empty;
                        if (targetIdentity != null)
                            targetUserId = targetIdentity.TeamFoundationId;

                        // Create a test point assignment with target test case id, target configuration (id and name) and target identity
                        var newAssignment = targetTS.CreateTestPointAssignment(targetTc.Id, targetConfiguration, targetUserId);

                        // add the test point assignment to the list
                        assignmentsToAdd.Add(newAssignment);
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Cannot find configuration with name [{sourceConfigName}] in target. Cannot assign tester to it.");
                        _mySource.Value.Flush();
                    }
                }
            }

            // assign the list to the suite
            targetTS.AssignTestPoints(assignmentsToAdd);
        }

        /// <summary>
        /// Retrieve the target identity for a given source descriptor
        /// </summary>
        /// <param name="sourceIdentityDescriptor">Source identity Descriptor</param>
        /// <returns>Target Identity</returns>
        private TeamFoundationIdentity GetTargetIdentity(IdentityDescriptor sourceIdentityDescriptor)
        {
            var sourceIdentity = _sourceIdentityManagementService.ReadIdentity(
                sourceIdentityDescriptor,
                MembershipQuery.Direct,
                ReadIdentityOptions.ExtendedProperties);
            string sourceIdentityMail = sourceIdentity.GetProperty("Mail") as string;

            // Try refresh the Identity if we are missing the Mail property
            if (string.IsNullOrEmpty(sourceIdentityMail))
            {
                sourceIdentity = _sourceIdentityManagementService.ReadIdentity(
                    sourceIdentityDescriptor,
                    MembershipQuery.Direct,
                    ReadIdentityOptions.ExtendedProperties);

                sourceIdentityMail = sourceIdentity.GetProperty("Mail") as string;
            }

            if (!string.IsNullOrEmpty(sourceIdentityMail))
            {
                // translate source assignedto name to target identity
                var targetIdentity = _targetIdentityManagementService.ReadIdentity(
                    IdentitySearchFactor.MailAddress,
                    sourceIdentityMail,
                    MembershipQuery.Direct,
                    ReadIdentityOptions.None);

                if (targetIdentity == null)
                {
                    targetIdentity = _targetIdentityManagementService.ReadIdentity(
                        IdentitySearchFactor.AccountName,
                        sourceIdentityMail,
                        MembershipQuery.Direct,
                        ReadIdentityOptions.None);
                }

                if (targetIdentity == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Cannot find tester with e-mail [{sourceIdentityMail}] in target system. Cannot assign.");
                    _mySource.Value.Flush();

                    return null;
                }

                return targetIdentity;
            }
            else
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"No e-mail address known in source system for [{sourceIdentity.DisplayName}]. Cannot translate to target.");
                _mySource.Value.Flush();

                return null;
            }
        }

        /// <summary>
        /// Apply configurations to a single test case entry on the target, by copying from the source
        /// </summary>
        /// <param name="sourceTSE"></param>
        /// <param name="targetTSE"></param>
        private void ApplyConfigurations(ITestSuiteEntry sourceTSE, ITestSuiteEntry targetTSE)
        {
            int sourceConfigCount = sourceTSE.Configurations != null ? sourceTSE.Configurations.Count : 0;
            int targetConfigCount = targetTSE.Configurations != null ? targetTSE.Configurations.Count : 0;
            var deviations = sourceConfigCount > 0 && targetConfigCount > 0 && sourceTSE.Configurations.Select(x => x.Name).Intersect(targetTSE.Configurations.Select(x => x.Name)).Count() < sourceConfigCount;

            if ((sourceConfigCount != targetConfigCount) || deviations)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("CONFIG MISMATCH FOUND --- FIX ATTEMPTING");
                _mySource.Value.Flush();

                IList<IdAndName> targetConfigs = new List<IdAndName>();
                foreach (var config in sourceTSE.Configurations)
                {
                    var targetFound = (from tc in _targetTestConfigs
                                        where tc.Name == config.Name
                                        select tc).SingleOrDefault();
                    if (targetFound != null)
                        targetConfigs.Add(new IdAndName(targetFound.Id, targetFound.Name));
                }
                try
                { targetTSE.SetConfigurations(targetConfigs); }
                catch (Exception ex)
                {
                    // SOmetimes this will error out for no reason.

                    // Send telemetry data.
                    Telemetry.Current.TrackException(ex);
                }
            }
        }

        private bool HasChildTestCases(ITestSuiteBase sourceTS)
        {
            return (sourceTS.TestCaseCount > 0);
        }

        private ITestSuiteBase CreateNewDynamicTestSuite(ITestSuiteBase sourceTS)
        {
            IDynamicTestSuite targetSuiteChild = _targetTestStore.Project.TestSuites.CreateDynamic();
            targetSuiteChild.TestSuiteEntry.Title = sourceTS.TestSuiteEntry.Title;
            ApplyTestSuiteQuery(sourceTS, targetSuiteChild, _targetTestStore);

            return targetSuiteChild;
        }

        private ITestSuiteBase CreateNewRequirementTestSuite(ITestSuiteBase sourceTS, WorkItem requirementWI)
        {
            IRequirementTestSuite targetSuiteChild;
            try
            { targetSuiteChild = _targetTestStore.Project.TestSuites.CreateRequirement(requirementWI); }
            catch (TestManagementValidationException ex)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Unable to Create Requirement based Test Suite: {0}", ex.Message);
                _mySource.Value.Flush();
                return null;
            }

            targetSuiteChild.Title = sourceTS.Title;
            return targetSuiteChild;
        }

        private void SaveNewTestSuiteToPlan(ITestPlan testPlan, IStaticTestSuite parentTS, ITestSuiteBase newTS)
        {
            // Send some traces.
            _mySource.Value.TraceInformation($"Saving {newTS.TestSuiteType} : {newTS.Id} - {newTS.Title}");
            _mySource.Value.Flush();

            try
            { ((IStaticTestSuite)parentTS).Entries.Add(newTS); }
            catch (TestManagementServerException ex)
            {
                // Send telemetry data.
                Telemetry.Current.TrackException(ex,
                        new Dictionary<string, string> {
                                { "Name", Name},
                                { "Target Project", Engine.Target.Name},
                                { "Target Collection", Engine.Target.Collection.Name },
                                { "Source Project", Engine.Source.Name},
                                { "Source Collection", Engine.Source.Collection.Name },
                                { "Status", Status.ToString() },
                                { "Task", "SaveNewTestSuitToPlan" },
                                { "Id", newTS.Id.ToString()},
                                { "Title", newTS.Title},
                                { "TestSuiteType", newTS.TestSuiteType.ToString()}
                        });

                // Send some traces.
                _mySource.Value.TraceInformation("FAILED {0} : {1} - {2} | {3}", newTS.TestSuiteType.ToString(), newTS.Id, newTS.Title, ex.Message);
                _mySource.Value.Flush();

                ITestSuiteBase ErrorSuiteChild = _targetTestStore.Project.TestSuites.CreateStatic();
                ErrorSuiteChild.TestSuiteEntry.Title = string.Format(@"BROKEN: {0} | {1}", newTS.Title, ex.Message);
                ((IStaticTestSuite)parentTS).Entries.Add(ErrorSuiteChild);
            }

            testPlan.Save();
        }

        private ITestSuiteBase CreateNewStaticTestSuite(ITestSuiteBase sourceTS)
        {
            ITestSuiteBase targetSuiteChild = _targetTestStore.Project.TestSuites.CreateStatic();
            targetSuiteChild.TestSuiteEntry.Title = sourceTS.TestSuiteEntry.Title;
            return targetSuiteChild;
        }

        private ITestSuiteBase FindSuiteEntry(IStaticTestSuite staticTS, string titleToFind)
        {
            return (from s in staticTS.SubSuites where s.Title == titleToFind select s).SingleOrDefault();
        }

        private bool HasChildSuites(ITestSuiteBase sourceTS)
        {
            bool hasChildren = false;
            if (sourceTS != null && sourceTS.TestSuiteType == TestSuiteType.StaticTestSuite)
            hasChildren = (((IStaticTestSuite)sourceTS).Entries.Count > 0);
            return hasChildren;
        }

        private ITestPlan CreateNewTestPlanFromSource(ITestPlan sourceTP, string newPlanName)
        {
            ITestPlan targetPlan;
            targetPlan = _targetTestStore.CreateTestPlan();
            targetPlan.CopyPropertiesFrom(sourceTP);
            targetPlan.Name = newPlanName;
            targetPlan.StartDate = sourceTP.StartDate;
            targetPlan.EndDate = sourceTP.EndDate;
            targetPlan.Description = sourceTP.Description;

            // Set area and iteration to root of the target project. 
            // We will set the correct values later, when we actually have a work item available
            targetPlan.Iteration = Engine.Target.Name;
            targetPlan.AreaPath = Engine.Target.Name;

            // Remove testsettings reference because VSTS Sync doesn't support migrating these artifacts
            if (targetPlan.ManualTestSettingsId != 0)
            {
                targetPlan.ManualTestSettingsId = 0;
                targetPlan.AutomatedTestSettingsId = 0;

                // Send some traces.
                _mySource.Value.TraceInformation("Ignoring migration of Testsettings. Azure DevOps Migration Tools don't support migration of this artifact type.");
                _mySource.Value.Flush();
            }

            // Remove reference to build uri because VSTS Sync doesn't support migrating these artifacts
            if (targetPlan.BuildUri != null)
            {
                targetPlan.BuildUri = null;

                // Send some traces.
                _mySource.Value.TraceInformation("Ignoring migration of assigned Build artifact {0}. Azure DevOps Migration Tools don't support migration of this artifact type.", sourceTP.BuildUri);
                _mySource.Value.Flush();
            }
            return targetPlan;
        }

        private ITestPlan FindTestPlan(TestManagementContext tmc, string name)
        {
            return (from p in tmc.Project.TestPlans.Query("Select * From TestPlan") where p.Name == name select p).SingleOrDefault();
        }

        private void ApplyTestSuiteQuery(ITestSuiteBase sourceTS, IDynamicTestSuite targetTSChild, TestManagementContext targetTestStore)
        {
            targetTSChild.Query = ((IDynamicTestSuite)sourceTS).Query;

            // Postprocessing common errors
            FixQueryForTeamProjectNameChange(sourceTS, targetTSChild, targetTestStore);
            FixWorkItemIdInQuery(targetTSChild);
        }

        private void FixQueryForTeamProjectNameChange(ITestSuiteBase sourceTS, IDynamicTestSuite targetTSChild, TestManagementContext targetTestStore)
        {
            // Replacing old projectname in queries with new projectname
            // The target team project name is only available via target test store because the dyn. testsuite isnt saved at this point in time
            if (!sourceTS.Plan.Project.TeamProjectName.Equals(targetTestStore.Project.TeamProjectName))
            {
                // Send some traces.
                _mySource.Value.TraceInformation(@"Team Project names dont match. We need to fix the query in dynamic test suite {0} - {1}.", sourceTS.Id, sourceTS.Title);
                _mySource.Value.TraceInformation(@"Replacing old project name {1} in query {0} with new team project name {2}", targetTSChild.Query.QueryText, sourceTS.Plan.Project.TeamProjectName, targetTestStore.Project.TeamProjectName);
                _mySource.Value.Flush();

                // First need to check is prefix project nodes has been applied for the migration
                if (_config.PrefixProjectToNodes)
                {
                    // if prefix project nodes has been applied we need to take the original area/iteration value and prefix
                    targetTSChild.Query =
                        targetTSChild.Project.CreateTestQuery(targetTSChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", sourceTS.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}\{1}", targetTestStore.Project.TeamProjectName, sourceTS.Plan.Project.TeamProjectName)));
                }
                else
                {
                    // If we are not profixing project nodes then we just need to take the old value for the project and replace it with the new project value
                    targetTSChild.Query = targetTSChild.Project.CreateTestQuery(targetTSChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", sourceTS.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}", targetTestStore.Project.TeamProjectName)));
                }

                // Send some traces.
                _mySource.Value.TraceInformation("New query is now {0}", targetTSChild.Query.QueryText);
                _mySource.Value.Flush();
            }
        }

        private void ValidateAndFixTestSuiteQuery(ITestSuiteBase sourceTS, IDynamicTestSuite targetTSChild, TestManagementContext targetTestStore)
        {
            try
            {
                // Verifying that the query is valid 
                targetTSChild.Query.Execute();
            }
            catch (Exception e)
            {
                FixIterationNotFound(e, sourceTS, targetTSChild, targetTestStore);
            }
            finally
            {
                targetTSChild.Repopulate();
            }
        }

        private void FixIterationNotFound(Exception exception, ITestSuiteBase sourceTS, IDynamicTestSuite targetTSChild, TestManagementContext targetTestStore)
        {
            if (exception.Message.Contains("The specified iteration path does not exist."))
            {
                Regex r = new Regex(@"'(.*?)'");

                var missingIterationPath = r.Match(exception.Message).Groups[0].Value;
                missingIterationPath = missingIterationPath.Substring(missingIterationPath.IndexOf(@"\") + 1, missingIterationPath.Length - missingIterationPath.IndexOf(@"\") - 2);

                // Send some traces.
                _mySource.Value.TraceInformation("Found a orphaned iteration path in test suite query.");
                _mySource.Value.TraceInformation("Invalid iteration path {0}:", missingIterationPath);
                _mySource.Value.TraceInformation("Replacing the orphaned iteration path from query with root iteration path. Please fix the query after the migration.");
                _mySource.Value.Flush();

                targetTSChild.Query = targetTSChild.Project.CreateTestQuery(
                    targetTSChild.Query.QueryText.Replace(
                        string.Format(@"'{0}\{1}'", sourceTS.Plan.Project.TeamProjectName, missingIterationPath),
                        string.Format(@"'{0}'", targetTestStore.Project.TeamProjectName)
                    ));

                targetTSChild.Query = targetTSChild.Project.CreateTestQuery(
                    targetTSChild.Query.QueryText.Replace(
                        string.Format(@"'{0}\{1}'", targetTestStore.Project.TeamProjectName, missingIterationPath),
                        string.Format(@"'{0}'", targetTestStore.Project.TeamProjectName)
                    ));
            }
        }

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Create a stop watch to measure the test plan execution time.
            Stopwatch TestPlanTimer = new Stopwatch();

            // Start timer.
            TestPlanTimer.Start();

            ITestPlanCollection sourcePlans = _sourceTestStore.GetTestPlans();

            // Send some traces.
            _mySource.Value.TraceInformation("Plan to copy {0} Plans?", sourcePlans.Count);
            _mySource.Value.Flush();

            if (_config.IsClone)
            {
                var mayNeed = "to do something";
            }

            // var currentTP = sourcePlans.Count; // total number of test plans
            int successfulTP = 0; // number of test plans succeeding
            ConcurrentBag<ITestPlan> failedTestPlans = new ConcurrentBag<ITestPlan>();

            foreach (ITestPlan sourcePlan in sourcePlans)
            {
                try
                {

                    if (CanSkipElementBecauseOfTags(sourcePlan.Id))
                        continue;

                    var newPlanName = _config.PrefixProjectToNodes
                        ? $"{_sourceWitStore.GetProject().Name}-{sourcePlan.Name}"
                        : $"{sourcePlan.Name}";

                    // Send some traces.
                    _mySource.Value.TraceInformation($"Process Plan {newPlanName}", Name);
                    _mySource.Value.Flush();

                    var targetPlan = FindTestPlan(_targetTestStore, newPlanName);
                    if (targetPlan == null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Plan missing... creating", Name);
                        _mySource.Value.Flush();

                        targetPlan = CreateNewTestPlanFromSource(sourcePlan, newPlanName);

                        RemoveInvalidLinks(targetPlan);

                        targetPlan.Save();

                        ApplyFieldMappings(sourcePlan.Id, targetPlan.Id);
                        AssignReflectedWorkItemId(sourcePlan.Id, targetPlan.Id);
                        FixAssignedToValue(sourcePlan.Id, targetPlan.Id);

                        ApplyDefaultConfigurations(sourcePlan.RootSuite, targetPlan.RootSuite);

                        ApplyFieldMappings(sourcePlan.RootSuite.Id, targetPlan.RootSuite.Id);
                        AssignReflectedWorkItemId(sourcePlan.RootSuite.Id, targetPlan.RootSuite.Id);
                        FixAssignedToValue(sourcePlan.RootSuite.Id, targetPlan.RootSuite.Id);
                        // Add Test Cases & apply configurations
                        AddChildTestCases(sourcePlan.RootSuite, targetPlan.RootSuite, targetPlan);
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Plan already found, not creating", Name);
                        _mySource.Value.Flush();
                    }
                    if (HasChildSuites(sourcePlan.RootSuite))
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation($"Source Plan has {sourcePlan.RootSuite.Entries.Count} Suites", Name);
                        _mySource.Value.Flush();

                        foreach (var sourceSuiteChild in sourcePlan.RootSuite.SubSuites)
                            ProcessTestSuite(sourceSuiteChild, targetPlan.RootSuite, targetPlan);
                    }

                    targetPlan.Save();

                    // Load the plan again, because somehow it doesn't let me set configurations on the already loaded plan
                    ITestPlan targetPlan2 = FindTestPlan(_targetTestStore, targetPlan.Name);
                    ApplyConfigurationsAndAssignTesters(sourcePlan.RootSuite, targetPlan2.RootSuite);
                    successfulTP++;
                }
                catch (Exception ex)
                {
                    failedTestPlans.Add(sourcePlan);
                    // Send some traces.
                    _mySource.Value.TraceInformation($"{sourcePlan.Id} - {sourcePlan.Name}: : Error{Environment.NewLine}{ex.Message}");
                    _mySource.Value.Flush();

                }
                _mySource.Value.TraceInformation($"{sourcePlan.Id} - {sourcePlan.Name}: Completed");
            }
                
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "TestPlansAndSuitesMigrationContext"; }
        }

        public TestPlandsAndSuitesMigrationContext(MigrationEngine me, TestPlansAndSuitesMigrationConfig config) : base(me)
        {
            _sourceWitStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            _sourceTestStore = new TestManagementContext(me.Source);
            _targetWitStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            _targetTestStore = new TestManagementContext(me.Target);
            _sourceTestConfigs = _sourceTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            _targetTestConfigs = _targetTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            _sourceIdentityManagementService = me.Source.Collection.GetService<IIdentityManagementService>();
            _targetIdentityManagementService = me.Target.Collection.GetService<IIdentityManagementService>();
            _config = config;
        }

        #endregion
    }
}