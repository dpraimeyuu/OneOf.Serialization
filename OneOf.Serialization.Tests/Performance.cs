using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace OneOf.Serialization.Tests
{
    public class Performance
    {
        public int PistonCount { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EPistonAngle
        {
            NotApplicable = -1,
            _90_Degrees = 0,
            _75_Degrees = 1,
            _60_Degrees = 2,
        }

        [JsonConverter(typeof(OneOfStructJsonConverter<double, EPistonAngle, sbyte>))]
        public OneOf<double, EPistonAngle, sbyte> PistonAngle { get; set; }

        public int HorsePower { get; set; }

        public int Torque { get; set; }

        [JsonConverter(typeof(OneOfStructJsonConverter<bool, float>))]
        public OneOf<bool, float> IsSporty { get; set; }
    }

}
