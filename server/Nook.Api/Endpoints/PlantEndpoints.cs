using System.Data;
using Microsoft.EntityFrameworkCore;
using Nook.Api.Data;
using Nook.Api.Services;

namespace Nook.Api.Endpoints;

public static class PlantEndpoints
{
    public static IEndpointRouteBuilder MapPlantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plants");

        group.MapGet("", GetPlants);
        group.MapGet("/{id:int}", GetPlant);
        group.MapPost("", CreatePlant);
        group.MapPut("/{id:int}", UpdatePlant);
        group.MapDelete("/{id:int}", DeletePlant);
        group.MapPost("/{id:int}/observations", AddObservation).DisableAntiforgery();
        group.MapPost("/{id:int}/care", AddCareEvent);
        group.MapPost("/analyze", AnalyzePhoto).DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> GetPlants(NookDbContext db)
    {
        var rows = new List<object>();
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.PlantId, p.Name, p.Species, p.Variety, p.Location, p.AcquiredDate,
                   p.Notes, p.CoverImageUrl, p.CreatedAtUtc,
                   (SELECT MAX(o.ObservedAtUtc) FROM PlantObservation o WHERE o.PlantId = p.PlantId) AS LastObservedAtUtc,
                   (SELECT MAX(c.OccurredAtUtc) FROM PlantCareEvent c WHERE c.PlantId = p.PlantId AND c.EventType = 'Watered') AS LastWateredAtUtc
            FROM Plant p
            ORDER BY p.Name;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(ReadPlantSummary(reader));
        }

        return Results.Ok(rows);
    }

    private static async Task<IResult> GetPlant(int id, NookDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();

        object? plant;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT PlantId, Name, Species, Variety, Location, AcquiredDate, Notes, CoverImageUrl, CreatedAtUtc
                FROM Plant WHERE PlantId = @id;
                """;
            AddParameter(command, "@id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return Results.NotFound();
            plant = new
            {
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                species = GetNullableString(reader, 2),
                variety = GetNullableString(reader, 3),
                location = GetNullableString(reader, 4),
                acquiredDate = GetNullableDateOnly(reader, 5),
                notes = GetNullableString(reader, 6),
                coverImageUrl = GetNullableString(reader, 7),
                createdAtUtc = reader.GetDateTime(8)
            };
        }

        var observations = new List<object>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT PlantObservationId, ObservedAtUtc, Notes, ImageUrl, IssueSummary
                FROM PlantObservation WHERE PlantId = @id
                ORDER BY ObservedAtUtc DESC, PlantObservationId DESC;
                """;
            AddParameter(command, "@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                observations.Add(new
                {
                    id = reader.GetInt32(0),
                    observedAtUtc = reader.GetDateTime(1),
                    notes = GetNullableString(reader, 2),
                    imageUrl = GetNullableString(reader, 3),
                    issueSummary = GetNullableString(reader, 4)
                });
            }
        }

        var careEvents = new List<object>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT PlantCareEventId, EventType, OccurredAtUtc, Notes
                FROM PlantCareEvent WHERE PlantId = @id
                ORDER BY OccurredAtUtc DESC, PlantCareEventId DESC;
                """;
            AddParameter(command, "@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                careEvents.Add(new
                {
                    id = reader.GetInt32(0),
                    eventType = reader.GetString(1),
                    occurredAtUtc = reader.GetDateTime(2),
                    notes = GetNullableString(reader, 3)
                });
            }
        }

        return Results.Ok(new { plant, observations, careEvents });
    }

    private static async Task<IResult> CreatePlant(PlantRequest request, NookDbContext db)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { message = "Plant name is required." });

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Plant (Name, Species, Variety, Location, AcquiredDate, Notes, CreatedAtUtc)
            OUTPUT INSERTED.PlantId
            VALUES (@name, @species, @variety, @location, @acquiredDate, @notes, SYSUTCDATETIME());
            """;
        AddParameter(command, "@name", request.Name.Trim());
        AddParameter(command, "@species", request.Species);
        AddParameter(command, "@variety", request.Variety);
        AddParameter(command, "@location", request.Location);
        AddParameter(command, "@acquiredDate", request.AcquiredDate);
        AddParameter(command, "@notes", request.Notes);
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return Results.Created($"/api/plants/{id}", new { id });
    }

    private static async Task<IResult> UpdatePlant(int id, PlantRequest request, NookDbContext db)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { message = "Plant name is required." });

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Plant SET Name=@name, Species=@species, Variety=@variety, Location=@location,
                AcquiredDate=@acquiredDate, Notes=@notes
            WHERE PlantId=@id;
            """;
        AddParameter(command, "@id", id);
        AddParameter(command, "@name", request.Name.Trim());
        AddParameter(command, "@species", request.Species);
        AddParameter(command, "@variety", request.Variety);
        AddParameter(command, "@location", request.Location);
        AddParameter(command, "@acquiredDate", request.AcquiredDate);
        AddParameter(command, "@notes", request.Notes);
        return await command.ExecuteNonQueryAsync() == 0 ? Results.NotFound() : Results.NoContent();
    }

    private static async Task<IResult> DeletePlant(int id, NookDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Plant WHERE PlantId=@id;";
        AddParameter(command, "@id", id);
        return await command.ExecuteNonQueryAsync() == 0 ? Results.NotFound() : Results.NoContent();
    }

    private static async Task<IResult> AddObservation(int id, HttpRequest request, IWebHostEnvironment environment, NookDbContext db)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new { message = "Observation must be submitted as form data." });

        var form = await request.ReadFormAsync();
        var notes = form["notes"].ToString().Trim();
        var issueSummary = form["issueSummary"].ToString().Trim();
        var observedAt = DateTime.UtcNow;
        if (DateTime.TryParse(form["observedAtUtc"], out var parsed)) observedAt = parsed.ToUniversalTime();

        string? imageUrl = null;
        var image = form.Files.GetFile("image");
        if (image is { Length: > 0 })
        {
            if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { message = "Only image files can be uploaded." });
            if (image.Length > 10 * 1024 * 1024)
                return Results.BadRequest(new { message = "Plant photos must be 10 MB or smaller." });

            var extension = Path.GetExtension(image.FileName);
            if (string.IsNullOrWhiteSpace(extension) || extension.Length > 8) extension = ".jpg";
            var directory = Path.Combine(environment.WebRootPath, "images", "plants");
            Directory.CreateDirectory(directory);
            var fileName = $"{id}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var destination = Path.Combine(directory, fileName);
            await using var stream = File.Create(destination);
            await image.CopyToAsync(stream);
            imageUrl = $"/images/plants/{fileName}";
        }

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO PlantObservation (PlantId, ObservedAtUtc, Notes, ImageUrl, IssueSummary)
            OUTPUT INSERTED.PlantObservationId
            SELECT @id, @observedAt, @notes, @imageUrl, @issueSummary
            WHERE EXISTS (SELECT 1 FROM Plant WHERE PlantId=@id);
            """;
        AddParameter(command, "@id", id);
        AddParameter(command, "@observedAt", observedAt);
        AddParameter(command, "@notes", string.IsNullOrWhiteSpace(notes) ? null : notes);
        AddParameter(command, "@imageUrl", imageUrl);
        AddParameter(command, "@issueSummary", string.IsNullOrWhiteSpace(issueSummary) ? null : issueSummary);

        var result = await command.ExecuteScalarAsync();
        if (result is null) return Results.NotFound();

        if (imageUrl is not null)
        {
            await using var coverCommand = connection.CreateCommand();
            coverCommand.CommandText = "UPDATE Plant SET CoverImageUrl=@imageUrl WHERE PlantId=@id;";
            AddParameter(coverCommand, "@id", id);
            AddParameter(coverCommand, "@imageUrl", imageUrl);
            await coverCommand.ExecuteNonQueryAsync();
        }

        return Results.Created($"/api/plants/{id}", new { id = Convert.ToInt32(result), imageUrl });
    }

    private static async Task<IResult> AnalyzePhoto(HttpRequest request, PlantAnalysisService service, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new { message = "Plant analysis requires an image upload." });

        var form = await request.ReadFormAsync(cancellationToken);
        var image = form.Files.GetFile("image");
        if (image is null)
            return Results.BadRequest(new { message = "Choose a plant photo first." });

        try
        {
            var result = await service.AnalyzeAsync(
                image,
                form["plantName"].ToString(),
                form["species"].ToString(),
                cancellationToken);
            return Results.Ok(result);
        }
        catch (PlantAnalysisException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> AddCareEvent(int id, CareEventRequest request, NookDbContext db)
    {
        var allowed = new[] { "Watered", "Fed", "Pruned", "Repotted", "Treated", "Moved", "Other" };
        if (!allowed.Contains(request.EventType, StringComparer.OrdinalIgnoreCase))
            return Results.BadRequest(new { message = "Unknown care event type." });

        var eventType = allowed.First(x => x.Equals(request.EventType, StringComparison.OrdinalIgnoreCase));
        var occurredAt = request.OccurredAtUtc ?? DateTime.UtcNow;
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO PlantCareEvent (PlantId, EventType, OccurredAtUtc, Notes)
            OUTPUT INSERTED.PlantCareEventId
            SELECT @id, @eventType, @occurredAt, @notes
            WHERE EXISTS (SELECT 1 FROM Plant WHERE PlantId=@id);
            """;
        AddParameter(command, "@id", id);
        AddParameter(command, "@eventType", eventType);
        AddParameter(command, "@occurredAt", occurredAt);
        AddParameter(command, "@notes", request.Notes);
        var result = await command.ExecuteScalarAsync();
        return result is null ? Results.NotFound() : Results.Created($"/api/plants/{id}", new { id = Convert.ToInt32(result) });
    }

    private static object ReadPlantSummary(IDataRecord reader) => new
    {
        id = reader.GetInt32(0),
        name = reader.GetString(1),
        species = GetNullableString(reader, 2),
        variety = GetNullableString(reader, 3),
        location = GetNullableString(reader, 4),
        acquiredDate = GetNullableDateOnly(reader, 5),
        notes = GetNullableString(reader, 6),
        coverImageUrl = GetNullableString(reader, 7),
        createdAtUtc = reader.GetDateTime(8),
        lastObservedAtUtc = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
        lastWateredAtUtc = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10)
    };

    private static string? GetNullableString(IDataRecord reader, int index) =>
        reader.IsDBNull(index) ? null : reader.GetString(index);

    private static DateOnly? GetNullableDateOnly(IDataRecord reader, int index) =>
        reader.IsDBNull(index) ? null : DateOnly.FromDateTime(reader.GetDateTime(index));

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    public sealed record PlantRequest(string Name, string? Species, string? Variety, string? Location, DateOnly? AcquiredDate, string? Notes);
    public sealed record CareEventRequest(string EventType, DateTime? OccurredAtUtc, string? Notes);
}
