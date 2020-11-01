using Newtonsoft.Json;

namespace OneOf.Serialization.Tests {

    [JsonConverter(typeof(OneOfJsonConverter<SomeStrangeType>))]
    public sealed class SomeStrangeType : OneOfBase<string, SomeStrangeType.LessStrangeCase> {
        public sealed class LessStrangeCase : OneOfCase {}

        public static implicit operator SomeStrangeType(string value) => value == null? null : new SomeStrangeType(value);
        public SomeStrangeType(string value) : base(0, value) {}
        
        public static implicit operator SomeStrangeType(LessStrangeCase value) => value == null? null : new SomeStrangeType(value);
        public SomeStrangeType(LessStrangeCase value) : base(1, null, value) {}

        [JsonConstructor]
        private SomeStrangeType() {}
    }
}