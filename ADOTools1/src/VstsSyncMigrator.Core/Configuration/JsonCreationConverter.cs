using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VstsSyncMigrator.Engine.Configuration
{
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="o">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create(Type objectType, JObject o);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken j = JToken.FromObject(value);
            if (j.Type != JTokenType.Object)
                j.WriteTo(writer);
            else
            {
                JObject o = (JObject)j;
                o.AddFirst(new JProperty("ObjectType", value.GetType().FullName));
                o.WriteTo(writer);
            }
        }
    }
}