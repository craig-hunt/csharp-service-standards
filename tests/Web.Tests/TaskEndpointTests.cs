using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ServiceStandards.Domain.Errors;
using ServiceStandards.Domain.Tasks;
using Xunit;

namespace ServiceStandards.Web.Tests;

/// <summary>
/// The task routes as a client meets them.
/// </summary>
/// <remarks>
/// These assert status codes and body members rather than C# objects, because
/// the contract is what goes over the wire. A refactor that keeps the types and
/// changes a member name is exactly the break these should catch.
/// </remarks>
public sealed class TaskEndpointTests : IClassFixture<ApiFactory>
{
    private const string FirstTitle = "Draft the proposal";
    private const string SecondTitle = "Send the invoice";
    private const string NewTitle = "Book the venue";
    private const long FirstId = 1;
    private const long SecondId = 2;
    private const int SeededTasks = 2;
    private const int SeededRemaining = 1;
    private const long NoIdentifier = 0;
    private const string TasksMember = "tasks";
    private const string RemainingMember = "remaining";
    private const string TotalMember = "total";
    private const string RemovedMember = "removed";
    private const string TitleMember = "title";
    private const string CompletedMember = "completed";
    private const string IdMember = "id";
    private const string FieldsMember = "fields";
    private const string CodeMember = "code";
    private const string BadFilterQuery = "/api/tasks?filter=bogus";
    private const string UnknownMemberBody = """{"title":"ok","nope":true}""";
    private const string EmptyTitleBody = """{"title":"   "}""";
    private const string EmptyObjectBody = "{}";
    private const string JsonContentType = "application/json";
    private const string FirstTaskPath = "/api/tasks/1";
    private const string SuppliedRequestId = "trace-7f3a";
    private const long ClearedRows = 3;

    private readonly ApiFactory _factory;

    public TaskEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.Tasks.Reset();
        _factory.Tasks.Seed(
            new TaskItem(TaskId.From(FirstId), TaskTitle.From(FirstTitle), false),
            new TaskItem(TaskId.From(SecondId), TaskTitle.From(SecondTitle), true));
    }

    [Fact]
    public async Task ListRejectsARequestWithoutAToken()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri(WebConstants.PathTasks, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(WebConstants.SchemeBearer, response.Headers.WwwAuthenticate.ToString());

        var problem = await ReadJsonAsync(response);
        Assert.Equal(WebConstants.CodeUnauthorized, problem.GetProperty(CodeMember).GetString());
    }

    [Fact]
    public async Task ListAnswersWithTasksAndCounts()
    {
        using var client = Authorized();

        using var response = await client.GetAsync(
            new Uri(WebConstants.PathTasks, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(SeededTasks, body.GetProperty(TasksMember).GetArrayLength());
        Assert.Equal(SeededRemaining, body.GetProperty(RemainingMember).GetInt32());
        Assert.Equal(SeededTasks, body.GetProperty(TotalMember).GetInt32());
    }

    [Fact]
    public async Task ListRejectsAnUnknownFilter()
    {
        using var client = Authorized();

        using var response = await client.GetAsync(
            new Uri(BadFilterQuery, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await ReadJsonAsync(response);
        Assert.Equal(TaskConstants.CodeInvalidFilter, problem.GetProperty(CodeMember).GetString());
    }

    [Fact]
    public async Task CreateAnswersWithTheCreatedTask()
    {
        using var client = Authorized();

        using var response = await client.PostAsJsonAsync(
            new Uri(WebConstants.PathTasks, UriKind.Relative),
            new { title = NewTitle },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(NewTitle, body.GetProperty(TitleMember).GetString());
        Assert.False(body.GetProperty(CompletedMember).GetBoolean());
        Assert.True(body.GetProperty(IdMember).GetInt64() > NoIdentifier);
    }

    [Fact]
    public async Task CreateRejectsAnEmptyTitleWithAFieldProblem()
    {
        using var client = Authorized();

        using var response = await PostRawAsync(client, WebConstants.PathTasks, EmptyTitleBody);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var problem = await ReadJsonAsync(response);
        Assert.Equal(ErrorConstants.CodeValidation, problem.GetProperty(CodeMember).GetString());
        Assert.Equal(
            TaskConstants.MsgTitleRequired,
            problem.GetProperty(FieldsMember).GetProperty(TitleMember).GetString());
    }

    [Fact]
    public async Task CreateRejectsAnUnknownMember()
    {
        using var client = Authorized();

        using var response = await PostRawAsync(client, WebConstants.PathTasks, UnknownMemberBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await ReadJsonAsync(response);
        Assert.Equal(ErrorConstants.CodeInvalidBody, problem.GetProperty(CodeMember).GetString());
    }

    [Fact]
    public async Task UpdateRequiresTheCompletionFlag()
    {
        using var client = Authorized();

        using var content = new StringContent(EmptyObjectBody, Encoding.UTF8, JsonContentType);
        using var response = await client.PatchAsync(
            new Uri(FirstTaskPath, UriKind.Relative),
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var problem = await ReadJsonAsync(response);
        Assert.Equal(
            TaskConstants.MsgCompletedRequired,
            problem.GetProperty(FieldsMember).GetProperty(CompletedMember).GetString());
    }

    [Fact]
    public async Task ClearCompletedReportsWhatItRemoved()
    {
        using var client = Authorized();
        _factory.Tasks.ClearResult = ClearedRows;

        using var response = await client.DeleteAsync(
            new Uri(WebConstants.PathTasks, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(ClearedRows, body.GetProperty(RemovedMember).GetInt64());
    }

    [Fact]
    public async Task EveryResponseCarriesACorrelationHeader()
    {
        using var client = Authorized();

        using var response = await client.GetAsync(
            new Uri(WebConstants.PathTasks, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.True(response.Headers.Contains(WebConstants.HeaderRequestId));
    }

    [Fact]
    public async Task ASuppliedCorrelationHeaderSurvives()
    {
        using var client = Authorized();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(WebConstants.PathTasks, UriKind.Relative));
        request.Headers.Add(WebConstants.HeaderRequestId, SuppliedRequestId);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(
            SuppliedRequestId,
            response.Headers.GetValues(WebConstants.HeaderRequestId).Single());
    }

    [Fact]
    public async Task AFailureAnswersWithProblemDetails()
    {
        using var client = Authorized();

        using var response = await client.GetAsync(
            new Uri(BadFilterQuery, UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Equal(WebConstants.ContentTypeProblem, response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<HttpResponseMessage> PostRawAsync(HttpClient client, string path, string body)
    {
        using var content = new StringContent(body, Encoding.UTF8, JsonContentType);
        return await client
            .PostAsync(new Uri(path, UriKind.Relative), content, TestContext.Current.CancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var payload = await response.Content
            .ReadAsStringAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(false);

        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }

    private HttpClient Authorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(WebConstants.SchemeBearer, ApiFactory.Token);
        return client;
    }
}
