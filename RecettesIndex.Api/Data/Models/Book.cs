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

    [Reference(typeof(Author), includeInQuery: true, useInnerJoin: false)]
    public List<Author> Authors { get; set; } = default!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}