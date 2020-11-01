using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OneOf;
using OneOf.Serialization;
using static OneOf.Serialization.Status;

namespace OneOf.Serialization.Tests {
    class WorkingEngine : OneOfCase
    {
        public Status Status { get; set; }
        public Power Power { get; set; }

        public IEnumerable<string> HistoricalIssues { get; private set; }

        public static WorkingEngine From(BrokenEngine brokenEngine)
        {
            return new WorkingEngine()
            {
                HistoricalIssues = new List<string> { brokenEngine.Reason.Value },
                Status = new Idle(),
                Power = Power.Create(0)
            };
        }

        [JsonConstructor]
        private WorkingEngine() { }
        public WorkingEngine(Power power)
        {
            Power = power;
            Status = new Idle();
            HistoricalIssues = Enumerable.Empty<string>();
        }

        public void Test()
        {
            Status = new Stopped(new List<Status> {
                new Idle(),
                new Started(0),
                new Started(10),
                new Started(20),
                new Started(50),
                new Started(90),
                new Started(100),
            });
            Power = Power.Create(999);
        }
    }

    class Reason
    {
        [JsonConstructor]
        private Reason(string reason)
        {
            Value = reason;
        }

        public static Reason Create(string reason) => new Reason(reason);

        public string Value { get; }
    }
    class BrokenEngine : OneOfCase
    {
        public Reason Reason { get; }

        public IEnumerable<Status> PreviousStatuses { get; }

        [JsonConstructor]
        private BrokenEngine(Reason reason, IEnumerable<Status> previousStatuses = null)
        {
            Reason = reason;
            PreviousStatuses = previousStatuses ?? new List<Status>();
        }

        public static BrokenEngine From(WorkingEngine engine, Reason reason)
        {
            return engine.Status.MatchSingle(
                (Stopped stopped) => new BrokenEngine(reason, stopped.PreviousStatuses),
                () => new BrokenEngine(reason)
            );
        }
    }

    [JsonConverter(typeof(OneOfJsonConverter<Engine>))]
    class Engine : OneOfBase<WorkingEngine, BrokenEngine>
    {
        public Engine(WorkingEngine engine) : base(0, engine) { }
        public Engine(BrokenEngine engine) : base(1, null, engine) { }

        public static implicit operator Engine(WorkingEngine engine) => engine == null ? engine : new Engine(engine);
        public static implicit operator Engine(BrokenEngine engine) => engine == null ? engine : new Engine(engine);

        [JsonConstructor]
        private Engine() { }
    }
}