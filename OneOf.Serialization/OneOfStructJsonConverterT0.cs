using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneOf.Serialization
{

    public class OneOfStructJsonConverter<T0> : JsonConverter
    {
        private static readonly Type OneOfType = typeof(OneOf<T0>);

        private static readonly Type OneOfT0Type = typeof(T0);
        private static readonly OneOfArgToken OneOfT0Token;

        static OneOfStructJsonConverter()
        {
            OneOfT0Token = OneOfJsonConverterCommon.GetOneOfArgToken(OneOfT0Type);
        }

        private static int GetOneOfArgIndex(OneOfArgToken oneOfArgToken)
        {
            return (oneOfArgToken == OneOfT0Token) ? 0 : -1;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is OneOf<T0> v)
            {
                serializer.Serialize(writer, v.Value);
            }
            else if (value is IEnumerable<OneOf<T0>> en)
            {
                var values = new object[en.Count()];
                int i = 0;
                foreach (var oneOf in en)
                    values[i++] = oneOf.Value;
                serializer.Serialize(writer, values);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == null)
                return existingValue;

            if (OneOfJsonConverterCommon.CanConvertDirectly(objectType, OneOfType))
            {
                switch (reader.TokenType)
                {
                case JsonToken.StartObject:
                case JsonToken.Integer:
                case JsonToken.Float: 
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Date:
                    var jToken = JToken.Load(reader);
                    var oneOf = DeserializeToOneOf(jToken);
                    if (oneOf != null)
                        return oneOf;
                    break;
                }
            }
            else if (reader.TokenType == JsonToken.StartArray &&
                     OneOfJsonConverterCommon.CanConvertByEnumerating(objectType, OneOfType))
            {
                var jArray = JArray.Load(reader);
                var oneOfList = new List<OneOf<T0>> (jArray.Count);
                foreach (var jToken in jArray)
                {
                    var oneOf = DeserializeToOneOf (jToken);
                    if (oneOf != null)
                        oneOfList.Add (oneOf.Value);
                }
                return oneOfList;
            }

            return existingValue;
        }

        private OneOf<T0>? DeserializeToOneOf(JToken jToken)
        {
            var oneOfArgToken = OneOfJsonConverterCommon.MapJTokenTypeToOneOfArgToken(jToken.Type);
            if (oneOfArgToken == OneOfArgToken.None)
                return null;

            var oneOfArgIndex = GetOneOfArgIndex(oneOfArgToken);
            if (oneOfArgIndex < 0)
                return null;

            object oneOfArg;
            switch (oneOfArgToken)
            {
            case OneOfArgToken.Object:
                oneOfArg = JsonConvert.DeserializeObject(jToken.ToString(), OneOfT0Type);
                return CreateOneOfInstance(oneOfArg);
            case OneOfArgToken.StringOrEnum:
                return DeserializeToOneOfWithStringOrEnumArg(jToken.ToString());
            case OneOfArgToken.Integer:
            case OneOfArgToken.Float:
            case OneOfArgToken.Boolean:
            case OneOfArgToken.Date:
                oneOfArg = Convert.ChangeType(jToken, OneOfT0Type);
                return CreateOneOfInstance(oneOfArg);
            }

            return null;
        }

        private OneOf<T0> DeserializeToOneOfWithStringOrEnumArg(string oneOfArg)
        {
            if (OneOfT0Type.IsEnum)
            {
                var enumValue = JsonConvert.DeserializeObject('"' + oneOfArg + '"', OneOfT0Type);
                return CreateOneOfInstance(enumValue);
            }

            // The current OneOf<> generic argument (Tn) is of type string
            return CreateOneOfInstance(oneOfArg);
        }

        private OneOf<T0> CreateOneOfInstance(object oneOfArg)
        {
            return (OneOf<T0>)(T0)oneOfArg;
        }

        public override bool CanConvert(Type objectType) =>
            OneOfJsonConverterCommon.CanConvertDirectly(objectType, OneOfType) ||
            OneOfJsonConverterCommon.CanConvertByEnumerating(objectType, OneOfType);
    }
}