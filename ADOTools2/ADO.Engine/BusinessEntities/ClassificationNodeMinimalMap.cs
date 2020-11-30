using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ADO.Engine.BusinessEntities
{
    public class ClassificationNodeMinimalMap : IClassificationNodeMinimalMap
    {
        private Dictionary<Tuple<string, string>, SimpleMutableClassificationNodeMinimalWithIdNode> internalMap
            = new Dictionary<Tuple<string, string>, SimpleMutableClassificationNodeMinimalWithIdNode>();
        
        public static ClassificationNodeMinimalMap NewClassificationNodeMinimalMap()
        {
            ClassificationNodeMinimalMap newClassificationNodeMinimalMap = new ClassificationNodeMinimalMap();
            return newClassificationNodeMinimalMap;
        }

        public void AddClassificationNodeMinimal(string organization, string project, SimpleMutableClassificationNodeMinimalWithIdNode classificationNodeMinimal)
        {
            if (internalMap.ContainsKey(new Tuple<string, string>(organization, project)))
            {
                throw new KeyNotFoundException($"entry with key {organization},{project} already exists");
            }
            else
            {
                internalMap[new Tuple<string, string>(organization, project)] = classificationNodeMinimal;
            }
        }

        public SimpleMutableClassificationNodeMinimalWithIdNode GetClassificationNodeMinimal(string organization, string project)
        {
            if (!internalMap.ContainsKey(new Tuple<string, string>(organization, project)))
            {
                throw new KeyNotFoundException($"entry with key {organization},{project} already exists");
            }
            else
            {
                return internalMap[new Tuple<string, string>(organization, project)];
            }
        }
    }
}
