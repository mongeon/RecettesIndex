using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace RecettesIndex.Api.Data.Models;

[Table("authors")]
public class Author : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = "";

    [Column("last_name")]
    public string LastName { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Reference(typeof(Book), useInnerJoin: false, includeInQuery: true)]
    public List<Book> Books { get; set; } = default!;
}