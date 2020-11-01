using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OneOf.Serialization.Tests
{
    
    public class OneOfJsonConverterTests
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        [Fact]
        public void Given_Instance_Of_OneOf_Case_When_Serializing_Then_Produces_Valid_Json_String()
        {
            Engine engine = new WorkingEngine(Power.Create(999));
            var json = JsonConvert.SerializeObject(engine, settings);
            var engineJObject = JObject.FromObject(engine);
            json.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Given_Instance_Of_OneOf_Case_When_Serializing_Then_Produced_JObject_Contains_Correct_Discriminator_Values()
        {
            Engine engine = new WorkingEngine(Power.Create(999));
            var engineJObject = JObject.FromObject(engine);
            var engineJObjectDiscriminator = engineJObject.Value<string>("Value");
            var engineJObjectStatusDiscriminator = engineJObject["Status"].Value<string>("Value");

            engineJObject.Should().NotBeNull();
            engineJObjectDiscriminator.Should().NotBeNullOrEmpty();
            engineJObjectDiscriminator.Should().Equals(JToken.FromObject(nameof(WorkingEngine)));

            engineJObjectStatusDiscriminator.Should().NotBeNullOrEmpty();
            engineJObjectStatusDiscriminator.Should().Equals(JToken.FromObject(nameof(Status.Idle)));
        }

        [Fact]
        public void Given_Json_With_OneOf_Case_When_Deserializing_Then_Produces_Correct_Case_Instance()
        {
            var json = File.ReadAllText("./working-engine.json");
            var engine = JsonConvert.DeserializeObject<Engine>(json);
            var engineStatus = (engine.Value as WorkingEngine).Status;
            engine.Should().NotBeNull();
            engine.Value.Should().BeOfType<WorkingEngine>();
            engineStatus.Should().NotBeNull();
            engineStatus.Value.Should().BeOfType<Status.Idle>();
        }

        [Fact]
        public void Given_Data_Type_With_Fieldless_Unions_When_Serializing_And_Deserializing_Then_Produces_Correct_Fieldless_Union_Instances() {
            var job = new Job();
            job.Status.Value.Should().BeOfType<JobStatus.Waiting>();

            var json = JsonConvert.SerializeObject(job, settings);
            var deserializedJob = JsonConvert.DeserializeObject<Job>(json, settings);
            deserializedJob.Status.Value.Should().BeOfType<JobStatus.Waiting>();
            deserializedJob.Run();
            deserializedJob.Status.Value.Should().BeOfType<JobStatus.Running>();

            var runningJobJson = JsonConvert.SerializeObject(deserializedJob, settings);
            var deserializedRunningJob = JsonConvert.DeserializeObject<Job>(runningJobJson, settings);
            deserializedRunningJob.Status.Value.Should().BeOfType<JobStatus.Running>();
            deserializedRunningJob.Done();
            deserializedRunningJob.Status.Value.Should().BeOfType<JobStatus.Completed>();
            
            var completedJobJson = JsonConvert.SerializeObject(deserializedRunningJob, settings);
            var deserializedCompletedJobJson = JsonConvert.DeserializeObject<Job>(completedJobJson, settings);
            deserializedCompletedJobJson.Status.Value.Should().BeOfType<JobStatus.Completed>();
        }

        [Fact]
        public void Given_Data_Type_With_Primitive_Case_As_Type_When_Serializing_And_Deserializing_Then_Incorrectly_Produces_Null() {
            SomeStrangeType myType = "some strange string";
            var json = JsonConvert.SerializeObject(myType, settings);
            json.Should().NotBeNullOrEmpty();
            var deserializedMyType = JsonConvert.DeserializeObject<SomeStrangeType>(json, settings);
            deserializedMyType.Should().BeNull();
        }
    }
}
