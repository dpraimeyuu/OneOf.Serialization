using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
//using System.Runtime.Serialization;

namespace OneOf.Serialization.Tests
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Fuel
    {
        Unknown = 0,
        Petrol = 1,
        Diesel = 2,
        Gas = 3,
        Electricity = 4,
    }
}
