using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OneOf.Serialization.Tests
{
    class SimpleEngineDetails
    {
        [JsonConverter(typeof(OneOfStructJsonConverter<int, Fuel>))]
        public OneOf<int, Fuel> Fuel { get; set; }
    }

    class ComplexEngineDetails
    {
        public string CodeName { get; set; }

        [JsonConverter(typeof(OneOfStructJsonConverter<DateTime>))]
        public OneOf<DateTime> ReleaseDate { get; set; }

        [JsonConverter(typeof(OneOfStructJsonConverter<short, EngineType>))]
        public OneOf<short, EngineType> Type { get; set; } 

        [JsonConverter(typeof(OneOfStructJsonConverter<int, Fuel>))]
        public IEnumerable<OneOf<int, Fuel>> Fuel { get; set; }

        public Performance Performance { get; set; }

        [JsonConverter(typeof(OneOfStructJsonConverter<string, CNotes>))]
        public OneOf<string, CNotes> Notes { get; set; }

        public class CNotes
        {
            [JsonConverter(typeof(OneOfStructJsonConverter<string>))]
            //public IEnumerable<OneOf<string>> StrongPointsList { get; set; }
            public List<OneOf<string>> StrongPointsList { get; set; }

            [JsonConverter(typeof(OneOfStructJsonConverter<string>))]
            public OneOf<string> WeakPoints { get; set; }
        }
    }
}