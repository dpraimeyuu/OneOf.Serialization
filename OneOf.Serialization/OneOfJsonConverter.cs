using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneOf.Serialization
{
    public class OneOfCase
    {

        public string Value { get; private set; }
        public OneOfCase()
        {
            Value = this.GetType().Name;
        }

        [JsonConstructor]
        private OneOfCase(string value)
        {
            Value = value;
        }
    }

    public class OneOfJsonConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IOneOf v)
            {
                serializer.Serialize(writer, v.Value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return existingValue;
            var obj = JObject.Load(reader);
            var instance = DeserializeToDiscriminatedUnion(obj, objectType);

            return CreateInstance(instance);
        }

        private object DeserializeToDiscriminatedUnion(JObject obj, Type objectType)
        {
            var oneOfCaseTypes = objectType?.BaseType?.GenericTypeArguments.ToList() ?? new List<Type>();
            object result = null;
            foreach (var oneOfDiscriminatedUnion in oneOfCaseTypes)
            {
                string json = obj.ToString();
                var oneOfCase = JsonConvert.DeserializeObject<OneOfCase>(json);
                if (oneOfCase.Value == oneOfDiscriminatedUnion.Name)
                {
                    var subType = JsonConvert.DeserializeObject(json, oneOfDiscriminatedUnion);
                    return subType;
                }
            }

            return result;
        }

        private T CreateInstance(params object[] paramArray)
        {
            return (T)Activator.CreateInstance(typeof(T), args: paramArray);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOneOf));
        }
    }
}