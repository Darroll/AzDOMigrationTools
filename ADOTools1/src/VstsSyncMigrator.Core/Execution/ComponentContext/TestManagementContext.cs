using System.Linq;
using System.Collections.Generic;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class TestManagementContext
    {
        #region - Private Members

        private readonly ITeamProjectContext _source;
        private readonly ITestManagementService _tms;
        private readonly ITestManagementTeamProject _project;

        #endregion

        #region - Internal Members

        internal ITestManagementTeamProject Project
        {
            get { return _project; }
        }

        internal ITestPlanCollection GetTestPlans()
        {
            return _project.TestPlans.Query("Select * From TestPlan");
        }

        internal List<ITestRun> GetTestRuns()
        {
            return _project.TestRuns.Query("Select * From TestRun").ToList();
        }

        internal ITestPlan CreateTestPlan()
        {
            return _project.TestPlans.Create();
        }

        #endregion

        #region - Public Members

        public TestManagementContext(ITeamProjectContext source)
        {
            _source = source;
            _tms = (ITestManagementService)source.Collection.GetService(typeof(ITestManagementService));
            _project = _tms.GetTeamProject(source.Name);
        }

        #endregion
    }
}