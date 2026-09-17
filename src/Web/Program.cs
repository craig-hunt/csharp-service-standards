using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Json;
using ServiceStandards.Application.Health;
using ServiceStandards.Application.Inventory;
using ServiceStandards.Application.Signups;
using ServiceStandards.Application.Tasks;
using ServiceStandards.Infrastructure;
using ServiceStandards.Web;
using ServiceStandards.Web.Auth;
using ServiceStandards.Web.Endpoints;
using ServiceStandards.Web.Errors;
using ServiceStandards.Web.Json;
using ServiceStandards.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Structured JSON on stdout: a container platform collects the stream and an
// operator queries fields rather than grepping sentences.
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(new JsonFormatter()));

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = WebConstants.MaxBodyBytes);

var connectionString = builder.Configuration.GetConnectionString(InfrastructureConstants.ConnectionName)
    ?? throw new InvalidOperationException(WebConstants.MsgMissingConnection);

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<SignupService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<HealthService>();

// The relay publishes what the outbox holds. A test host turns it off, because
// it fakes the stores and so never writes a message for the relay to find.
if (builder.Configuration.GetValue(WebConstants.ConfigOutboxEnabled, true))
{
    builder.Services.AddOutboxRelay();
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.UnmappedMemberHandling = JsonBody.Options.UnmappedMemberHandling;
    foreach (var converter in JsonBody.Options.Converters)
    {
        options.SerializerOptions.Converters.Add(converter);
    }
});

// An empty value is not a configured value. Reading these with ?? would let an
// appsettings placeholder through, and an empty token makes the expected header
// exactly the scheme, which any caller can send. Missing credentials stop the
// service rather than quietly disabling the check that guards every route.
var apiToken = Required(builder.Configuration[WebConstants.ConfigApiToken], WebConstants.MsgMissingToken);
var signingKey = Required(
    builder.Configuration[WebConstants.ConfigJwtSigningKey],
    WebConstants.MsgMissingSigningKey);

static string Required(string? value, string message) =>
    string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException(message) : value;

// Two schemes answer one header. A JSON Web Token carries three dot-separated
// segments; anything else is the static token a script or a probe presents.
builder.Services
    .AddAuthentication(WebConstants.SchemeSelector)
    .AddPolicyScheme(WebConstants.SchemeSelector, WebConstants.SchemeSelector, options =>
        options.ForwardDefaultSelector = context =>
            context.Request.Headers[WebConstants.HeaderAuthorization].ToString()
                .Count(character => character == WebConstants.JwtSeparator) == WebConstants.JwtSegmentCount - 1
                ? JwtBearerDefaults.AuthenticationScheme
                : WebConstants.SchemeStaticToken)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration[WebConstants.ConfigJwtIssuer],
            ValidAudience = builder.Configuration[WebConstants.ConfigJwtAudience],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async challenge =>
            {
                challenge.HandleResponse();
                challenge.Response.Headers[WebConstants.HeaderWwwAuthenticate] = WebConstants.SchemeBearer;
                await ProblemWriter.WriteAsync(
                        challenge.HttpContext,
                        StatusCodes.Status401Unauthorized,
                        WebConstants.TitleUnauthorized,
                        WebConstants.CodeUnauthorized,
                        WebConstants.MsgUnauthorized)
                    .ConfigureAwait(false);
            },
        };
    })
    .AddScheme<StaticTokenOptions, StaticTokenAuthenticationHandler>(
        WebConstants.SchemeStaticToken,
        options => options.Token = apiToken);

builder.Services.AddAuthorization();

var app = builder.Build();

// Order matters: the identifier exists before anything logs, the error boundary
// sits inside it so a failure still carries the identifier, and authentication
// runs after both so a rejected request logs like every other one.
app.UseMiddleware<RequestIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();

var api = app.MapGroup(string.Empty).RequireAuthorization();
api.MapTaskEndpoints();
api.MapSignupEndpoints();
api.MapInventoryEndpoints();

await app.RunAsync().ConfigureAwait(false);

/// <summary>
/// Named so the integration tests can drive this application through a test
/// host rather than a second copy of the composition root.
/// </summary>
/// <remarks>
/// This is the one type in the application that stays public. A test host takes
/// the entry point as a generic argument, and InternalsVisibleTo grants access
/// without granting accessibility, so a public fixture cannot close over an
/// internal type however visible it has been made.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "The test host names this type as a generic argument, which requires it to be public.")]
public partial class Program
{
}
