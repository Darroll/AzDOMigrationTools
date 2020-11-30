using ADO.Engine.BusinessEntities;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace ADO.Engine.Utilities
{
    //Dictionary<TripleKey, ClassificationNodeMinimalWithIdItem>

    public class TripleKeyTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var elements = Convert.ToString(value).Trim('(').Trim(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            elements = elements.Select(elt => elt.Trim()).ToArray();
            return new TripleKey(elements[0], elements[1], elements[2]);
        }
    }

    //public class TripleKeyConverter : JsonConverter
    //{
    //    /// <summary>
    //    /// Override ReadJson to read the dictionary key and value
    //    /// </summary>
    //    /// <param name="reader"></param>
    //    /// <param name="objectType"></param>
    //    /// <param name="existingValue"></param>
    //    /// <param name="serializer"></param>
    //    /// <returns></returns>
    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        TripleKey _tuple = null;
    //        ClassificationNodeMinimalWithIdItem _value = null;
    //        var _dict = new Dictionary<TripleKey, ClassificationNodeMinimalWithIdItem>();

    //        //loop through the JSON string reader
    //        while (reader.Read())
    //        {
    //            // check whether it is a property
    //            if (reader.TokenType == JsonToken.PropertyName)
    //            {
    //                string readerValue = reader.Value.ToString();
    //                if (reader.Read())
    //                {
    //                    // check if the property is tuple (Dictionary key)
    //                    if (readerValue.StartsWith("(") && readerValue.EndsWith(")"))
    //                    {
    //                        string[] result = ConvertTuple(readerValue);

    //                        if (result == null)
    //                            continue;

    //                        // Custom Deserialize the Dictionary key (Tuple)
    //                        _tuple = new TripleKey(result[0], result[1], result[2]);

    //                        // Custom Deserialize the Dictionary value
    //                        _value = (ClassificationNodeMinimalWithIdItem)serializer.Deserialize(reader, typeof(ClassificationNodeMinimalWithIdItem));

    //                        _dict.Add(_tuple, _value);
    //                    }
    //                    else
    //                    {
    //                        // Deserialize the remaining data from the reader
    //                        //serializer.Deserialize(reader);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //        return _dict;
    //    }

    //    /// <summary>
    //    /// To convert Tuple
    //    /// </summary>
    //    /// <param name="_string"></param>
    //    /// <returns></returns>
    //    public string[] ConvertTuple(string _string)
    //    {
    //        if (string.IsNullOrEmpty(_string))
    //            return null;
    //        var elements = Convert.ToString(_string).Trim('(').Trim(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //        return elements.Select(elt => elt.Trim()).ToArray();
    //    }

    //    /// <summary>
    //    /// WriteJson needs to be implemented since it is an abstract function.
    //    /// </summary>
    //    /// <param name="writer"></param>
    //    /// <param name="value"></param>
    //    /// <param name="serializer"></param>
    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        serializer.Serialize(writer, value);
    //    }

    //    /// <summary>
    //    /// Check whether to convert or not
    //    /// </summary>
    //    /// <param name="objectType"></param>
    //    /// <returns></returns>
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return true;
    //    }
    //}
}
