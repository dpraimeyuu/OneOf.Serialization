using Newtonsoft.Json;
using OneOf;
using OneOf.Serialization;

namespace OneOf.Serialization.Tests
{
    public class Job {
        public JobStatus Status { get; private set; }

        [JsonConstructor]
        private Job(JobStatus status) {
            Status = status;
        }
        public Job() => Status = new JobStatus.Waiting();
        public void Run() => Status = new JobStatus.Running();

        public void Done() => Status = new JobStatus.Completed();
    }

    [JsonConverter(typeof(OneOfJsonConverter<JobStatus>))]
    public sealed class JobStatus : OneOfBase<JobStatus.Waiting, JobStatus.Running, JobStatus.Completed>
    {
        public sealed class Waiting : OneOfCase { }
        public sealed class Running : OneOfCase { }
        public sealed class Completed : OneOfCase { }

        [JsonConstructor]
        private JobStatus() {}

        public static implicit operator JobStatus(Waiting value) => value == null ? null : new JobStatus(value);
        public JobStatus(Waiting value) : base(0, value, null, null) {}
        public static implicit operator JobStatus(Running value) => value == null ? null : new JobStatus(value);
        public JobStatus(Running value) : base(1, null, value, null) {}
        public static implicit operator JobStatus(Completed value) => value == null ? null : new JobStatus(value);
        public JobStatus(Completed value) : base(2, null, null, value) {}
    }
}
