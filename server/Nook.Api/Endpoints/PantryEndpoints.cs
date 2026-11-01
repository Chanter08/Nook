using Nook.Api.DTOs.Pantry;
using Nook.Api.Services;

namespace Nook.Api.Endpoints;

public static class PantryEndpoints
{
    public static IEndpointRouteBuilder MapPantryEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pantry");

        group.MapGet("", GetPantry);
        group.MapPost("/items", AddItem);
        group.MapPut("/items/{itemId:int}", UpdateItem);
        group.MapDelete("/items/{itemId:int}", DeleteItem);
        group.MapPost("/receipt/scan", ScanReceipt);
        group.MapPost("/receipt/import", ImportReceipt);

        return app;
    }

    private static async Task<IResult> GetPantry(PantryService service)
    {
        return Results.Ok(await service.GetPantryAsync());
    }

    private static async Task<IResult> AddItem(
        UpsertPantryItemRequest request,
        PantryService service)
    {
        try
        {
            var item = await service.AddItemAsync(request);
            return Results.Ok(item);
        }
        catch (PantryValidationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }

    private static async Task<IResult> UpdateItem(
        int itemId,
        UpsertPantryItemRequest request,
        PantryService service)
    {
        try
        {
            var item = await service.UpdateItemAsync(itemId, request);

            return item is null
                ? Results.NotFound(new { message = "Pantry item not found." })
                : Results.Ok(item);
        }
        catch (PantryValidationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }

    private static async Task<IResult> DeleteItem(
        int itemId,
        PantryService service)
    {
        return await service.DeleteItemAsync(itemId)
            ? Results.NoContent()
            : Results.NotFound(new { message = "Pantry item not found." });
    }

    private static async Task<IResult> ScanReceipt(
        HttpRequest request,
        ReceiptScannerService scanner,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(
                    new { message = "Upload a receipt photo." });
            }

            var form = await request.ReadFormAsync(cancellationToken);
            var receipt = form.Files.GetFile("receipt");

            if (receipt is null)
            {
                return Results.BadRequest(
                    new { message = "Upload a receipt photo." });
            }

            return Results.Ok(
                await scanner.ScanAsync(receipt, cancellationToken));
        }
        catch (ReceiptScanValidationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
        catch (ReceiptScanNotConfiguredException exception)
        {
            return Results.Json(
                new { message = exception.Message },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (ReceiptScanProviderException exception)
        {
            return Results.Json(
                new { message = exception.Message },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> ImportReceipt(
        ImportReceiptRequest request,
        PantryService service)
    {
        try
        {
            return Results.Ok(await service.ImportReceiptAsync(request));
        }
        catch (PantryValidationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }
}
