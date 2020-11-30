using System;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessHierarchyNameComparer : IComparer<BusinessHierarchy>
    {
        public int Compare(BusinessHierarchy x, BusinessHierarchy y)
        {
            return String.CompareOrdinal(x.Name, y.Name);
        }
    }
    //public class ProgramOrProductNameComparer : IComparer<ProgramOrProduct>
    //{
    //    public int Compare(ProgramOrProduct x, ProgramOrProduct y)
    //    {
    //        return String.CompareOrdinal(x.Name, y.Name);
    //    }
    //}
    //public class TeamProjectNameComparer : IComparer<TeamProject>
    //{
    //    public int Compare(TeamProject x, TeamProject y)
    //    {
    //        return String.CompareOrdinal(x.ProjectName, y.ProjectName);
    //    }
    //}
}
