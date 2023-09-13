using FluentAssertions;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using static OneOf.Serialization.Tests.ComplexEngineDetails;
using static OneOf.Serialization.Tests.Performance;

namespace OneOf.Serialization.Tests
{

    public class OneOfStructJsonConverterTests
    {
        //private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        //{
        //    Formatting = Formatting.Indented
        //};

        [Fact]
        public void Given_Instances_With_OneOf_Field_When_Serializing_Then_Produces_Valid_Json_String()
        {
            var engineDetails = new SimpleEngineDetails();
            var json = JsonConvert.SerializeObject(engineDetails);
            var engineDetailsJObject = JObject.FromObject(engineDetails);
            json.Should().NotBeNullOrEmpty();
            json.Should().Be("{'Fuel':0}".Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();

            engineDetails.Fuel = Fuel.Petrol;
            json = JsonConvert.SerializeObject(engineDetails);
            engineDetailsJObject = JObject.FromObject(engineDetails);
            json.Should().NotBeNullOrEmpty();
            json.Should().Be("{'Fuel':'Petrol'}".Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();

            engineDetails.Fuel = 2;
            json = JsonConvert.SerializeObject(engineDetails);
            engineDetailsJObject = JObject.FromObject(engineDetails);
            json.Should().NotBeNullOrEmpty();
            json.Should().Be("{'Fuel':2}".Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();
        }

        [Fact]
        public void Given_Enumerable_Instance_With_OneOf_Fields_When_Serializing_Then_Produces_Valid_Json_String()
        {
            var engineDetailsList = new List<SimpleEngineDetails> ()
            {
                new SimpleEngineDetails(),
                new SimpleEngineDetails() { Fuel = Fuel.Petrol },
                new SimpleEngineDetails() { Fuel = 2 },
                new SimpleEngineDetails() { Fuel = Fuel.Electricity },
            };

            var json = JsonConvert.SerializeObject(engineDetailsList.AsEnumerable());
            var engineDetailsJArray = JArray.FromObject(engineDetailsList.AsEnumerable());
            json.Should().NotBeNullOrEmpty();
            json.Should().Be("[{'Fuel':0},{'Fuel':'Petrol'},{'Fuel':2},{'Fuel':'Electricity'}]".Replace('\'', '"'));
            engineDetailsJArray.Should().NotBeNull();
        }

        [Fact]
        public void Given_Jsons_With_OneOf_Property_When_Deserializing_Then_Produces_Correct_OneOf_Field()
        {
            var json = "{}";
            var engineDetails = JsonConvert.DeserializeObject<SimpleEngineDetails>(json);
            engineDetails.Should().NotBeNull();
            engineDetails.Fuel.IsT0.Should().BeTrue();
            engineDetails.Fuel.Value.Should().Be(0);

            json = File.ReadAllText("./simple-engine-details-1.json");
            engineDetails = JsonConvert.DeserializeObject<SimpleEngineDetails>(json);
            engineDetails.Should().NotBeNull();
            engineDetails.Fuel.IsT1.Should().BeTrue();
            engineDetails.Fuel.Value.Should().Be(Fuel.Petrol);

            json = File.ReadAllText("./simple-engine-details-2.json");
            engineDetails = JsonConvert.DeserializeObject<SimpleEngineDetails>(json);
            engineDetails.Should().NotBeNull();
            engineDetails.Fuel.IsT0.Should().BeTrue();
            engineDetails.Fuel.Value.Should().Be(2);
        }

        [Fact]
        public void Given_Json_Array_With_OneOf_Properties_When_Deserializing_Then_Produces_Correct_Enumerable_Instance_With_OneOf_Fields()
        {
            var json = File.ReadAllText("./simple-engine-details-array.json");
            var engineDetailsEnumerable = JsonConvert.DeserializeObject<IEnumerable<SimpleEngineDetails>>(json);
            engineDetailsEnumerable.Should().NotBeNull();

            var engineDetailsList = engineDetailsEnumerable.ToList();

            var engineDetails0 = engineDetailsList[0];
            engineDetails0.Should().NotBeNull();
            engineDetails0.Should().BeOfType<SimpleEngineDetails>();
            engineDetails0.Fuel.IsT0.Should().BeTrue();
            engineDetails0.Fuel.Value.Should().Be(0);

            var engineDetails1 = engineDetailsList[1];
            engineDetails1.Should().NotBeNull();
            engineDetails1.Should().BeOfType<SimpleEngineDetails>();
            engineDetails1.Fuel.IsT1.Should().BeTrue();
            engineDetails1.Fuel.Value.Should().Be(Fuel.Petrol);

            var engineDetails2 = engineDetailsList[2];
            engineDetails2.Should().NotBeNull();
            engineDetails2.Should().BeOfType<SimpleEngineDetails>();
            engineDetails2.Fuel.IsT0.Should().BeTrue();
            engineDetails2.Fuel.Value.Should().Be(2);

            var engineDetails3 = engineDetailsList[3];
            engineDetails3.Should().NotBeNull();
            engineDetails3.Should().BeOfType<SimpleEngineDetails>();
            engineDetails3.Fuel.IsT1.Should().BeTrue();
            engineDetails3.Fuel.Value.Should().Be(Fuel.Electricity);
        }

        [Fact]
        public void Given_Instances_With_Complex_OneOf_Fields_When_Serializing_Then_Produces_Valid_Json_String()
        {
            var engineDetails = new ComplexEngineDetails();
            var json = JsonConvert.SerializeObject(engineDetails);
            var engineDetailsJObject = JObject.FromObject(engineDetails);
            json.Should().NotBeNullOrEmpty();
            json.Should().Be(
                "{'CodeName':null,'ReleaseDate':'0001-01-01T00:00:00','Type':0,'Fuel':null,'Performance':null,'Notes':null}"
                .Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();

            engineDetails.CodeName = "CE1";
            engineDetails.ReleaseDate = new DateTime(2021, 01, 20, 12, 00, 00, DateTimeKind.Utc);
            engineDetails.Type = EngineType.InternalCombustion;
            engineDetails.Fuel = new List<OneOf<int, Fuel>>() { Fuel.Petrol, Fuel.Gas };
            engineDetails.Performance = new Performance()
            {
                PistonCount = 4,
                PistonAngle = EPistonAngle._90_Degrees,
                HorsePower = 175,
                Torque = 2800,
                IsSporty = true,
            };
            engineDetails.Notes = "This engine has great fuel economy, but is somewhat sluggish";
            json = JsonConvert.SerializeObject(engineDetails);
            engineDetailsJObject = JObject.FromObject(engineDetails);
            json.Should().NotBeNullOrEmpty();
            json.Should().Be(
                ("{'CodeName':'CE1','ReleaseDate':'2021-01-20T12:00:00Z','Type':'InternalCombustion','Fuel':['Petrol','Gas']," +
                  "'Performance':{'PistonCount':4,'PistonAngle':'_90_Degrees','HorsePower':175,'Torque':2800,'IsSporty':true}," +
                  "'Notes':'This engine has great fuel economy, but is somewhat sluggish'}")
                .Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();

            engineDetails.CodeName = "CE2";
            engineDetails.ReleaseDate = new DateTime(2021, 06, 15, 11, 30, 00, DateTimeKind.Utc);
            engineDetails.Type = 1;
            engineDetails.Fuel = new List<OneOf<int, Fuel>>() { 1, Fuel.Gas };
            engineDetails.Performance = new Performance()
            {
                PistonCount = 4,
                PistonAngle = 90.0,
                HorsePower = 175,
                Torque = 2800,
                IsSporty = 0.5f,
            };
            engineDetails.Notes = new CNotes()
            {
                StrongPointsList = new List<OneOf<string>>() { "Has great fuel economy" },
                WeakPoints = "Is somewhat sluggish",
            };
            json = JsonConvert.SerializeObject(engineDetails);
            engineDetailsJObject = JObject.FromObject(engineDetails);
            json.Should().NotBeNullOrEmpty();
            json.Should().Be(
                ("{'CodeName':'CE2','ReleaseDate':'2021-06-15T11:30:00Z','Type':1,'Fuel':[1,'Gas']," +
                  "'Performance':{'PistonCount':4,'PistonAngle':90.0,'HorsePower':175,'Torque':2800,'IsSporty':0.5}," +
                  "'Notes':{'StrongPointsList':['Has great fuel economy'],'WeakPoints':'Is somewhat sluggish'}}")
                .Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();
        }

        [Fact]
        public void Given_Enumerable_Instance_With_Complex_OneOf_Fields_When_Serializing_Then_Produces_Valid_Json_String()
        {
            var engineDetailsList = new List<ComplexEngineDetails> ()
            {
                new ComplexEngineDetails(),
                new ComplexEngineDetails()
                {
                    CodeName = "CE1",
                    ReleaseDate = new DateTime(2021, 01, 20, 12, 00, 00, DateTimeKind.Utc),
                    Type = EngineType.InternalCombustion,
                    Fuel = new List<OneOf<int, Fuel>>() { Fuel.Petrol, Fuel.Gas },
                    Performance = new Performance()
                    {
                        PistonCount = 4,
                        PistonAngle = EPistonAngle._90_Degrees,
                        HorsePower = 175,
                        Torque = 2800,
                        IsSporty = true,
                    },
                    Notes = "This engine has great fuel economy, but is somewhat sluggish",
                },
                new ComplexEngineDetails()
                {
                    CodeName = "CE2b",
                    ReleaseDate = new DateTime(2022, 03, 07, 09, 10, 30, DateTimeKind.Utc),
                    Type = 1,
                    Fuel = new List<OneOf<int, Fuel>>() { 2 },
                    Performance = new Performance()
                    {
                        PistonCount = 4,
                        PistonAngle = 90.0,
                        HorsePower = 185,
                        Torque = 2900,
                        IsSporty = 0.6f,
                    },
                    Notes = new CNotes()
                    {
                        StrongPointsList = new List<OneOf<string>>() { "Has great fuel economy" },
                        WeakPoints = "Is only a bit sluggish",
                    },
                },
                new ComplexEngineDetails()
                {
                    CodeName = "CE3",
                    ReleaseDate = new DateTime(2023, 09, 11, 15, 20, 25, DateTimeKind.Utc),
                    Type = EngineType.Electric,
                    Fuel = new List<OneOf<int, Fuel>>() { 4 },
                    Performance = new Performance()
                    {
                        PistonCount = 0,
                        PistonAngle = -1,
                        HorsePower = 220,
                        Torque = 3200,
                        IsSporty = 0.75f,
                    },
                    Notes = "This engine has fast reaction times",
                },
            };

            var json = JsonConvert.SerializeObject(engineDetailsList.AsEnumerable());
            var engineDetailsJObject = JArray.FromObject(engineDetailsList.AsEnumerable());
            json.Should().NotBeNullOrEmpty();
            json.Should().Be(
                ("[" +
                   "{'CodeName':null,'ReleaseDate':'0001-01-01T00:00:00','Type':0,'Fuel':null,'Performance':null,'Notes':null}" + "," +
                   "{'CodeName':'CE1','ReleaseDate':'2021-01-20T12:00:00Z','Type':'InternalCombustion','Fuel':['Petrol','Gas']," +
                    "'Performance':{'PistonCount':4,'PistonAngle':'_90_Degrees','HorsePower':175,'Torque':2800,'IsSporty':true}," +
                    "'Notes':'This engine has great fuel economy, but is somewhat sluggish'}" + "," +
                   "{'CodeName':'CE2b','ReleaseDate':'2022-03-07T09:10:30Z','Type':1,'Fuel':[2]," +
                    "'Performance':{'PistonCount':4,'PistonAngle':90.0,'HorsePower':185,'Torque':2900,'IsSporty':0.6}," +
                    "'Notes':{'StrongPointsList':['Has great fuel economy'],'WeakPoints':'Is only a bit sluggish'}}" + "," +
                   "{'CodeName':'CE3','ReleaseDate':'2023-09-11T15:20:25Z','Type':'Electric','Fuel':[4]," +
                    "'Performance':{'PistonCount':0,'PistonAngle':-1,'HorsePower':220,'Torque':3200,'IsSporty':0.75}," +
                    "'Notes':'This engine has fast reaction times'}" +
                 "]")
                 .Replace('\'', '"'));
            engineDetailsJObject.Should().NotBeNull();
        }

        [Fact]
        public void Given_Jsons_With_Complex_OneOf_Properties_When_Deserializing_Then_Produces_Correct_OneOf_Fields()
        {
            var json = "{}";
            var engineDetails = JsonConvert.DeserializeObject<ComplexEngineDetails>(json);
            engineDetails.Should().NotBeNull();
            engineDetails.CodeName.Should().BeNull();
            engineDetails.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails.ReleaseDate.Value.Should().Be(default(DateTime));
            engineDetails.Type.IsT0.Should().BeTrue();
            engineDetails.Type.Value.Should().Be(0);
            engineDetails.Fuel.Should().BeNull();
            engineDetails.Performance.Should().BeNull();
            engineDetails.Notes.Should().NotBeNull();
            engineDetails.Notes.IsT0.Should().BeTrue();
            engineDetails.Notes.Value.Should().BeNull();

            json = File.ReadAllText("./complex-engine-details-1.json");
            engineDetails = JsonConvert.DeserializeObject<ComplexEngineDetails>(json);
            engineDetails.Should().NotBeNull();
            engineDetails.CodeName.Should().Be("CE1");
            engineDetails.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails.ReleaseDate.Value.Should().Be(new DateTime(2021, 01, 20, 12, 00, 00, DateTimeKind.Utc));
            engineDetails.Type.IsT1.Should().BeTrue();
            engineDetails.Type.Value.Should().Be(EngineType.InternalCombustion);
            engineDetails.Fuel.Should().NotBeNullOrEmpty().And.HaveCount(2);
            engineDetails.Fuel.First().IsT1.Should().BeTrue();
            engineDetails.Fuel.First().Value.Should().Be(Fuel.Petrol);
            engineDetails.Fuel.Last().IsT1.Should().BeTrue();
            engineDetails.Fuel.Last().Value.Should().Be(Fuel.Gas);
            engineDetails.Performance.Should().NotBeNull();
            engineDetails.Performance.PistonCount.Should().Be(4);
            engineDetails.Performance.PistonAngle.IsT1.Should().BeTrue();
            engineDetails.Performance.PistonAngle.Value.Should().Be(EPistonAngle._90_Degrees);
            engineDetails.Performance.HorsePower.Should().Be(175);
            engineDetails.Performance.Torque.Should().Be(2800);
            engineDetails.Performance.IsSporty.IsT0.Should().BeTrue();
            engineDetails.Performance.IsSporty.Value.Should().Be(true);
            engineDetails.Notes.Should().NotBeNull();
            engineDetails.Notes.IsT0.Should().BeTrue();
            engineDetails.Notes.Value.Should().Be("This engine has great fuel economy, but is somewhat sluggish");

            json = File.ReadAllText("./complex-engine-details-2.json");
            engineDetails = JsonConvert.DeserializeObject<ComplexEngineDetails>(json);
            engineDetails.Should().NotBeNull();
            engineDetails.CodeName.Should().Be("CE2");
            engineDetails.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails.ReleaseDate.Value.Should().Be(new DateTime(2021, 06, 15, 11, 30, 00, DateTimeKind.Utc));
            engineDetails.Type.IsT0.Should().BeTrue();
            engineDetails.Type.Value.Should().Be(1);
            engineDetails.Fuel.Should().NotBeNullOrEmpty().And.HaveCount(2);
            engineDetails.Fuel.First().IsT0.Should().BeTrue();
            engineDetails.Fuel.First().Value.Should().Be(1);
            engineDetails.Fuel.Last().IsT1.Should().BeTrue();
            engineDetails.Fuel.Last().Value.Should().Be(Fuel.Gas);
            engineDetails.Performance.PistonCount.Should().Be(4);
            engineDetails.Performance.PistonAngle.IsT0.Should().BeTrue();
            engineDetails.Performance.PistonAngle.Value.Should().Be(90.0);
            engineDetails.Performance.HorsePower.Should().Be(175);
            engineDetails.Performance.Torque.Should().Be(2800);
            engineDetails.Performance.IsSporty.IsT1.Should().BeTrue();
            engineDetails.Performance.IsSporty.Value.Should().Be(0.5f);
            engineDetails.Notes.Should().NotBeNull();
            engineDetails.Notes.IsT1.Should().BeTrue();
            engineDetails.Notes.AsT1.StrongPointsList.Should().NotBeNullOrEmpty().And.HaveCount(1);
            engineDetails.Notes.AsT1.StrongPointsList.First().IsT0.Should().BeTrue();
            engineDetails.Notes.AsT1.StrongPointsList.First().Value.Should().Be("Has great fuel economy");
            engineDetails.Notes.AsT1.WeakPoints.IsT0.Should().BeTrue();
            engineDetails.Notes.AsT1.WeakPoints.Value.Should().Be("Is somewhat sluggish");
        }

        [Fact]
        public void Given_Json_Array_With_Complex_OneOf_Properties_When_Deserializing_Then_Produces_Correct_Enumerable_Instance_With_OneOf_Fields()
        {
            var json = File.ReadAllText("./complex-engine-details-array.json");
            var engineDetailsEnumerable = JsonConvert.DeserializeObject<IEnumerable<ComplexEngineDetails>>(json);
            engineDetailsEnumerable.Should().NotBeNull();

            var engineDetailsList = engineDetailsEnumerable.ToList();

            var engineDetails0 = engineDetailsList[0];
            engineDetails0.Should().NotBeNull();
            engineDetails0.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails0.ReleaseDate.Value.Should().Be(default(DateTime));
            engineDetails0.CodeName.Should().BeNull();
            engineDetails0.Type.IsT0.Should().BeTrue();
            engineDetails0.Type.Value.Should().Be(0);
            engineDetails0.Fuel.Should().BeNull();
            engineDetails0.Performance.Should().BeNull();
            engineDetails0.Notes.Should().NotBeNull();
            engineDetails0.Notes.IsT0.Should().BeTrue();
            engineDetails0.Notes.Value.Should().BeNull();

            var engineDetails1 = engineDetailsList[1];
            engineDetails1.Should().NotBeNull();
            engineDetails1.CodeName.Should().Be("CE1");
            engineDetails1.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails1.ReleaseDate.Value.Should().Be(new DateTime(2021, 01, 20, 12, 00, 00, DateTimeKind.Utc));
            engineDetails1.Type.IsT1.Should().BeTrue();
            engineDetails1.Type.Value.Should().Be(EngineType.InternalCombustion);
            engineDetails1.Fuel.Should().NotBeNullOrEmpty().And.HaveCount(2);
            engineDetails1.Fuel.First().IsT1.Should().BeTrue();
            engineDetails1.Fuel.First().Value.Should().Be(Fuel.Petrol);
            engineDetails1.Fuel.Last().IsT1.Should().BeTrue();
            engineDetails1.Fuel.Last().Value.Should().Be(Fuel.Gas);
            engineDetails1.Performance.Should().NotBeNull();
            engineDetails1.Performance.PistonCount.Should().Be(4);
            engineDetails1.Performance.PistonAngle.IsT1.Should().BeTrue();
            engineDetails1.Performance.PistonAngle.Value.Should().Be(EPistonAngle._90_Degrees);
            engineDetails1.Performance.HorsePower.Should().Be(175);
            engineDetails1.Performance.Torque.Should().Be(2800);
            engineDetails1.Performance.IsSporty.IsT0.Should().BeTrue();
            engineDetails1.Performance.IsSporty.Value.Should().Be(true);
            engineDetails1.Notes.Should().NotBeNull();
            engineDetails1.Notes.IsT0.Should().BeTrue();
            engineDetails1.Notes.Value.Should().Be("This engine has great fuel economy, but is somewhat sluggish");

            var engineDetails2 = engineDetailsList[2];
            engineDetails2.Should().NotBeNull();
            engineDetails2.CodeName.Should().Be("CE2b");
            engineDetails2.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails2.ReleaseDate.Value.Should().Be(new DateTime(2022, 03, 07, 09, 10, 30, DateTimeKind.Utc));
            engineDetails2.Type.IsT0.Should().BeTrue();
            engineDetails2.Type.Value.Should().Be(1);
            engineDetails2.Fuel.Should().NotBeNullOrEmpty().And.HaveCount(1);
            engineDetails2.Fuel.First().IsT0.Should().BeTrue();
            engineDetails2.Fuel.First().Value.Should().Be(2);
            engineDetails2.Performance.PistonCount.Should().Be(4);
            engineDetails2.Performance.PistonAngle.IsT0.Should().BeTrue();
            engineDetails2.Performance.PistonAngle.Value.Should().Be(90.0);
            engineDetails2.Performance.HorsePower.Should().Be(185);
            engineDetails2.Performance.Torque.Should().Be(2900);
            engineDetails2.Performance.IsSporty.IsT1.Should().BeTrue();
            engineDetails2.Performance.IsSporty.Value.Should().Be(0.6f);
            engineDetails2.Notes.Should().NotBeNull();
            engineDetails2.Notes.IsT1.Should().BeTrue();
            engineDetails2.Notes.AsT1.StrongPointsList.Should().NotBeNullOrEmpty().And.HaveCount(1);
            engineDetails2.Notes.AsT1.StrongPointsList.First().IsT0.Should().BeTrue();
            engineDetails2.Notes.AsT1.StrongPointsList.First().Value.Should().Be("Has great fuel economy");
            engineDetails2.Notes.AsT1.WeakPoints.IsT0.Should().BeTrue();
            engineDetails2.Notes.AsT1.WeakPoints.Value.Should().Be("Is only a bit sluggish");

            var engineDetails3 = engineDetailsList[3];
            engineDetails3.Should().NotBeNull();
            engineDetails3.CodeName.Should().Be("CE3");
            engineDetails3.ReleaseDate.IsT0.Should().BeTrue();
            engineDetails3.ReleaseDate.Value.Should().Be(new DateTime(2023, 09, 11, 15, 20, 25, DateTimeKind.Utc));
            engineDetails3.Type.IsT1.Should().BeTrue();
            engineDetails3.Type.Value.Should().Be(EngineType.Electric);
            engineDetails3.Fuel.Should().NotBeNullOrEmpty().And.HaveCount(1);
            engineDetails3.Fuel.First().IsT0.Should().BeTrue();
            engineDetails3.Fuel.First().Value.Should().Be(4);
            engineDetails3.Performance.Should().NotBeNull();
            engineDetails3.Performance.PistonCount.Should().Be(0);
            engineDetails3.Performance.PistonAngle.IsT2.Should().BeTrue();
            engineDetails3.Performance.PistonAngle.Value.Should().Be(-1);
            engineDetails3.Performance.HorsePower.Should().Be(220);
            engineDetails3.Performance.Torque.Should().Be(3200);
            engineDetails3.Performance.IsSporty.IsT1.Should().BeTrue();
            engineDetails3.Performance.IsSporty.Value.Should().Be(0.75f);
            engineDetails3.Notes.Should().NotBeNull();
            engineDetails3.Notes.IsT0.Should().BeTrue();
            engineDetails3.Notes.Value.Should().Be("This engine has fast reaction times");
        }
    }
}
