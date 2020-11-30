using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class AttachementImportMigrationContext : AttachementMigrationContextBase
    {
        #region - Static Declarations
        // Create a trace source.

        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.AttachementImportMigrationContext"));

        #endregion

        #region - Private Members

        private readonly AttachementImportMigrationConfig _config;

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            // Create a stop watch to measure the query execution time.
            Stopwatch queryTimer = new Stopwatch();

            // Start timer.
            queryTimer.Start();

            // Get destination/target work item store.
            WorkItemStoreContext targetStore = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);

            // Find what is the destination project.
            Project destProject = targetStore.GetProject();

            // Send some traces.
            _mySource.Value.TraceInformation("Found target project as {0}", destProject.Name);
            _mySource.Value.Flush();

            List<string> files = Directory.EnumerateFiles(ExportPath).ToList<string>();
            WorkItem targetWI = null;
            int currentFiles = files.Count;
            int failures = 0;
            int skipped = 0;

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                try
                {
                    var fileNameParts = fileName.Split('#');
                    if (fileNameParts.Length != 2 || !int.TryParse(fileNameParts[0], out var sourceReflectedID))
                        continue;

                    var targetFileName = fileNameParts[1];
                    var renamedFilePath = Path.Combine(Path.GetDirectoryName(file), targetFileName);
                    File.Move(file, renamedFilePath);
                    targetWI = targetStore.FindReflectedWorkItemByReflectedWorkItemId(sourceReflectedID, Engine.ReflectedWorkItemIdFieldName, true);
                    if (targetWI != null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("{0} of {1} - Import {2} to {3}", currentFiles, files.Count, fileName, targetWI.Id);
                        _mySource.Value.Flush();

                        var attachments = targetWI.Attachments.Cast<Attachment>();
                        var attachment = attachments.Where(a => a.Name == targetFileName).FirstOrDefault();
                        if (attachment == null)
                        {
                            Attachment a = new Attachment(renamedFilePath);
                            targetWI.Attachments.Add(a);
                            targetWI.Save();
                        }
                        else
                        {
                            // Send some traces.
                            _mySource.Value.TraceInformation("[SKIP] WorkItem {0} already contains attachment {1}", targetWI.Id, fileName);
                            _mySource.Value.Flush();

                            // Increment skip counter.
                            skipped++;
                        }
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("{0} of {1} - Skipping {2} to {3}", currentFiles, files.Count, fileName, 0);
                        _mySource.Value.Flush();

                        // Increment skip counter.
                        skipped++;
                    }
                    
                    var fileDeleted = false;
                    for (var i = 0; i < 10; i++)
                    {
                        try
                        {
                            File.Delete(renamedFilePath);
                            fileDeleted = true;
                            break;
                        }
                        catch
                        {
                            _mySource.Value.TraceInformation($"Failed to delete file {renamedFilePath}");
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    if (!fileDeleted)
                    {
                        var dummyFilePath = Path.Combine(Path.GetDirectoryName(file), $"deleteme_{targetFileName}");
                        File.Move(renamedFilePath, dummyFilePath);
                    }
                }
                catch (FileAttachmentException ex)
                {
                    // Probably due to attachment being over size limit

                    // Send some traces.
                    _mySource.Value.TraceInformation(ex.Message);
                    _mySource.Value.Flush();

                    // Increment failure counter.
                    failures++;
                }
                catch (Exception ex)
                {
                    // e.g. Cannot create a file when that file already exists.

                    // Send some traces.
                    _mySource.Value.TraceInformation(ex.Message);
                    _mySource.Value.Flush();

                    // Increment failure counter.
                    failures++;
                }

                // Decrement counter.
                currentFiles--;
            }

            // Stop timer.
            queryTimer.Stop();

            // Send some traces.
            _mySource.Value.TraceInformation(@"IMPORT DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds - {4} Files, {1} Files imported, {2} Failures, {3} Skipped", queryTimer.Elapsed, (files.Count - failures - skipped), failures, skipped, files.Count);
            _mySource.Value.Flush();
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "AttachementImportMigrationContext"; }
        }

        public AttachementImportMigrationContext(MigrationEngine me, AttachementImportMigrationConfig config) : base(me)
        {
            _config = config;
        }

        #endregion

    }
}