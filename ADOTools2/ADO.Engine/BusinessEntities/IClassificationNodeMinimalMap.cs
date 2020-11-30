using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.Engine.BusinessEntities
{
    public interface IClassificationNodeMinimalMap
    {
        SimpleMutableClassificationNodeMinimalWithIdNode GetClassificationNodeMinimal(string organization, string project);
        void AddClassificationNodeMinimal(string organization, string project, SimpleMutableClassificationNodeMinimalWithIdNode classificationNodeMinimal);
    }
}
