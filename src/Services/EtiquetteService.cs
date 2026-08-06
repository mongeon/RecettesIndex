using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;
using static Supabase.Postgrest.Constants;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing recipe tags with caching and error handling.
/// </summary>
public class EtiquetteService(
    ICacheService cache,
    Supabase.Client supabaseClient,
    ILogger<EtiquetteService> logger) : CrudServiceBase<Etiquette, EtiquetteService>(cache, supabaseClient, logger), IEtiquetteService
{
    public async Task<IReadOnlyList<Etiquette>> GetAllAsync(CancellationToken ct = default)
        => await GetAllCachedAsync(
            CacheConstants.EtiquettesListKey,
            async token =>
            {
                var response = await _supabaseClient.From<Etiquette>()
                    .Order("name", Ordering.Ascending)
                    .Get(cancellationToken: token);
                return (IReadOnlyList<Etiquette>)(response.Models ?? []);
            },
            ct);

    public Task<Result<Etiquette>> GetOrCreateAsync(string name, CancellationToken ct = default)
        => CreateCoreAsync(
            new Etiquette { Name = name?.Trim() ?? string.Empty },
            // Not ValidationGuards.RequireNonEmpty: its message is English, and this one
            // surfaces directly in the French tag picker.
            () => string.IsNullOrWhiteSpace(name) ? "Le nom de l'étiquette est requis" : null,
            async token =>
            {
                var trimmed = name!.Trim();

                // The database enforces uniqueness on lower(btrim(name)), so match the same
                // way here rather than letting the insert fail on the unique index.
                var existing = await _supabaseClient.From<Etiquette>().Get(cancellationToken: token);
                var match = existing.Models?.FirstOrDefault(
                    e => string.Equals(e.Name.Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match;
                }

                var response = await _supabaseClient.From<Etiquette>()
                    .Insert(new Etiquette { Name = trimmed, CreationDate = DateTime.UtcNow }, cancellationToken: token);
                return response.Models?.FirstOrDefault();
            },
            onSuccess: created =>
            {
                _cache.Remove(CacheConstants.EtiquettesListKey);
                _logger.LogInformation("Etiquette resolved successfully: {EtiquetteId}", created.Id);
            },
            unexpectedUserMessage: "Une erreur inattendue est survenue lors de l'enregistrement de l'étiquette",
            ct: ct);

    public async Task<Result<bool>> SetForRecipeAsync(int recipeId, IReadOnlyCollection<int> etiquetteIds, CancellationToken ct = default)
    {
        try
        {
            var err = ValidationGuards.RequirePositive(recipeId, "recipe ID");
            if (err != null)
                return Result<bool>.Failure(err);

            var wanted = (etiquetteIds ?? []).Where(id => id > 0).Distinct().ToHashSet();

            var currentResponse = await _supabaseClient.From<RecipeEtiquette>()
                .Where(x => x.RecipeId == recipeId)
                .Get(cancellationToken: ct);
            var current = (currentResponse.Models ?? []).Select(x => x.EtiquetteId).ToHashSet();

            // Only write the difference: an unchanged selection costs no round trip, and a
            // tag that stays attached keeps its original created_at.
            foreach (var toAdd in wanted.Except(current))
            {
                await _supabaseClient.From<RecipeEtiquette>().Insert(
                    new RecipeEtiquette { RecipeId = recipeId, EtiquetteId = toAdd, CreationDate = DateTime.UtcNow },
                    cancellationToken: ct);
            }

            foreach (var toRemove in current.Except(wanted))
            {
                await _supabaseClient.From<RecipeEtiquette>()
                    .Where(x => x.RecipeId == recipeId && x.EtiquetteId == toRemove)
                    .Delete(cancellationToken: ct);
            }

            _logger.LogInformation("Tags updated for recipe {RecipeId}: {Count} tag(s)", recipeId, wanted.Count);
            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Supabase.Postgrest.Exceptions.PostgrestException ex) when (ex.StatusCode is 401 or 403)
        {
            _logger.LogWarning(ex, "Authorization failure while tagging recipe {RecipeId}", recipeId);
            return Result<bool>.Failure(AuthConstants.AuthorizationErrorMessage);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while tagging recipe {RecipeId}", recipeId);
            return Result<bool>.Failure("Network error. Please check your connection.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while tagging recipe {RecipeId}", recipeId);
            return Result<bool>.Failure("Une erreur inattendue est survenue lors de l'enregistrement des étiquettes");
        }
    }

    public Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
        => DeleteCoreAsync(
            id,
            async token => await _supabaseClient.From<Etiquette>().Where(x => x.Id == id).Single(cancellationToken: token),
            // Attachments go away on their own: recettes_etiquettes cascades on delete.
            async token => await _supabaseClient.From<Etiquette>().Where(x => x.Id == id).Delete(cancellationToken: token),
            onSuccess: () =>
            {
                _cache.Remove(CacheConstants.EtiquettesListKey);
                _logger.LogInformation("Etiquette deleted successfully: {EtiquetteId}", id);
            },
            notFoundMessage: $"Étiquette {id} introuvable",
            unexpectedUserMessage: "Une erreur inattendue est survenue lors de la suppression de l'étiquette",
            entityNameForLogging: "etiquette",
            ct: ct);
}
