using ADO.Engine.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using VstsSyncMigrator.Core.BusinessEntities;

namespace ADO.Engine.BusinessEntities
{
    public class SerializableClassificationNodeMap
    {
        public Dictionary<TripleKey, ClassificationNodeMinimalWithIdItem> Map { get; set; }
        public SerializableClassificationNodeMap()
        {
            this.Map = new Dictionary<TripleKey, ClassificationNodeMinimalWithIdItem>();
        }

        //public bool ContainsKey(string organization, string project, string sourcePath)
        //{
        //    return this.Map.ContainsKey(new Tuple<string, string, string>(organization, project, sourcePath));
        //}

        //public ClassificationNodeMinimalWithIdItem GetMappedItem(string organization, string project, string key)
        //{
        //    return this.Map[new Tuple<string, string, string>(organization, project, key)];
        //}

        //public void SetMappedItem(string organization, string project, string key, ClassificationNodeMinimalWithIdItem item)
        //{
        //    if (this.Map == null)
        //        this.Map = new Dictionary<Tuple<string, string, string>, ClassificationNodeMinimalWithIdItem>();
        //    this.Map[new Tuple<string, string, string>(organization, project, key)] = item;
        //}

        public static SerializableClassificationNodeMap LoadFromJson(string universalAreaMaps)
        {
            if (string.IsNullOrEmpty(universalAreaMaps))
                return null;
            if (!File.Exists(universalAreaMaps))
                return null;

            //TypeDescriptor.AddAttributes(typeof(TripleKey), new TypeConverterAttribute(typeof(TripleKeyTypeConverter)));
            string json = File.ReadAllText(universalAreaMaps);
            var result = JsonConvert.DeserializeObject<SerializableClassificationNodeMap>(json);
            return result;
        }

        public bool HasMappedClassificationNodeForOrganizationalProject(string organizationName, string projectName)
        {
            return this.Map.Keys.Any(key => key.Organization == organizationName && key.Project == projectName);
        }

        public SerializableClassificationNodeMapWithCache GetMappedClassificationNodeListForOrganizationalProject(string organizationName, string projectName)
        {
            if (!this.HasMappedClassificationNodeForOrganizationalProject(organizationName, projectName))
                return null;

            var mapWithCache = new SerializableClassificationNodeMapWithCache();
            mapWithCache.Map = this.Map.Where(entry => entry.Key.Organization == organizationName && entry.Key.Project == projectName).ToDictionary(ent => ent.Key.Path, ent => ent.Value.Path);
            return mapWithCache;
        }
    }
}