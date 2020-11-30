using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.TeamFoundation.Git.Client;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class FixGitCommitLinks : ProcessingContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.FixGitCommitLinks"));

        #endregion

        #region - Private Members

        private readonly FixGitCommitLinksConfig _config;

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Create a stop watch to measure the query execution time.
            Stopwatch queryTimer = new Stopwatch();

            // Create a stop watch to measure the change against a work item.
            Stopwatch modificationTimer = new Stopwatch();
            
            // Start timer.
            queryTimer.Start();
            
            var sourceGitRepoService = Engine.Source.Collection.GetService<GitRepositoryService>();
            var sourceGitRepos = sourceGitRepoService.QueryRepositories(Engine.Source.Name);

            var targetGitRepoService = Engine.Target.Collection.GetService<GitRepositoryService>();
            var targetGitRepos = targetGitRepoService.QueryRepositories(Engine.Target.Name);

            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);

            // Create a new query context.
            TfsQueryContext tfsqc = new TfsQueryContext(targetStore);

            // Add parameters to context.
            tfsqc.AddParameter("TeamProject", Engine.Target.Name);

            // Set the query.
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject");

            // Execute.
            WorkItemCollection workitems = tfsqc.Execute();

            // Send some traces.
            _mySource.Value.TraceInformation("Update {0} work items?", workitems.Count);
            _mySource.Value.Flush();

            //////////////////////////////////////////////////
            int currentWI = workitems.Count;
            int countWI = 0;
            long timeElapsed = 0;
            int noteFound = 0;

            foreach (WorkItem workitem in workitems)
            {
                // Reset and start.
                modificationTimer.Reset();
                modificationTimer.Start();

                workitem.Open();
                List<ExternalLink> newEL = new List<ExternalLink>();
                List<ExternalLink> removeEL = new List<ExternalLink>();

                // Send some traces.
                _mySource.Value.TraceInformation("WI: {0}?", workitem.Id);
                _mySource.Value.Flush();

                List<string> gitWits = new List<string>
                {
                    "Branch",
                    "Fixed in Commit",
                    "Pull Request"
                };

                foreach (Link l in workitem.Links)
                {
                    if (l is ExternalLink && gitWits.Contains(l.ArtifactLinkType.Name))
                    {
                        ExternalLink el = (ExternalLink)l;
                        //vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327%2f41dd2754058348d72a6417c0615c2543b9b55535
                        string guidbits = el.LinkedArtifactUri.Substring(el.LinkedArtifactUri.LastIndexOf('/') + 1);
                        string[] bits = Regex.Split(guidbits, "%2f", RegexOptions.IgnoreCase);
                        string oldCommitId = null;
                        string oldGitRepoId = bits[1];
                        if (bits.Count() >= 3)
                        {
                            oldCommitId = $"{bits[2]}";
                            for (int i = 3; i < bits.Count(); i++)
                            {
                                oldCommitId += $"%2f{bits[i]}";
                            }
                        }
                        else
                        {
                            oldCommitId = bits[2];
                        }
                        var oldGitRepo =
                            (from g in sourceGitRepos where g.Id.ToString() == oldGitRepoId select g)
                            .SingleOrDefault();

                        if (oldGitRepo != null)
                        {
                            // Find the target git repo
                            GitRepository newGitRepo = null;
                            var repoNameToLookFor = !string.IsNullOrEmpty(_config.TargetRepository)
                                ? _config.TargetRepository
                                : oldGitRepo.Name;

                            // Source and Target project names match
                            if (oldGitRepo.ProjectReference.Name == Engine.Target.Name)
                            {
                                newGitRepo = (from g in targetGitRepos
                                              where
                                              g.Name == repoNameToLookFor &&
                                              g.ProjectReference.Name == oldGitRepo.ProjectReference.Name
                                              select g).SingleOrDefault();
                            }
                            // Source and Target project names do not match
                            else
                            {
                                newGitRepo = (from g in targetGitRepos
                                              where
                                              g.Name == repoNameToLookFor &&
                                              g.ProjectReference.Name != oldGitRepo.ProjectReference.Name
                                              select g).SingleOrDefault();
                            }

                            // Fix commit links if target repo has been found
                            if (newGitRepo != null)
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"Fixing {oldGitRepo.RemoteUrl} to {newGitRepo.RemoteUrl}?");
                                _mySource.Value.Flush();

                                // Create External Link object
                                ExternalLink newLink = null;
                                switch (l.ArtifactLinkType.Name)
                                {
                                    case "Branch":
                                        newLink = new ExternalLink(targetStore.Store.RegisteredLinkTypes[ArtifactLinkIds.Branch],
                                            $"vstfs:///git/ref/{newGitRepo.ProjectReference.Id}%2f{newGitRepo.Id}%2f{oldCommitId}");
                                        break;

                                    case "Fixed in Commit":
                                        newLink = new ExternalLink(targetStore.Store.RegisteredLinkTypes[ArtifactLinkIds.Commit],
                                            $"vstfs:///git/commit/{newGitRepo.ProjectReference.Id}%2f{newGitRepo.Id}%2f{oldCommitId}");
                                        break;
                                    case "Pull Request":
                                        newLink = new ExternalLink(targetStore.Store.RegisteredLinkTypes[ArtifactLinkIds.PullRequest],
                                            $"vstfs:///Git/PullRequestId/{newGitRepo.ProjectReference.Id}%2f{newGitRepo.Id}%2f{oldCommitId}");
                                        break;

                                    default:

                                        // Send some traces.
                                        _mySource.Value.TraceInformation("Skipping unsupported link type {0}", l.ArtifactLinkType.Name);
                                        _mySource.Value.Flush();

                                        break;
                                }

                                if (newLink != null)
                                {
                                    var elinks = from Link lq in workitem.Links
                                                 where gitWits.Contains(lq.ArtifactLinkType.Name)
                                                 select (ExternalLink)lq;
                                    var found =
                                    (from Link lq in elinks
                                     where (((ExternalLink)lq).LinkedArtifactUri.ToLower() == newLink.LinkedArtifactUri.ToLower())
                                     select lq).SingleOrDefault();
                                    if (found == null)
                                    {
                                        newEL.Add(newLink);
                                    }
                                    removeEL.Add(el);
                                }
                            }
                            else
                            {
                                // Send some traces.
                                _mySource.Value.TraceInformation($"FAIL: cannot map {oldGitRepo.RemoteUrl} to ???");
                                _mySource.Value.Flush();
                            }
                        }
                        else
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation($"FAIL could not find source git repo");
                            _mySource.Value.Flush();

                            noteFound++;
                        }
                    }
                }
                // add and remove
                foreach (ExternalLink eln in newEL)
                {
                    try
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Adding " + eln.LinkedArtifactUri, Name);
                        _mySource.Value.Flush();
                        workitem.Links.Add(eln);

                    }
                    catch (Exception)
                    {

                        // eat exception as sometimes TFS thinks this is an attachment
                    }
                }
                foreach (ExternalLink elr in removeEL)
                {
                    if (workitem.Links.Contains(elr))
                    {
                        try
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("Removing " + elr.LinkedArtifactUri, Name);
                            _mySource.Value.Flush();

                            workitem.Links.Remove(elr);
                        }
                        catch (Exception)
                        {

                            // eat exception as sometimes TFS thinks this is an attachment
                        }
                    }
                }

                if (workitem.IsDirty)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"Saving {workitem.Id}");
                    _mySource.Value.Flush();

                    workitem.Save();
                }

                // Stop timer.
                modificationTimer.Stop();

                // Decrement number of work items to process.
                currentWI--;

                // Increment counter.
                countWI++;

                // Calculate stats.
                timeElapsed += modificationTimer.ElapsedMilliseconds;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(timeElapsed / countWI));
                string averageAsString = string.Format(@"{0:s\:fff} seconds", average);
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * currentWI));

                // Calculate the time to completion.
                string estimatedTimeToCompletion = string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining);

                // Send some traces.
                _mySource.Value.TraceInformation("Average time of {0} per work item and {1} estimated to completion", averageAsString, estimatedTimeToCompletion);
                _mySource.Value.Flush();
            }

            // Send some traces.
            _mySource.Value.TraceInformation("Did not find old repo for {0} links?", noteFound);
            _mySource.Value.Flush();

            // Stop timer.
            queryTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", queryTimer.Elapsed);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "FixGitCommitLinks"; }
        }

        public FixGitCommitLinks(MigrationEngine me, FixGitCommitLinksConfig config ) : base(me)
        {
            _config = config;
        }

        #endregion
    }
}