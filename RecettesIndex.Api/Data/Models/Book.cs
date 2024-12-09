using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace RecettesIndex.Api.Data.Models;

[Table("books")]
public class Book : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = "";

    [Column("author")]
    public int? AuthorId { get; set; }

    [Reference(typeof(Author), true)]
    public Author Author { get; set; } = default!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}