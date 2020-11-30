using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.TeamFoundation.TestManagement.Client;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine
{
    public class TestVariablesMigrationContext : MigrationContextBase
    {
        #region - Static Declarations

        // Create a trace source.
        private static readonly Lazy<TraceSource> _mySource = new Lazy<TraceSource>(() => Tracing.Create("Migration.Engine.TestVariablesMigrationContext"));

        #endregion

        #region - Internal Members

        internal override void InternalExecute()
        {
            //WorkItemStoreContext sourceWisc = new WorkItemStoreContext(Engine.Source, WorkItemStoreFlags.None);
            TestManagementContext SourceTmc = new TestManagementContext(Engine.Source);

            //WorkItemStoreContext targetWisc = new WorkItemStoreContext(Engine.Target, WorkItemStoreFlags.BypassRules);
            TestManagementContext targetTmc = new TestManagementContext(Engine.Target);

            List<ITestVariable> sourceVars = SourceTmc.Project.TestVariables.Query().ToList();

            // Send some traces.
            _mySource.Value.TraceInformation("Plan to copy {0} Veriables?", sourceVars.Count);
            _mySource.Value.Flush();

            foreach (var sourceVar in sourceVars)
            {
                // Send some traces.
                _mySource.Value.TraceInformation("Copy: {0}", sourceVar.Name);
                _mySource.Value.Flush();

                ITestVariable targetVar = GetVar(targetTmc.Project.TestVariables, sourceVar.Name);
                if (targetVar == null)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Need to create: {0}", sourceVar.Name);
                    _mySource.Value.Flush();

                    targetVar = targetTmc.Project.TestVariables.Create();
                    targetVar.Name = sourceVar.Name;
                    targetVar.Save();
                }
                else
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Exists: {0}", sourceVar.Name);
                    _mySource.Value.Flush();
                }
                // match values
                foreach (var sourceVal in sourceVar.AllowedValues)
                {
                    // Send some traces.
                    _mySource.Value.TraceInformation("Seeking: {0}", sourceVal.Value);
                    _mySource.Value.Flush();

                    ITestVariableValue targetVal = GetVal(targetVar, sourceVal.Value);
                    if (targetVal == null)
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Need to create: {0}", sourceVal.Value);
                        _mySource.Value.Flush();

                        targetVal = targetTmc.Project.TestVariables.CreateVariableValue(sourceVal.Value);
                        targetVar.AllowedValues.Add(targetVal);
                        targetVar.Save();
                    }
                    else
                    {
                        // Send some traces.
                        _mySource.Value.TraceInformation("Exists: {0}", targetVal.Value);
                        _mySource.Value.Flush();
                    }
                }
            }
        }

        internal ITestVariable GetVar(ITestVariableHelper tvh, string variableToFind)
        {
            // Test Variables are case insensitive in VSTS so need ignore case in comparison
            return tvh.Query().FirstOrDefault(variable => string.Equals(variable.Name, variableToFind, StringComparison.OrdinalIgnoreCase));
        }

        internal ITestVariableValue GetVal(ITestVariable targetVar, string valueToFind)
        {
            // Test Variable values are case insensitive in VSTS so need ignore case in comparison
            return targetVar.AllowedValues.FirstOrDefault(variable => string.Equals(variable.Value, valueToFind, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region - Public Members

        public override string Name
        {
            get { return "TestVariablesMigrationContext"; }
        }

        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/
        public TestVariablesMigrationContext(MigrationEngine me) : base(me)
        {

        }

        #endregion
    }
}
