using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class TestConfigurationsMigrationContext : MigrationContextBase
    {
        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/

        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.TestConfigurationsMigrationContext"));

        #endregion

        #region - Private Members

        private ITestConfiguration GetTestConfiguration(ITestConfigurationHelper tch, string configToFind)
        {
            // Test configurations are case insensitive in VSTS so need ignore case in comparison
            return tch.Query("Select * From TestConfiguration").FirstOrDefault(variable => string.Equals(variable.Name, configToFind, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            TestManagementContext SourceTmc = new TestManagementContext(Engine.Source);
            TestManagementContext targetTmc = new TestManagementContext(Engine.Target);

            ITestConfigurationCollection tcc = SourceTmc.Project.TestConfigurations.Query("Select * From TestConfiguration");

            // Send some traces.
            _mySource.Value.TraceInformation($"Plan to copy {tcc.Count} Configurations", Name);
            _mySource.Value.Flush();

            foreach (var sourceTestConf in tcc)
            {
                // Send some traces.
                _mySource.Value.TraceInformation($"{sourceTestConf.Name} - Copy Configuration", Name);
                _mySource.Value.Flush();

                ITestConfiguration targetTc = GetTestConfiguration(targetTmc.Project.TestConfigurations, sourceTestConf.Name);
                if (targetTc != null)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation($"{sourceTestConf.Name} - Found", Name);
                    _mySource.Value.Flush();

                    // Move on
                }
                else
                {

                    // Send some traces.
                    _mySource.Value.TraceInformation($"{sourceTestConf.Name} - Create new", Name);
                    _mySource.Value.Flush();

                    targetTc = targetTmc.Project.TestConfigurations.Create();
                    targetTc.AreaPath = sourceTestConf.AreaPath.Replace(Engine.Source.Name, Engine.Target.Name);
                    targetTc.Description = sourceTestConf.Description;
                    targetTc.IsDefault = sourceTestConf.IsDefault;
                    targetTc.Name = sourceTestConf.Name;

                    foreach (var val in sourceTestConf.Values)
                        if (!targetTc.Values.ContainsKey(val.Key))
                            targetTc.Values.Add(val);

                    targetTc.State = sourceTestConf.State;
                    targetTc.Save();

                    // Send some traces.
                    _mySource.Value.TraceInformation($"{sourceTestConf.Name} - Saved as {targetTc.Name}", Name);
                    _mySource.Value.Flush();
                }
            }
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "TestConfigurationsMigrationContext"; }
        }

        public TestConfigurationsMigrationContext(MigrationEngine me, TestConfigurationsMigrationConfig config) : base(me)
        {

        }

        #endregion
    }
}