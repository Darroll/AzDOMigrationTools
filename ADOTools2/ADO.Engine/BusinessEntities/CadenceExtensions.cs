using System.Collections.Generic;
using System.Linq;

namespace ADO.Engine.BusinessEntities
{
    public static class CadenceExtensions
    {
        public static bool CheckHasNoCadenceOverlap(this IEnumerable<Cadence> cadences)
        {
            List<Cadence> sortedCadences = cadences.Select(a => a).ToList();
            sortedCadences.Sort(new CadenceDateComparer());
            if (sortedCadences.Count <= 1)
            {
                return false;
            }
            else
            {
                var previousCadence = sortedCadences.First();
                var nextCadence = sortedCadences.Skip(1).First();
                if (nextCadence.CadenceStart <= previousCadence.CadenceEnd)
                    return false;
                foreach (var cadence in sortedCadences.Skip(2))
                {
                    previousCadence = nextCadence;
                    nextCadence = cadence;
                    if (nextCadence.CadenceStart <= previousCadence.CadenceEnd)
                        return false;
                }
                return true;
            }
        }
    }
}
