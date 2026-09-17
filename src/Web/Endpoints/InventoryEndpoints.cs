using ServiceStandards.Application.Inventory;
using ServiceStandards.Domain.Inventory;
using ServiceStandards.Web.Contracts;

namespace ServiceStandards.Web.Endpoints;

/// <summary>
/// The stock route.
/// </summary>
internal static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder routes) =>
        routes.MapGet(WebConstants.PathInventory, ListAsync);

    private static async Task<IResult> ListAsync(
        HttpContext context,
        InventoryService inventory,
        CancellationToken cancellationToken)
    {
        var query = InventoryQuery.From(
            context.Request.Query[WebConstants.QuerySearch],
            context.Request.Query[WebConstants.QuerySort],
            context.Request.Query[WebConstants.QueryDirection]);

        var result = await inventory.QueryAsync(query, cancellationToken).ConfigureAwait(false);
        return Results.Ok(InventoryResponse.From(result));
    }
}
