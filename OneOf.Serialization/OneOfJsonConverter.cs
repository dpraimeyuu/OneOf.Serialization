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
        private static readonly List<Type> EmptyTypeList = new List<Type> (0);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IOneOf v)
            {
                serializer.Serialize(writer, v.Value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = JObject.Load(reader);
                var value = DeserializeToDiscriminatedUnion(obj, objectType);
                return CreateInstance(value);
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var array = JArray.Load(reader);
                var valueArray = DeserializeToDiscriminatedUnionArray(array, objectType);
                return CreateArrayInstance(valueArray);
            }

            return existingValue;
        }

        private object DeserializeToDiscriminatedUnion(JObject obj, Type objectType)
        {
            var oneOfCaseTypes = objectType?.BaseType?.GenericTypeArguments.ToList() ?? EmptyTypeList;
            foreach (var oneOfDiscriminatedUnion in oneOfCaseTypes)
            {
                string json = obj.ToString();
                var oneOfCase = JsonConvert.DeserializeObject<OneOfCase>(json);
                if (oneOfCase.Value == oneOfDiscriminatedUnion.Name)
                    return JsonConvert.DeserializeObject(json, oneOfDiscriminatedUnion);
            }

            return null;
        }

        private Array DeserializeToDiscriminatedUnionArray(JArray array, Type objectType)
        {
            var arrayArgTypes = objectType?.GenericTypeArguments.ToList() ?? EmptyTypeList;
            if (arrayArgTypes.Count != 1 || !arrayArgTypes[0].GetInterfaces().Contains(typeof(IOneOf)))
                return null;

            var oneOfArray = Array.CreateInstance(arrayArgTypes[0], array.Count);
            for (int i = 0, num = array.Count; i < num; ++i)
            {
                string json = array[i].ToString();
                oneOfArray.SetValue(JsonConvert.DeserializeObject(json, arrayArgTypes[0]), i);
            }
            return oneOfArray;
        }

        private T CreateInstance(params object[] paramArray)
        {
            return (T)Activator.CreateInstance(typeof(T), args: paramArray);
        }

        private T CreateArrayInstance(Array valueArray)
        {
            return (T)Activator.CreateInstance(typeof(T), new object[] { valueArray });
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IOneOf));
        }
    }
}