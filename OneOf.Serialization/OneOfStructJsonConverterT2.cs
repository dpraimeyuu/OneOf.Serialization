using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OneOf.Serialization
{

    public class OneOfStructJsonConverter<T0, T1, T2> : JsonConverter
    {
        private static readonly Type OneOfType = typeof(OneOf<T0, T1, T2>);

        private static readonly Type[] OneOfArgTypes = new Type[]
        {
            typeof(T0),
            typeof(T1),
            typeof(T2),
        };
        private static readonly OneOfArgToken[] OneOfArgTokens = new OneOfArgToken[OneOfArgTypes.Length];

        static OneOfStructJsonConverter()
        {
            for (int i = 0, num = OneOfArgTypes.Length; i < num; ++i)
                OneOfArgTokens[i] = OneOfJsonConverterCommon.GetOneOfArgToken(OneOfArgTypes[i]);
        }

        private static int GetOneOfArgIndex(OneOfArgToken oneOfArgToken)
        {
            for (int i = 0, num = OneOfArgTypes.Length; i < num; ++i)
                if (oneOfArgToken == OneOfArgTokens[i])
                    return i;
            return -1;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is OneOf<T0, T1, T2> v)
            {
                serializer.Serialize(writer, v.Value);
            }
            else if (value is IEnumerable<OneOf<T0, T1, T2>> en)
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
                var oneOfList = new List<OneOf<T0, T1, T2>> (jArray.Count);
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

        private OneOf<T0, T1, T2>? DeserializeToOneOf(JToken jToken)
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
                oneOfArg = JsonConvert.DeserializeObject(jToken.ToString(), OneOfArgTypes[oneOfArgIndex]);
                return CreateOneOfInstance(oneOfArg, oneOfArgIndex);
            case OneOfArgToken.StringOrEnum:
                return DeserializeToOneOfWithStringOrEnumArg(jToken.ToString(), oneOfArgIndex);
            case OneOfArgToken.Integer:
            case OneOfArgToken.Float:
            case OneOfArgToken.Boolean:
            case OneOfArgToken.Date:
                oneOfArg = Convert.ChangeType(jToken, OneOfArgTypes[oneOfArgIndex]);
                return CreateOneOfInstance(oneOfArg, oneOfArgIndex);
            }

            return null;
        }

        private OneOf<T0, T1, T2> DeserializeToOneOfWithStringOrEnumArg(string oneOfArg, int oneOfArgIndex)
        {
            Debug.Assert((uint) oneOfArgIndex < OneOfArgTypes.Length);
            var oneOfArgType = OneOfArgTypes[oneOfArgIndex];

            if (oneOfArgType.IsEnum)
            {
                var enumValue = JsonConvert.DeserializeObject('"' + oneOfArg + '"', oneOfArgType);
                return CreateOneOfInstance(enumValue, oneOfArgIndex);
            }

            // The current OneOf<> generic argument (Tn) is of type string
            return CreateOneOfInstance(oneOfArg, oneOfArgIndex);
        }

        private OneOf<T0, T1, T2> CreateOneOfInstance(object oneOfArg, int oneOfArgIndex)
        {
            Debug.Assert((uint) oneOfArgIndex < OneOfArgTypes.Length);
            return
                (oneOfArgIndex == 0) ? (OneOf<T0, T1, T2>)(T0)oneOfArg :
                (oneOfArgIndex == 1) ? (OneOf<T0, T1, T2>)(T1)oneOfArg :
                (OneOf<T0, T1, T2>)(T2)oneOfArg;
        }

        public override bool CanConvert(Type objectType) =>
            OneOfJsonConverterCommon.CanConvertDirectly(objectType, OneOfType) ||
            OneOfJsonConverterCommon.CanConvertByEnumerating(objectType, OneOfType);
    }
}