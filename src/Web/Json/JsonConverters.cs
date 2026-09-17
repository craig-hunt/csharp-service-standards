using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Domain.Signups;
using ServiceStandards.Domain.Tasks;

namespace ServiceStandards.Web.Json;

/// <summary>
/// Writes a task identifier as the number a client expects.
/// </summary>
/// <remarks>
/// Without a converter the serializer would emit the wrapper as an object with
/// a Value member, so the typed identifier would leak its shape into the public
/// contract. The type stays distinct in C# and stays a plain number on the wire.
/// </remarks>
internal sealed class TaskIdJsonConverter : JsonConverter<TaskId>
{
    public override TaskId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TaskId.From(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, TaskId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);
}

/// <summary>Writes a signup identifier as a number.</summary>
internal sealed class SignupIdJsonConverter : JsonConverter<SignupId>
{
    public override SignupId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        SignupId.From(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, SignupId value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);
}

/// <summary>Writes a task title as a plain string.</summary>
internal sealed class TaskTitleJsonConverter : JsonConverter<TaskTitle>
{
    public override TaskTitle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TaskTitle.From(reader.GetString());

    public override void Write(Utf8JsonWriter writer, TaskTitle value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}

/// <summary>Writes a stock status as the words the table shows.</summary>
internal sealed class StatusJsonConverter : JsonConverter<Status>
{
    public override Status Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Status.From(reader.GetString());

    public override void Write(Utf8JsonWriter writer, Status value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
