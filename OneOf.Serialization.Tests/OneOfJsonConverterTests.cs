using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OneOf.Serialization.Tests
{
    
    public class OneOfJsonConverterTests
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
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
            json.Should().StartWith("{").And.EndWith("}");
            engineJObject.Should().NotBeNull();
        }

        [Fact]
        public void Given_Enumerable_Instance_Of_OneOf_Cases_When_Serializing_Then_Produces_Valid_Json_String()
        {
            var engines = new List<Engine> ()
            {
                new WorkingEngine(Power.Create(999)),
                new WorkingEngine(Power.Create(888)),
            };
            engines[1].AsT0.Status = new Status.Started(20);
            var json = JsonConvert.SerializeObject(engines.AsEnumerable(), settings);
            var enginesJArray = JArray.FromObject(engines.AsEnumerable());
            json.Should().NotBeNullOrEmpty();
            json.Should().StartWith("[").And.EndWith("]");
            enginesJArray.Should().NotBeNull();
        }

        [Fact]
        public void Given_Instance_With_Enumerable_Field_Of_OneOf_Cases_When_Serializing_Then_Produces_Valid_Json_String()
        {
            var engines = new Engines()
            {
                EngineList = new List<Engine> ()
                {
                    new WorkingEngine(Power.Create(999)),
                    new WorkingEngine(Power.Create(888)),
                }
            };
            engines.EngineList[1].AsT0.Status = new Status.Started(20);
            var json = JsonConvert.SerializeObject(engines, settings);
            var enginesJObject = JObject.FromObject(engines);
            json.Should().NotBeNullOrEmpty();
            json.Should().StartWith("{").And.EndWith("}");
            enginesJObject.Should().NotBeNull();
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
            engine.Should().NotBeNull();
            engine.Value.Should().BeOfType<WorkingEngine>();
            var engineStatus = (engine.Value as WorkingEngine).Status;
            engineStatus.Should().NotBeNull();
            engineStatus.Value.Should().BeOfType<Status.Idle>();
        }

        [Fact]
        public void Given_Json_Array_Of_OneOf_Cases_When_Deserializing_Then_Produces_Correct_Enumerable_Instance_Of_Cases()
        {
            var json = File.ReadAllText("./working-engine-array.json");
            var engineEnumerable = JsonConvert.DeserializeObject<IEnumerable<Engine>>(json);
            engineEnumerable.Should().NotBeNull();

            var engineList = engineEnumerable.ToList();

            var engine0 = engineList[0];
            engine0.Should().NotBeNull();
            engine0.Value.Should().BeOfType<WorkingEngine>();
            var engine0Status = (engine0.Value as WorkingEngine).Status;
            engine0Status.Should().NotBeNull();
            engine0Status.Value.Should().BeOfType<Status.Idle>();

            var engine1 = engineList[1];
            engine1.Should().NotBeNull();
            engine1.Value.Should().BeOfType<WorkingEngine>();
            var engine1Status = (engine1.Value as WorkingEngine).Status;
            engine1Status.Should().NotBeNull();
            engine1Status.Value.Should().BeOfType<Status.Started>();
            engine1Status.AsT1.Readiness.Equals(20);
        }

        [Fact]
        public void Given_Json_Object_With_Array_Of_OneOf_Cases_When_Deserializing_Then_Produces_Correct_Instance_With_Enumerable_Field_Of_Cases()
        {
            var json = File.ReadAllText("./working-engines.json");
            var engines = JsonConvert.DeserializeObject<Engines>(json);
            engines.Should().NotBeNull();

            var engine0 = engines.EngineList[0];
            engine0.Should().NotBeNull();
            engine0.Value.Should().BeOfType<WorkingEngine>();
            var engine0Status = (engine0.Value as WorkingEngine).Status;
            engine0Status.Should().NotBeNull();
            engine0Status.Value.Should().BeOfType<Status.Idle>();

            var engine1 = engines.EngineList[1];
            engine1.Should().NotBeNull();
            engine1.Value.Should().BeOfType<WorkingEngine>();
            var engine1Status = (engine1.Value as WorkingEngine).Status;
            engine1Status.Should().NotBeNull();
            engine1Status.Value.Should().BeOfType<Status.Started>();
            engine1Status.AsT1.Readiness.Equals(20);
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
