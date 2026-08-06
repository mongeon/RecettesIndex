using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace RecettesIndex.Models;

/// <summary>
/// Represents a tag that can be attached to recipes ("Souper", "Végé", "Rapide"…).
/// </summary>
/// <remarks>
/// Tags are the one model addition required by the redesign: they drive the tag row in
/// the filter bar, the chips on list rows and grid cards, and the mobile filter sheet.
/// The database enforces uniqueness on <c>lower(btrim(name))</c>, so "Souper" and
/// "souper " cannot coexist — see database/migrations/add_etiquettes.sql.
/// </remarks>
[Table("etiquettes")]
public class Etiquette : BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the tag.
    /// </summary>
    [PrimaryKey("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tag label as displayed in the UI.
    /// </summary>
    [Column("name")]
    [Required(ErrorMessage = "Le nom de l'étiquette est requis.")]
    [MaxLength(40, ErrorMessage = "Une étiquette ne peut pas dépasser 40 caractères")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date of the tag record.
    /// </summary>
    [Column("created_at")]
    public DateTime CreationDate { get; set; }
}

/// <summary>
/// Junction table for the many-to-many relationship between recipes and tags.
/// </summary>
/// <remarks>
/// Mirrors <see cref="BookAuthor"/>: a composite primary key over both foreign keys,
/// which is what lets PostgREST resolve the embedded <c>Recipe.Etiquettes</c> collection.
/// </remarks>
[Table("recettes_etiquettes")]
public class RecipeEtiquette : BaseModel
{
    /// <summary>
    /// Gets or sets the ID of the recipe in the relationship.
    /// </summary>
    [Column("recette_id")]
    public int RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the tag in the relationship.
    /// </summary>
    [Column("etiquette_id")]
    public int EtiquetteId { get; set; }

    /// <summary>
    /// Gets or sets the creation date of this association.
    /// </summary>
    [Column("created_at")]
    public DateTime CreationDate { get; set; }
}
