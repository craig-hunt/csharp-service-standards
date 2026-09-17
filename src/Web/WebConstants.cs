namespace ServiceStandards.Web;

/// <summary>
/// Every literal the HTTP surface carries, named once.
/// </summary>
internal static class WebConstants
{
    public const string ApiPrefix = "/api";
    public const string PathTasks = "/api/tasks";
    public const string PathTaskById = "/api/tasks/{id}";
    public const string PathSignups = "/api/signups";
    public const string PathInventory = "/api/inventory";
    public const string PathHealthLive = "/health";
    public const string PathHealthReady = "/health/ready";

    public const string RouteParamId = "id";
    public const string QueryFilter = "filter";
    public const string QuerySearch = "search";
    public const string QuerySort = "sort";
    public const string QueryDirection = "direction";

    public const string HeaderRequestId = "X-Request-ID";
    public const string HeaderWwwAuthenticate = "WWW-Authenticate";
    public const string HeaderAuthorization = "Authorization";
    public const int MaxRequestIdLength = 64;
    public const int MaxBodyBytes = 1048576;

    public const string LogKeyRequestId = "request_id";
    public const string LogKeyError = "error";
    public const string MsgRequestFailed = "request failed";
    public const string MsgNotReady = "readiness check failed";
    public const string MsgMissingConnection = "the service needs a connection string named Default";
    public const string MsgMissingToken = "the service needs an API token";
    public const string MsgTokenHasSeparator =
        "the API token must not contain a period, which would make it indistinguishable from a JSON Web Token";
    public const string MsgMissingSigningKey = "the service needs a JWT signing key";

    public const string ContentTypeProblem = "application/problem+json";
    public const string ProblemTypeBlank = "about:blank";
    public const string ExtensionCode = "code";
    public const string ExtensionFields = "fields";

    public const string TitleBadRequest = "Bad Request";
    public const string TitleUnauthorized = "Unauthorized";
    public const string TitleNotFound = "Not Found";
    public const string TitleUnprocessable = "Unprocessable Content";
    public const string TitleInternal = "Internal Server Error";

    public const string CodeUnauthorized = "unauthorized";
    public const string MsgUnauthorized = "a valid bearer token is required";

    public const string SchemeBearer = "Bearer";
    public const string BearerPrefix = "Bearer ";
    public const string SchemeStaticToken = "StaticToken";
    public const string SchemeSelector = "TokenSelector";

    public const string ConfigOutboxEnabled = "Outbox:Enabled";
    public const string ConfigApiToken = "Api:Token";
    public const string ConfigJwtSigningKey = "Jwt:SigningKey";
    public const string ConfigJwtIssuer = "Jwt:Issuer";
    public const string ConfigJwtAudience = "Jwt:Audience";

    public const char JwtSeparator = '.';
    public const int JwtSegmentCount = 3;
}
