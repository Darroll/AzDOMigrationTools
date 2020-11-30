using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.Engine.Utilities
{
    public static class JsonExtensions
    {
        public static void SaveToJson<T>(this T dataRoot, string path)
        {
            string json = JsonConvert.SerializeObject(dataRoot, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static T LoadFromJson<T>(this string path)
        {
            string json = File.ReadAllText(path);
            T classificationNodeMinimal = JsonConvert.DeserializeObject<T>(json);
            return classificationNodeMinimal;
        }
    }
}
