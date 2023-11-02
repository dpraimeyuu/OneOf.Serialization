using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OneOf.Serialization.Tests
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EngineType
    {
        InternalCombustion = 1,
        Electric = 2,
    }
}
