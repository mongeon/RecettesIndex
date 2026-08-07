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
    {
        // Tracks whether this call actually inserted, so the "get" path leaves the cached
        // list — and every consumer already holding it — untouched.
        var inserted = false;

        return CreateCoreAsync(
            new Etiquette { Name = name?.Trim() ?? string.Empty },
            // Not ValidationGuards.RequireNonEmpty: its message is English, and this one
            // surfaces directly in the French tag picker.
            () => string.IsNullOrWhiteSpace(name) ? "Le nom de l'étiquette est requis" : null,
            async token =>
            {
                var trimmed = name!.Trim();

                // The cached list is the same data the tag picker is already showing, so
                // the common "tag already exists" path costs no round trip at all.
                var match = FindByName(await GetAllAsync(token), trimmed);
                if (match != null)
                {
                    return match;
                }

                try
                {
                    var response = await _supabaseClient.From<Etiquette>()
                        .Insert(new Etiquette { Name = trimmed, CreationDate = DateTime.UtcNow }, cancellationToken: token);
                    inserted = true;
                    return response.Models?.FirstOrDefault();
                }
                catch (Supabase.Postgrest.Exceptions.PostgrestException ex) when (ex.StatusCode == 409)
                {
                    // Someone created the same tag between our lookup and this insert. The
                    // unique index on lower(btrim(name)) did its job; honouring the
                    // get-or-create contract means returning their row, not an error.
                    _logger.LogInformation("Etiquette {Name} was created concurrently; reusing it", trimmed);
                    var fresh = await _supabaseClient.From<Etiquette>().Get(cancellationToken: token);
                    return FindByName(fresh.Models ?? [], trimmed);
                }
            },
            onSuccess: created =>
            {
                if (inserted)
                {
                    _cache.Remove(CacheConstants.EtiquettesListKey);
                    _logger.LogInformation("Etiquette created successfully: {EtiquetteId}", created.Id);
                }
            },
            unexpectedUserMessage: "Une erreur inattendue est survenue lors de l'enregistrement de l'étiquette",
            ct: ct);
    }

    /// <summary>
    /// Matches a tag the same way the database unique index does: on the trimmed name,
    /// ignoring case. Keeps C# and SQL from disagreeing about what counts as a duplicate.
    /// </summary>
    private static Etiquette? FindByName(IEnumerable<Etiquette> etiquettes, string trimmedName)
        => etiquettes.FirstOrDefault(
            e => string.Equals(e.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));

    public async Task<Result<bool>> SetForRecipeAsync(int recipeId, IReadOnlyCollection<int> etiquetteIds, CancellationToken ct = default)
    {
        try
        {
            // Not ValidationGuards.RequirePositive: it answers "Invalid recipe ID", and this
            // message can reach the French UI.
            if (recipeId <= 0)
                return Result<bool>.Failure("Identifiant de recette invalide");

            var wanted = (etiquetteIds ?? []).Where(id => id > 0).Distinct().ToHashSet();

            var currentResponse = await _supabaseClient.From<RecipeEtiquette>()
                .Where(x => x.RecipeId == recipeId)
                .Get(cancellationToken: ct);
            var current = (currentResponse.Models ?? []).Select(x => x.EtiquetteId).ToHashSet();

            // Only write the difference: an unchanged selection costs no round trip, and a
            // tag that stays attached keeps its original created_at.
            var toAdd = wanted.Except(current).ToList();
            if (toAdd.Count > 0)
            {
                // One request for the whole batch, as BookAuthorService does — tagging a
                // recipe with six labels is one insert, not six.
                var rows = toAdd.Select(etiquetteId => new RecipeEtiquette
                {
                    RecipeId = recipeId,
                    EtiquetteId = etiquetteId,
                    CreationDate = DateTime.UtcNow
                }).ToList();

                await _supabaseClient.From<RecipeEtiquette>().Insert(rows, cancellationToken: ct);
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
