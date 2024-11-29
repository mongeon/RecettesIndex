using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace RecettesIndex.Api.Data.Models;

[Table("recettes")]
public class Recette : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("book_id")]
    public int Book_Id { get; set; }

    [Reference(typeof(Book), true)]
    public Book Book { get; set; }
}