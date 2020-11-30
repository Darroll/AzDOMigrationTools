using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Core.BusinessEntities
{
    public class SerializableClassificationNodeMapWithCache
    {
        public Dictionary<string, string> Map { get; set; }
        public SerializableClassificationNodeMapWithCache()
        {
            this.Map = new Dictionary<string, string>();
        }

        public string GetMappedPath(string areaOrIterationPath, bool removeStructureType, string structureType)
        {
            if (this.Map.ContainsKey(areaOrIterationPath))
            {
                var mappedPath = this.Map[areaOrIterationPath];
                if (removeStructureType)
                {
                    mappedPath = RemoveStructureType(mappedPath, structureType);
                }
                return mappedPath;
            }
            else
            {
                var decreasingMap = this.Map.OrderByDescending(entry => entry.Key).ToDictionary(ent=>ent.Key,ent=>ent.Value);
                var hasMatch = decreasingMap.Any(ent => areaOrIterationPath.StartsWith(ent.Key));
                if (hasMatch)
                {
                    var firstEntry = decreasingMap.First(ent => areaOrIterationPath.StartsWith(ent.Key));
                    KeyValuePair<string,string> newEntry = new KeyValuePair<string, string>(areaOrIterationPath, $"{firstEntry.Value}{areaOrIterationPath.Substring(firstEntry.Key.Length)}");
                    this.Map.Add(newEntry.Key, newEntry.Value);
                    var mappedPath = this.Map[areaOrIterationPath];
                    if (removeStructureType)
                    {
                        mappedPath = RemoveStructureType(mappedPath, structureType);
                    }
                    return mappedPath;
                }
                else
                {
                    throw new ArgumentException($"Incomplete map. Could not find a key for which {areaOrIterationPath} is under");
                }                
            }
        }

        private string RemoveStructureType(string mappedPath, string structureType)
        {
            if (mappedPath.Contains(structureType))
            {
                var parts = mappedPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                var noStructure = new List<string>() { parts[0] }.Concat(parts.Skip(2));
                return String.Join("\\", noStructure);
            }
            else
            {
                return mappedPath;
            }
        }
    }
}
