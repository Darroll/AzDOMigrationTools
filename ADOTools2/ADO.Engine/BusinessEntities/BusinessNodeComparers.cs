using System;
using System.Collections.Generic;

namespace ADO.Engine.BusinessEntities
{
    public class BusinessNodeNameComparer : IComparer<BusinessNode>
    {
        public int Compare(BusinessNode x, BusinessNode y)
        {
            return String.CompareOrdinal(x.Name, y.Name);
        }
    }
    public class CadenceDateComparer : IComparer<Cadence>
    {
        public int Compare(Cadence x, Cadence y)
        {
            if (x.CadenceStart < y.CadenceStart)
            {
                return -1;
            }
            else if (x.CadenceStart > y.CadenceStart)
            {
                return 1;
            }
            else
            {
                //x and y start at the same date
                if (x.CadenceEnd == y.CadenceEnd)
                {
                    return 0;
                }
                else if (x.CadenceEnd < y.CadenceEnd)
                {
                    return -1;
                }
                else
                {
                    //x.CadenceEnd>y.CadenceEnd
                    return 1;
                }
            }
        }
    }
}
