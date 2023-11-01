# Why?

Sooner or later we would need to serialize/deserialize our lovely discriminated unions and [OneOf](https://github.com/mcintyre321/OneOf) doesn't come with built-in serialization/deserialization. `OneOf.Serialization` was created mainly to solve this challange by giving convention-based serialization/deserialization when using the powerful tool [OneOf](https://github.com/mcintyre321/OneOf).

**NOTE**: `OneOf.Serialization` has direct dependecy on `Newtonsoft.Json`.

# How to use it?
1. Take your discriminated union and annotate it with `JsonConvert` attribute, giving `OneOfJsonConverter<T>` as an argument (where `T` is your discriminated union)

**NOTE**: ⚠️ The way we prepare our `OneOf` class is dependent on the original `OneOf` package version ⚠️ 

* `OneOf` version `2.x.x`  (without `GenerateOneOf` support)
```csharp
[JsonConverter(typeof(OneOfJsonConverter<Status>))]
public class Status : OneOfBase<Idle, Running, Completed> {
    
    public Status(Idle idle) : base(0, idle) {}
    public Status(Running running) : base(1, null, running) {}
    public Status(Completed completed) : base(2, null, null, completed) {}

    public static implicit operator Status(Idle value) => value == null? null : new Status(value);
    public static implicit operator Status(Running value) => value == null? null : new Status(value);
    public static implicit operator Status(Completed value) => value == null? null : new Status(value);
}
```
* `OneOf` version `3.x.x` (supporting `GenerateOneOf` based on Source Generators)
```csharp
[JsonConverter(typeof(OneOfJsonConverter<Status>))]
[GenerateOneOf]
public partial class Status : OneOfBase<Idle, Running, Completed> {
    public Status(Idle idle) : base(idle) {}
    public Status(Running running) : base(running) {}
    public Status(Completed completed) : base(completed) {}
}
```

2. Take all cases of your discriminated union and inherit `OneOfCase` marker class
```csharp
public class Idle : OneOfCase {}

public class Running : OneOfCase 
{
    public DateTime StartedAt { get; }

    public Running(DateTime startedAt) {
        StartedAt = startedAt;
    }
}

public class Completed : OneOfCase 
{
    public DateTime CompletedAt { get; }

    public Running(DateTime completedAt) {
        CompletedAt = completedAt;
    }
}
```
3. Serialize!
```csharp
    Status status = new Completed(DateTime.Now);
    Console.WriteLine(JsonConvert.SerializeObject(status));
    // output:
    // {\r\n    \"CompletedAt\": \"2020-11-17T21:29:52.0664389+01:00\",\r\n    \"Value\": \"Completed\"\r\n  }
```

