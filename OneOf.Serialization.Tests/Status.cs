using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OneOf.Serialization
{
    [JsonConverter(typeof(OneOfJsonConverter<Status>))]
    class Status : OneOfBase<Status.Idle, Status.Started, Status.Stopped> {
        public class Stopped : OneOfCase {
            public IEnumerable<Status> PreviousStatuses { get; }

            public Stopped(IEnumerable<Status> previousStatuses)
            {
                PreviousStatuses = previousStatuses;
            }
        }
        public class Idle : OneOfCase
        {
        }

        public class Started : OneOfCase
        {
            public int Readiness { get; set; }

            public Started(int readiness) 
            {
                Readiness = readiness;
            }
        }
        public static implicit operator Status(Idle value) {
            return value == null ? value : new Status(value);
        }
        public Status(Idle value): base(0, value) {}
        
        public static implicit operator Status(Started value) {
            return value == null ? value : new Status(value);
        }
        public Status(Started value): base(1, null, value) {}

        public static implicit operator Status(Stopped value) {
            return value == null ? value : new Status(value);
        }
        public Status(Stopped value): base(2, null, null, value) {}
        
        [JsonConstructor]
        private Status() { }

        public TOut MatchSingle<TOut, TIn>(Func<TIn, TOut> oneOfCaseHandler, Func<TOut> restHandler) where TIn: class {
            if(this.Value is TIn) {
                return oneOfCaseHandler(this.Value as TIn);
            }

            return restHandler();
        }
        public void SwitchSingle<TIn>(Action<TIn> oneOfCaseHandler, Action<object> restHandler) where TIn: class {
            if(this.Value is TIn) {
                oneOfCaseHandler(this.Value as TIn);
            } else {
                restHandler(this.Value);
            }
        }
    }
}
