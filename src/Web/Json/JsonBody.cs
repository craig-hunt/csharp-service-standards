using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceStandards.Web.Errors;

namespace ServiceStandards.Web.Json;

/// <summary>
/// Reads request bodies under one strict policy.
/// </summary>
/// <remarks>
/// Model binding accepts an unknown member, a JSON null, and trailing content
/// after the object, each of which hides a client mistake behind a successful
/// request. Reading explicitly rejects all three and answers with the invalid
/// body envelope, matching the sibling Go project's strict decoder.
/// </remarks>
internal static class JsonBody
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new TaskIdJsonConverter(),
            new SignupIdJsonConverter(),
            new TaskTitleJsonConverter(),
            new StatusJsonConverter(),
        },
    };

    public static async Task<T> ReadAsync<T>(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var value = await JsonSerializer
                .DeserializeAsync<T>(request.Body, Options, cancellationToken)
                .ConfigureAwait(false);

            return value ?? throw new InvalidRequestBodyException();
        }
        catch (JsonException)
        {
            throw new InvalidRequestBodyException();
        }
    }
}
