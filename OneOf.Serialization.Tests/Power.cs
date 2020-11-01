using Newtonsoft.Json;

namespace OneOf.Serialization.Tests
{
    public class Power {
        [JsonConstructor]
        private Power(int value)
        {
            Value = value;
        }

        public static Power Create(int power) {
            return new Power(power);
        }

        public int Value { get; }
    }
}
